namespace SteamDataCollector.Models
{
    /// <summary>
    /// 価格情報
    /// </summary>
    internal class Price
    {
        /// <summary>
        /// 通貨
        /// </summary>
        public string Currency { get; }

        /// <summary>
        /// 通常価格
        /// </summary>
        public decimal Initial { get; }

        /// <summary>
        /// 現在価格
        /// </summary>
        public decimal Final { get; }

        /// <summary>
        /// 割引率
        /// </summary>
        public sbyte DiscountPercent { get; }
    }
}