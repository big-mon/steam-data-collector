namespace SteamDataCollector.Models
{
    internal class StoreAPI
    {
        /// <summary>ストア地域</summary>
        internal enum CC
        {
            /// <summary>アメリカ</summary>
            us,

            /// <summary>日本</summary>
            jp
        }

        /// <summary>アプリ詳細API用のURL</summary>
        /// <param name="appid">appid</param>
        /// <param name="area">ストア地域</param>
        /// <returns>URL</returns>
        internal static string GetAppDetailURL(string appid, CC area) => $"https://store.steampowered.com/api/appdetails/?l=en&appids={appid}&cc={area}";
    }
}