using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamDataCollector.Models.FeaturedList
{
    public class Root
    {
        [JsonPropertyName("featured_win")]
        public List<FeaturedWin> FeaturedWin { get; set; }

        [JsonPropertyName("featured_mac")]
        public List<FeaturedMac> FeaturedMac { get; set; }

        [JsonPropertyName("featured_linux")]
        public List<FeaturedLinux> FeaturedLinux { get; set; }
    }

    public class FeaturedWin
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class FeaturedMac
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class FeaturedLinux
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}