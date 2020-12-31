using System.Collections.Generic;

namespace SteamDataCollector.Models
{
    /// <summary>
    /// Steam Game Data
    /// </summary>
    internal class SteamApp
    {
        /// <summary>
        /// 基本情報
        /// </summary>
        public App app { get; }

        /// <summary>
        /// デベロッパー情報
        /// </summary>
        public List<Developer> developers { get; }

        /// <summary>
        /// パブリッシャー情報
        /// </summary>
        public List<Publisher> publishers { get; }

        /// <summary>
        /// 価格情報
        /// </summary>
        public List<Price> prices { get; }

        /// <summary>
        /// ジャンル情報
        /// </summary>
        public List<Genre> genres { get; }

        /// <summary>
        /// 発売日情報
        /// </summary>
        public Release release { get; }
    }
}