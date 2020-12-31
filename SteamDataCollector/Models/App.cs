namespace SteamDataCollector.Models
{
    /// <summary>
    /// 基本情報
    /// </summary>
    internal class App
    {
        /// <summary>
        /// アプリID
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// アプリ名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// アプリ種別
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// レビュー数
        /// </summary>
        public uint Recommendations { get; }

        /// <summary>
        /// True: 無料配布
        /// </summary>
        public bool IsFree { get; }
    }
}