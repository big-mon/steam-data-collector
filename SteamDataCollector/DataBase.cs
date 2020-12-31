using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    class DataBase
    {
        /// <summary>
        /// DB接続文字列
        /// </summary>
        private static string ConnString =>
            $"Server={Properties.Resources.Server};Port={Properties.Resources.Port};Uid={Properties.Resources.UserID};Pwd={Properties.Resources.Password};Database={Properties.Resources.DataBase}";

        public static async void TestConnAsync()
        {
            using (var conn = new MySqlConnection(ConnString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand("SELECT * FROM apps", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0));
                    }
                }

            }
        }
    }
}
