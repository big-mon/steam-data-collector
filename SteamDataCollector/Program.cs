using MySqlConnector;
using Newtonsoft.Json.Linq;
using SteamDataCollector.Models;
using SteamDataCollector.Models.AppList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
            List<int> list = await GetAppList();

            // DBを更新
            await UpdateData(list);
        }

        #region アプリ取得

        /// <summary>
        /// AppIDのリストを取得
        /// </summary>
        /// <returns>AppIDリスト</returns>
        private static async Task<List<int>> GetAppList()
        {
            // Appリストを取得
            var res = GetAllApps();
            var appList = System.Text.Json.JsonSerializer.Deserialize<Root>(await res);

            // IDリストを返却
            return null != appList ? appList.Applist.Apps.Select(x => x.Appid).OrderBy(x => x).ToList() : new List<int>();
        }

        /// <summary>
        /// Steamから全Appリストを取得
        /// </summary>
        /// <returns>API返却値</returns>
        private static async Task<string> GetAllApps()
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync("http://api.steampowered.com/ISteamApps/GetAppList/v0002/?key=STEAMKEY&format=json");

            return result;
        }

        #endregion アプリ取得

        #region App毎更新

        /// <summary>APIをコールしDBへ反映</summary>
        /// <param name="ids">AppIDリスト</param>
        private static async Task UpdateData(IReadOnlyList<int> ids)
        {
            var stopwatch = new Stopwatch();
            var client = new HttpClient();

            foreach (var id in ids)
            {
                foreach (var cc in Enum.GetValues(typeof(CC)))
                {
                    // 処理時間を計測開始
                    stopwatch.Restart();

                    // APIから結果取得
                    var result = await client.GetStringAsync(string.Format("https://store.steampowered.com/api/appdetails/?l=en&appids={0}&cc={1}", id.ToString(), cc.ToString()));

                    // オブジェクト変換
                    var res = JObject.Parse(result).SelectToken(id.ToString());
                    var sa = new SteamApp(id.ToString(), res);

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
                cmd.Parameters.AddWithValue("appid", app.App.AppId);

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
                cmd1.Parameters.AddWithValue("appid", app.App.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Developers)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO developers (`appid`, `name`) VALUES (@appid, @name)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.App.AppId);
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
                cmd1.Parameters.AddWithValue("appid", app.App.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Publishers)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO publishers (`appid`, `name`) VALUES (@appid, @name)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.App.AppId);
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
                cmd1.Parameters.AddWithValue("appid", app.App.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Genres)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO genres (`appid`, `name`, `id`) VALUES (@appid, @name, @id)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.App.AppId);
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
                cmd1.Parameters.AddWithValue("appid", app.App.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                foreach (var item in app.App.Languages)
                {
                    using var cmd2 = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = "INSERT INTO languages (`appid`, `name`) VALUES (@appid, @name)"
                    };
                    cmd2.Parameters.AddWithValue("appid", app.App.AppId);
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
                cmd1.Parameters.AddWithValue("appid", app.App.AppId);
                cmd1.Parameters.AddWithValue("currency", app.App.PriceOverview.Currency);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                using var cmd2 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO prices (`appid`, `currency`, `initial`, `final`, `discount_percent`) VALUES (@appid, @currency, @initial, @final, @discount_percent)"
                };
                cmd2.Parameters.AddWithValue("appid", app.App.AppId);
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
                cmd1.Parameters.AddWithValue("appid", app.App.AppId);
                await cmd1.ExecuteNonQueryAsync();

                // 挿入
                using var cmd2 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO releases (`appid`, `comming_soon`, `date`) VALUES (@appid, @comming_soon, @date)"
                };
                cmd2.Parameters.AddWithValue("appid", app.App.AppId);
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