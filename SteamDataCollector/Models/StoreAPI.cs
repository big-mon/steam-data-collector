﻿namespace SteamDataCollector.Models
{
    /// <summary>ストアAPI関連</summary>
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

        /// <summary>DB接続文字列</summary>
        internal static string ConnString => $"Server={Properties.Resources.Server};Port={Properties.Resources.Port};Uid={Properties.Resources.UserID};Pwd={Properties.Resources.Password};Database={Properties.Resources.DataBase}";

        /// <summary>全Appリスト用のURL</summary>
        internal const string ALL_APP_API_URL = "https://api.steampowered.com/ISteamApps/GetAppList/v0002/?key=STEAMKEY&format=json";

        /// <summary>特集Appリスト用のURL</summary>
        internal const string FEATURED_API_URL = "https://store.steampowered.com/api/featured/";

        /// <summary>特集カテゴリーリスト用のURL</summary>
        internal const string FEATURED_CATEGORIES_API_URL = "https://store.steampowered.com/api/featuredcategories/";

        /// <summary>アプリ詳細API用のURL</summary>
        /// <param name="appid">appid</param>
        /// <param name="area">ストア地域</param>
        /// <returns>URL</returns>
        internal static string AppDetailURL(string appid, CC area) => $"https://store.steampowered.com/api/appdetails/?l=en&appids={appid}&cc={area}";
    }
}