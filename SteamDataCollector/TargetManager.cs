using MySqlConnector;
using SteamDataCollector.Models;
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
        /// <summary>SteamからAppリストを取得</summary>
        /// <param name="url">APIのURL</param>
        /// <returns>API返却値</returns>
        private static async Task<string> RequestAPI(string url)
        {
            var client = new HttpClient();
            string result = await client.GetStringAsync(url);

            return result;
        }

        #region AllApps

        /// <summary>AppIDのリストを取得</summary>
        /// <returns>AppIDリスト</returns>
        internal static async Task<List<string>> FetchAllAppsList()
        {
            // Appリストを取得
            Task<List<string>> originList = NormalizeApiToList();

            // 除外リストを取得
            Task<List<string>> rejectList = RetrivetRejectList();

            // Appリストへ除外リストを適用
            List<string> list = IgnoreDuplicatedID(await originList, await rejectList);

            // IDリストを返却
            return list;
        }

        /// <summary>API返却値をリストに変換</summary>
        /// <returns>AppIDリスト</returns>
        private static async Task<List<string>> NormalizeApiToList()
        {
            var appList = JsonSerializer.Deserialize<Root>(await RequestAPI(StoreAPI.ALL_APP_API_URL));
            var resList = null != appList ? appList.Applist.Apps.Select(x => x.Appid.ToString()).OrderByDescending(x => int.Parse(x)).ToList() : new List<string>();

            return resList;
        }

        /// <summary>除外対象リストを取得</summary>
        /// <returns>除外対象リスト</returns>
        private static async Task<List<string>> RetrivetRejectList()
        {
            var resList = new List<string>();

            using var conn = new MySqlConnection(StoreAPI.ConnString);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand
            {
                Connection = conn,
                CommandText = "SELECT `appid` FROM apps WHERE `type` IN ('movie', 'demo', 'advertising') OR `update_time` > DATE_SUB(CURRENT_DATE, INTERVAL 7 DAY)"
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
        private static List<string> IgnoreDuplicatedID(IReadOnlyList<string> origin, IReadOnlyList<string> reject)
        {
            HashSet<string> rejectIDs = new HashSet<string>(reject);
            var resList = origin.Where(x => !rejectIDs.Contains(x)).ToList();

            return resList;
        }

        #endregion AllApps

        #region FeaturedApps

        /// <summary>FeaturedAppsIDのリストを取得</summary>
        /// <returns>AppIDリスト</returns>
        internal static async Task<List<string>> FetchFeaturedList()
        {
            // API
            var appList = JsonSerializer.Deserialize<Models.FeaturedList.Root>(await RequestAPI(StoreAPI.FEATURED_API_URL));

            // リストへ変換
            var winList = null != appList && null != appList.FeaturedWin ? appList.FeaturedWin.Select(x => x.Id.ToString()) : new List<string>();
            var macList = null != appList && null != appList.FeaturedMac ? appList.FeaturedMac.Select(x => x.Id.ToString()) : new List<string>();
            var linList = null != appList && null != appList.FeaturedLinux ? appList.FeaturedLinux.Select(x => x.Id.ToString()) : new List<string>();
            var resList = winList.Concat(macList).Concat(linList).Distinct().ToList();

            // IDリストを返却
            return resList;
        }

        #endregion FeaturedApps

        #region FeaturedCategory

        /// <summary>FeaturedCategory内AppsIDのリストを取得</summary>
        /// <returns>AppIDリスト</returns>
        internal static async Task<List<string>> FetchFeaturedCategoryList()
        {
            // API
            var appList = JsonSerializer.Deserialize<Models.FeaturedCategory.Root>(await RequestAPI(StoreAPI.FEATURED_CATEGORIES_API_URL));

            // リストへ変換
            var spList = null != appList && null != appList.Specials && null != appList.Specials.Items ?
                appList.Specials.Items.Select(x => x.Id.ToString()) : new List<string>();
            var csList = null != appList && null != appList.ComingSoon && null != appList.ComingSoon.Items ?
                appList.ComingSoon.Items.Select(x => x.Id.ToString()) : new List<string>();
            var tsList = null != appList && null != appList.TopSellers && null != appList.TopSellers.Items ?
                appList.TopSellers.Items.Select(x => x.Id.ToString()) : new List<string>();
            var nrList = null != appList && null != appList.NewReleases && null != appList.NewReleases.Items ?
                appList.NewReleases.Items.Select(x => x.Id.ToString()) : new List<string>();
            var resList = spList.Concat(csList).Concat(tsList).Concat(nrList).Distinct().ToList();

            // IDリストを返却
            return resList;
        }

        #endregion FeaturedCategory
    }
}