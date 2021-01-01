using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SteamDataCollector.Models.AppList
{
    internal class Root
    {
        [JsonPropertyName("applist")]
        public Applist Applist { get; set; }
    }

    internal class Applist
    {
        [JsonPropertyName("apps")]
        public List<App> Apps { get; set; }
    }

    internal class App
    {
        [JsonPropertyName("appid")]
        public int Appid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}