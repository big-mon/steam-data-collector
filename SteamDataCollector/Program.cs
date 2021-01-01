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
                    var result = await client.GetStringAsync(string.Format("https://store.steampowered.com/api/appdetails/?l=en&appids={0}&cc={1}", 728530, cc.ToString()));

                    // オブジェクト変換
                    var res = JObject.Parse(result).SelectToken("728530");
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
            var connString = $"Server={Properties.Resources.Server};Port={Properties.Resources.Port};Uid={Properties.Resources.UserID};Pwd={Properties.Resources.Password};Database={Properties.Resources.DataBase}";

            using (var conn = new MySqlConnection(connString))
            {
                await conn.OpenAsync();

                // apps
                await UpdateApp(conn, app);

                if (CC.us == cc)
                {
                    // developers
                    await UpdateDevelopers(conn, app);

                    // publishers
                    await UpdatePublishers(conn, app);

                    // genres
                    await UpdateGenres(conn, app);
                }
            }
        }

        /// <summary>Appテーブルを更新</summary>
        /// <param name="conn">接続</param>
        /// <param name="app">App情報</param>
        private static async Task UpdateApp(MySqlConnection conn, SteamApp app)
        {
            using var cmd = new MySqlCommand
            {
                Connection = conn,
                CommandText = "INSERT INTO apps (`appid`, `name`, `type`, `recommendations`, `is_free`) VALUES (@appid, @name, @type, @recommendations, @is_free) ON DUPLICATE KEY UPDATE `name` = @name, `type` = @type, `recommendations` = @recommendations, `is_free` = @is_free, `update_time` = CURRENT_TIMESTAMP"
            };
            cmd.Parameters.AddWithValue("appid", app.App.AppId);
            cmd.Parameters.AddWithValue("name", app.App.Name);
            cmd.Parameters.AddWithValue("type", app.App.Type);
            cmd.Parameters.AddWithValue("recommendations", app.App.Recommendations);
            cmd.Parameters.AddWithValue("is_free", app.App.IsFree);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>developersテーブルを更新</summary>
        /// <param name="conn">接続</param>
        /// <param name="app">App情報</param>
        private static async Task UpdateDevelopers(MySqlConnection conn, SteamApp app)
        {
            using var cmd1 = new MySqlCommand
            {
                Connection = conn,
                CommandText = "DELETE FROM developers WHERE `appid` = @appid"
            };
            cmd1.Parameters.AddWithValue("appid", app.App.AppId);
            await cmd1.ExecuteNonQueryAsync();

            foreach (var item in app.App.Developers)
            {
                using var cmd2 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO developers (`appid`, `name`) VALUES (@appid, @name) ON DUPLICATE KEY UPDATE `name` = @name, `update_time` = CURRENT_TIMESTAMP"
                };
                cmd2.Parameters.AddWithValue("appid", app.App.AppId);
                cmd2.Parameters.AddWithValue("name", item.Name);
                _ = cmd2.ExecuteNonQueryAsync();
            }
        }

        /// <summary>publishersテーブルを更新</summary>
        /// <param name="conn">接続</param>
        /// <param name="app">App情報</param>
        private static async Task UpdatePublishers(MySqlConnection conn, SteamApp app)
        {
            using var cmd1 = new MySqlCommand
            {
                Connection = conn,
                CommandText = "DELETE FROM publishers WHERE `appid` = @appid"
            };
            cmd1.Parameters.AddWithValue("appid", app.App.AppId);
            await cmd1.ExecuteNonQueryAsync();

            foreach (var item in app.App.Publishers)
            {
                using var cmd2 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO publishers (`appid`, `name`) VALUES (@appid, @name) ON DUPLICATE KEY UPDATE `name` = @name, `update_time` = CURRENT_TIMESTAMP"
                };
                cmd2.Parameters.AddWithValue("appid", app.App.AppId);
                cmd2.Parameters.AddWithValue("name", item.Name);
                _ = cmd2.ExecuteNonQueryAsync();
            }
        }

        /// <summary>genresテーブルを更新</summary>
        /// <param name="conn">接続</param>
        /// <param name="app">App情報</param>
        private static async Task UpdateGenres(MySqlConnection conn, SteamApp app)
        {
            using var cmd1 = new MySqlCommand
            {
                Connection = conn,
                CommandText = "DELETE FROM genres WHERE `appid` = @appid"
            };
            cmd1.Parameters.AddWithValue("appid", app.App.AppId);
            await cmd1.ExecuteNonQueryAsync();

            foreach (var item in app.App.Genres)
            {
                using var cmd2 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "INSERT INTO genres (`appid`, `name`, `id`) VALUES (@appid, @name, @id) ON DUPLICATE KEY UPDATE `name` = @name, `id` = @id, `update_time` = CURRENT_TIMESTAMP"
                };
                cmd2.Parameters.AddWithValue("appid", app.App.AppId);
                cmd2.Parameters.AddWithValue("name", item.Name);
                cmd2.Parameters.AddWithValue("id", item.Id);
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