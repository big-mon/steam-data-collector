using MySqlConnector;
using Newtonsoft.Json.Linq;
using SteamDataCollector.Models;
using SteamDataCollector.Models.AppList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    internal class Program
    {
        /// <summary>ストア地域</summary>
        private enum CC
        {
            /// <summary>アメリカ</summary>
            us,

            /// <summary>日本</summary>
            jp
        }

        /// <summary>DB接続文字列</summary>
        private static string ConnString => $"Server={Properties.Resources.Server};Port={Properties.Resources.Port};Uid={Properties.Resources.UserID};Pwd={Properties.Resources.Password};Database={Properties.Resources.DataBase}";

        private static async Task Main(string[] args)
        {
            // 全Appのリストを取得
            List<string> list = await GetTargetList();

            // DBを更新
            await UpdateData(list);
        }

        #region アプリ取得

        /// <summary>AppIDのリストを取得</summary>
        /// <returns>AppIDリスト</returns>
        private static async Task<List<string>> GetTargetList()
        {
            // Appリストを取得
            Task<List<string>> originList = GetAllAppList();

            // 除外リストを取得
            Task<List<string>> rejectList = GetRejectList();

            // Appリストへ除外リストを適用
            List<string> list = GetFormattedTargetList(await originList, await rejectList);

            // IDリストを返却
            return list;
        }

        /// <summary>Steamから全Appリストを取得</summary>
        /// <returns>API返却値</returns>
        private static async Task<string> GetAllApps()
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync("http://api.steampowered.com/ISteamApps/GetAppList/v0002/?key=STEAMKEY&format=json");

            return result;
        }

        /// <summary>API返却値をリストに変換</summary>
        /// <returns>AppIDリスト</returns>
        private static async Task<List<string>> GetAllAppList()
        {
            var appList = JsonSerializer.Deserialize<Root>(await GetAllApps());
            var resList = null != appList ? appList.Applist.Apps.Select(x => x.Appid.ToString()).OrderBy(x => int.Parse(x)).ToList() : new List<string>();

            return resList;
        }

        /// <summary>除外対象リストを取得</summary>
        /// <returns>除外対象リスト</returns>
        private static async Task<List<string>> GetRejectList()
        {
            var resList = new List<string>();

            using var conn = new MySqlConnection(ConnString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand
            {
                Connection = conn,
                CommandText = "SELECT `appid` FROM apps WHERE `type` NOT IN ('game', 'dlc')"
            };
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                resList.Add(reader.GetUInt32(0).ToString());
            }

            return resList;
        }

        /// <summary>全Appリストに除外リストを適用</summary>
        /// <param name="origin">全Appリスト</param>
        /// <param name="reject">除外リスト</param>
        /// <returns>除外後リスト</returns>
        private static List<string> GetFormattedTargetList(IReadOnlyList<string> origin, IReadOnlyList<string> reject)
        {
            HashSet<string> rejectIDs = new HashSet<string>(reject);
            var resList = origin.Where(x => !rejectIDs.Contains(x)).ToList();

            return resList;
        }

        #endregion アプリ取得

        #region App毎更新

        /// <summary>APIをコールしDBへ反映</summary>
        /// <param name="ids">AppIDリスト</param>
        private static async Task UpdateData(IReadOnlyList<string> ids)
        {
            var stopwatch = new Stopwatch();
            var client = new HttpClient();

            var count = 0;
            foreach (var id in ids)
            {
                count += 1;
                var isSkip = false;

                foreach (var cc in Enum.GetValues(typeof(CC)))
                {
                    if (isSkip) continue;

                    // 処理時間を計測開始
                    stopwatch.Restart();

                    // APIから結果取得
                    var result = await client.GetStringAsync(string.Format("https://store.steampowered.com/api/appdetails/?l=en&appids={0}&cc={1}", id, cc.ToString()));

                    // オブジェクト変換
                    var res = JObject.Parse(result).SelectToken(id.ToString());
                    var sa = new SteamApp(id, res);

                    // 取得失敗の場合、地域別の取得をスキップ
                    isSkip = !sa.IsSuccess;

                    var title = null == sa.App ? "" : sa.App.Name;
                    Console.WriteLine($"{count, 7}/{ids.Count}-{cc} : {sa.AppId, 7} {title}");

                    // DB反映
                    await UpdateDatabase(sa, (CC)cc);

                    // APIリミット回避のため待機
                    stopwatch.Stop();
                    WaitSleep((int)stopwatch.ElapsedMilliseconds);
                }
            }
        }

        #region DB更新

        /// <summary>DB更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdateDatabase(SteamApp app, CC cc)
        {
            // apps
            await UpdateApp(app);

            if (app.IsSuccess)
            {
                // prices
                _ = UpdatePrices(app);

                if (CC.us == cc)
                {
                    // developers
                    _ = UpdateDevelopers(app);

                    // publishers
                    _ = UpdatePublishers(app);

                    // genres
                    _ = UpdateGenres(app);

                    // languages
                    _ = UpdateLanguages(app);

                    // releases
                    _ = UpdateReleases(app);
                }
            }
        }

        /// <summary>Appテーブルを更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdateApp(SteamApp app)
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                await conn.OpenAsync();

                using var cmd = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO apps (`appid`, `name`, `type`, `recommendations`, `is_free`) VALUES (@appid, @name, @type, @recommendations, @is_free) ON DUPLICATE KEY UPDATE `name` = @name, `type` = @type, `recommendations` = @recommendations, `is_free` = @is_free, `update_time` = CURRENT_TIMESTAMP"
                };
                cmd.Parameters.AddWithValue("appid", app.AppId);

                if (app.IsSuccess)
                {
                    cmd.Parameters.AddWithValue("name", app.App.Name);
                    cmd.Parameters.AddWithValue("type", app.App.Type);
                    cmd.Parameters.AddWithValue("recommendations", app.App.Recommendations);
                    cmd.Parameters.AddWithValue("is_free", app.App.IsFree);
                }
                else
                {
                    cmd.Parameters.AddWithValue("name", "");
                    cmd.Parameters.AddWithValue("type", "");
                    cmd.Parameters.AddWithValue("recommendations", 0);
                    cmd.Parameters.AddWithValue("is_free", false);
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>developersテーブルを更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdateDevelopers(SteamApp app)
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                await conn.OpenAsync();

                // クリーニング
                using var cmd1 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "DELETE FROM developers WHERE `appid` = @appid"
                };
                cmd1.Parameters.AddWithValue("appid", app.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Developers)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO developers (`appid`, `name`) VALUES (@appid, @name)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.AppId);
                    cmd2.Parameters.AddWithValue("name", item.Name);
                    _ = cmd2.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>publishersテーブルを更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdatePublishers(SteamApp app)
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                await conn.OpenAsync();

                // クリーニング
                using var cmd1 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "DELETE FROM publishers WHERE `appid` = @appid"
                };
                cmd1.Parameters.AddWithValue("appid", app.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Publishers)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO publishers (`appid`, `name`) VALUES (@appid, @name)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.AppId);
                    cmd2.Parameters.AddWithValue("name", item.Name);
                    _ = cmd2.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>genresテーブルを更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdateGenres(SteamApp app)
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                await conn.OpenAsync();

                // クリーニング
                using var cmd1 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "DELETE FROM genres WHERE `appid` = @appid"
                };
                cmd1.Parameters.AddWithValue("appid", app.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Genres)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO genres (`appid`, `name`, `id`) VALUES (@appid, @name, @id)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.AppId);
                    cmd2.Parameters.AddWithValue("name", item.Name);
                    cmd2.Parameters.AddWithValue("id", item.Id);
                    _ = cmd2.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>languagesテーブルを更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdateLanguages(SteamApp app)
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                await conn.OpenAsync();

                // クリーニング
                using var cmd1 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "DELETE FROM languages WHERE `appid` = @appid"
                };
                cmd1.Parameters.AddWithValue("appid", app.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Languages)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO languages (`appid`, `name`) VALUES (@appid, @name)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.AppId);
                    cmd2.Parameters.AddWithValue("name", item);
                    _ = cmd2.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>pricesテーブルを更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdatePrices(SteamApp app)
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                await conn.OpenAsync();

                // クリーニング
                using var cmd1 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "DELETE FROM prices WHERE `appid` = @appid and `currency` = @currency"
                };
                cmd1.Parameters.AddWithValue("appid", app.AppId);
                cmd1.Parameters.AddWithValue("currency", app.App.PriceOverview.Currency);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                using var cmd2 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO prices (`appid`, `currency`, `initial`, `final`, `discount_percent`) VALUES (@appid, @currency, @initial, @final, @discount_percent)"
                };
                cmd2.Parameters.AddWithValue("appid", app.AppId);
                cmd2.Parameters.AddWithValue("currency", app.App.PriceOverview.Currency);
                cmd2.Parameters.AddWithValue("initial", app.App.PriceOverview.Initial);
                cmd2.Parameters.AddWithValue("final", app.App.PriceOverview.Final);
                cmd2.Parameters.AddWithValue("discount_percent", app.App.PriceOverview.DiscountPercent);
                _ = cmd2.ExecuteNonQueryAsync();
            }
        }

        /// <summary>releasesテーブルを更新</summary>
        /// <param name="app">App情報</param>
        private static async Task UpdateReleases(SteamApp app)
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                await conn.OpenAsync();

                // クリーニング
                using var cmd1 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "DELETE FROM releases WHERE `appid` = @appid"
                };
                cmd1.Parameters.AddWithValue("appid", app.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                using var cmd2 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO releases (`appid`, `comming_soon`, `date`) VALUES (@appid, @comming_soon, @date)"
                };
                cmd2.Parameters.AddWithValue("appid", app.AppId);
                cmd2.Parameters.AddWithValue("comming_soon", app.App.Release.IsUnRelease);
                cmd2.Parameters.AddWithValue("date", app.App.Release.Date);
                _ = cmd2.ExecuteNonQueryAsync();
            }
        }

        #endregion DB更新

        /// <summary>指定時間を強制的に経過させる</summary>
        /// <param name="elapsed">これまでの経過時間ミリ秒</param>
        private static void WaitSleep(int elapsed)
        {
            var gap = int.Parse(Properties.Resources.SleepMilSec) - elapsed;
            if (gap > 0) Thread.Sleep(gap);
        }

        #endregion App毎更新
    }
}