using System;

namespace SteamDataCollector.Models
{
    /// <summary>
    /// リリース日
    /// </summary>
    internal class Release
    {
        /// <summary>
        /// リリース日
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// True: 未発売
        /// </summary>
        public bool IsUnRelease { get; }
    }
}