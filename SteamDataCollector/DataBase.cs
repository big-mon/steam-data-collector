using MySqlConnector;
using SteamDataCollector.Models;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    internal class DataBase
    {
        /// <summary>DB更新</summary>
        /// <param name="app">App情報</param>
        internal static async Task UpdateDatabase(SteamApp app, StoreAPI.CC cc)
        {
            // apps
            await UpdateApp(app);

            if (app.IsSuccess)
            {
                // prices
                _ = UpdatePrices(app);

                if (StoreAPI.CC.us == cc)
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
            using (var conn = new MySqlConnection(TargetManager.ConnString))
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
            using (var conn = new MySqlConnection(TargetManager.ConnString))
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

                // 終了判定
                if (null == app.App || null == app.App.Developers) return;

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
            using (var conn = new MySqlConnection(TargetManager.ConnString))
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

                // 終了判定
                if (null == app.App || null == app.App.Publishers) return;

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
            using (var conn = new MySqlConnection(TargetManager.ConnString))
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

                // 終了判定
                if (null == app.App || null == app.App.Genres) return;

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
            using (var conn = new MySqlConnection(TargetManager.ConnString))
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

                // 終了判定
                if (null == app.App || null == app.App.Languages) return;

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
            using (var conn = new MySqlConnection(TargetManager.ConnString))
            {
                await conn.OpenAsync();

                // 終了判定
                if (null == app.App || null == app.App.PriceOverview) return;

                // クリーニング
                using var cmd1 = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "DELETE FROM prices WHERE `appid` = @appid and `currency` = @currency"
                };
                cmd1.Parameters.AddWithValue("appid", app.AppId);
                cmd1.Parameters.AddWithValue("currency", null == app.App.PriceOverview.Currency);
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
            using (var conn = new MySqlConnection(TargetManager.ConnString))
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

                // 終了判定
                if (null == app.App || null == app.App.Release) return;

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
    }
}