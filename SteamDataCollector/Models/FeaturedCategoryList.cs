using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamDataCollector.Models.FeaturedCategory
{
    public class Root
    {
        [JsonPropertyName("specials")]
        public Specials Specials { get; set; }

        [JsonPropertyName("coming_soon")]
        public ComingSoon ComingSoon { get; set; }

        [JsonPropertyName("top_sellers")]
        public TopSellers TopSellers { get; set; }

        [JsonPropertyName("new_releases")]
        public NewReleases NewReleases { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class Specials
    {
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
    }

    public class ComingSoon
    {
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
    }

    public class TopSellers
    {
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
    }

    public class NewReleases
    {
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
    }
}