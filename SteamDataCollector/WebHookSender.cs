using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SteamDataCollector
{
    internal class WebHookSender
    {
        /// <summary>HttpClient</summary>
        private static HttpClient Client => new HttpClient();

        /// <summary>WebHook先URL</summary>
        private static string WebHookUrl => Properties.Resources.WebHook;

        /// <summary>WebHookを送信</summary>
        /// <param name="text">送信テキスト</param>
        internal static void SendWebHook(string text)
        {
            var payload = new Payload
            {
                text = text,
            };

            var json = JsonSerializer.Serialize(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _ = Client.PostAsync(WebHookUrl, content).Result;
        }

        public class Payload
        {
            public string text { get; set; }
        }
    }
}