using MySqlConnector;
using SteamDataCollector.Models.AppList;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    /// <summary>アプリ取得</summary>
    internal class TargetManager
    {
        /// <summary>DB接続文字列</summary>
        private static string ConnString => $"Server={Properties.Resources.Server};Port={Properties.Resources.Port};Uid={Properties.Resources.UserID};Pwd={Properties.Resources.Password};Database={Properties.Resources.DataBase}";

        /// <summary>AppIDのリストを取得</summary>
        /// <returns>AppIDリスト</returns>
        internal static async Task<List<string>> GetTargetList()
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
                CommandText = "SELECT `appid` FROM apps WHERE `type` NOT IN ('game', 'dlc') OR update_time > DATE_SUB(CURRENT_DATE, INTERVAL 7 DAY)"
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
    }
}