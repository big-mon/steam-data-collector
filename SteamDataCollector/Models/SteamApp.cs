using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SteamDataCollector.Models
{
    /// <summary>Steam Game Data</summary>
    internal class SteamApp
    {
        #region Prop

        /// <summary>API結果</summary>
        public bool IsSuccess { get; }

        /// <summary>アプリID</summary>
        public string AppId { get; }

        /// <summary>基本情報</summary>
        public App App { get; }

        #endregion Prop

        /// <summary>コンストラクタ</summary>
        /// <param name="id">AppID</param>
        /// <param name="json">JSON</param>
        internal SteamApp(string id, JToken json)
        {
            if (null == json) return;

            AppId = id;
            IsSuccess = (bool)json["success"];
            if (IsSuccess) App = new App(id, json["data"]);
        }
    }

    /// <summary>基本情報</summary>
    internal class App
    {
        #region Prop

        /// <summary>アプリID</summary>
        public string AppId { get; }

        /// <summary>アプリ種別</summary>
        public string Type { get; }

        /// <summary>アプリ名称</summary>
        public string Name { get; }

        /// <summary>True: 無料配布</summary>
        public bool IsFree { get; }

        /// <summary>対応言語</summary>
        public List<string> Languages { get; }

        /// <summary>デベロッパー情報</summary>
        public List<Developer> Developers { get; }

        /// <summary>パブリッシャー情報</summary>
        public List<Publisher> Publishers { get; }

        /// <summary>価格情報</summary>
        public Price PriceOverview { get; }

        /// <summary>ジャンル情報</summary>
        public List<Genre> Genres { get; }

        /// <summary>レビュー数</summary>
        public uint Recommendations { get; }

        /// <summary>発売日情報</summary>
        public Release Release { get; }

        #endregion Prop

        /// <summary>コンストラクタ</summary>
        /// <param name="id">AppID</param>
        /// <param name="json">JSON</param>
        internal App(string id, JToken json)
        {
            if (null == json) return;

            AppId = id;
            Type = json["type"].ToString();
            Name = json["name"].ToString();
            IsFree = (bool)json["is_free"];

            if (null != json["supported_languages"])
            {
                Languages = new Regex("<.*?</.*?>", RegexOptions.Singleline)
                .Replace(json["supported_languages"].ToString().Replace("languages with full audio support", ""), "")
                .Split(",")
                .ToList();
            }

            if (null != json["developers"])
            {
                Developers = json["developers"]
                .Select(x => new Developer(x.ToString()))
                .ToList();
            }
            
            if (null != json["publishers"])
            {
                Publishers = json["publishers"]
                .Select(x => new Publisher(x.ToString()))
                .ToList();
            }
            
            if (!IsFree) PriceOverview = new Price(json["price_overview"]);

            if (null != json["genres"])
            {
                Genres = json["genres"]
                .Select(x => new Genre(x))
                .ToList();
            }
            
            Recommendations = null != json["recommendations"]
                && uint.TryParse(json["recommendations"]["total"].ToString(), out uint i) ? i : 0;

            Release = new Release(json["release_date"]);
        }
    }

    /// <summary>デベロッパー</summary>
    internal class Developer
    {
        /// <summary>デベロッパー名称</summary>
        public string Name { get; }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">名称</param>
        internal Developer(string name)
        {
            Name = name;
        }
    }

    /// <summary>パブリッシャー</summary>
    internal class Publisher
    {
        /// <summary>パブリッシャー名称</summary>
        public string Name { get; }

        /// <summary>コンストラクタ</summary>
        /// <param name="name">名称</param>
        internal Publisher(string name)
        {
            Name = name;
        }
    }

    /// <summary>価格情報</summary>
    internal class Price
    {
        #region Prop

        /// <summary>通貨</summary>
        public string Currency { get; }

        /// <summary>通常価格</summary>
        public decimal Initial { get; }

        /// <summary>現在価格</summary>
        public decimal Final { get; }

        /// <summary>割引率</summary>
        public sbyte DiscountPercent { get; }

        #endregion Prop

        /// <summary>コンストラクタ</summary>
        /// <param name="json">JSON</param>
        internal Price(JToken json)
        {
            if (null == json) return;

            Currency = json["currency"].ToString();
            Initial = decimal.Parse(json["initial"].ToString()) / 100;
            Final = decimal.Parse(json["final"].ToString()) / 100;
            DiscountPercent = sbyte.Parse(json["discount_percent"].ToString());
        }
    }

    /// <summary>ジャンル</summary>
    internal class Genre
    {
        /// <summary>ジャンルID</summary>
        public ushort Id { get; }

        /// <summary>ジャンル名称</summary>
        public string Name { get; }

        /// <summary>コンストラクタ</summary>
        /// <param name="json">JSON</param>
        internal Genre(JToken json)
        {
            if (null == json) return;

            Id = ushort.Parse(json["id"].ToString());
            Name = json["description"].ToString();
        }
    }

    /// <summary>リリース日</summary>
    internal class Release
    {
        /// <summary>リリース日</summary>
        public string Date { get; }

        /// <summary>True: 未発売</summary>
        public bool IsUnRelease { get; }

        /// <summary>コンストラクタ</summary>
        /// <param name="json">JSON</param>
        internal Release(JToken json)
        {
            if (null == json) return;

            Date = json["date"].ToString();
            IsUnRelease = (bool)json["coming_soon"];
        }
    }
}