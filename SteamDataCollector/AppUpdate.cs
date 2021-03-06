﻿using Newtonsoft.Json.Linq;
using SteamDataCollector.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    /// <summary>Appデータ更新</summary>
    internal class AppUpdate
    {
        /// <summary>APIをコールしDBへ反映</summary>
        /// <param name="ids">AppIDリスト</param>
        internal static async Task UpdateData(IReadOnlyList<string> ids)
        {
            var stopwatch = new Stopwatch();
            var client = new HttpClient();

            var count = 0;
            foreach (var id in ids)
            {
                count += 1;
                var isSkip = false;

                foreach (StoreAPI.CC cc in Enum.GetValues(typeof(StoreAPI.CC)))
                {
                    if (isSkip) continue;

                    // 処理時間を計測開始
                    stopwatch.Restart();

                    var title = "";
                    try
                    {
                        // APIから結果取得
                        var result = await client.GetStringAsync(StoreAPI.AppDetailURL(id, cc));
                        if (string.IsNullOrWhiteSpace(result)) continue;

                        // オブジェクト変換
                        var res = JObject.Parse(result).SelectToken(id.ToString());
                        var sa = new SteamApp(id, res);

                        // 取得失敗の場合、地域別の取得をスキップ
                        isSkip = !sa.IsSuccess;

                        title = null == sa.App ? "" : sa.App.Name;
                        Console.WriteLine($"{count,7}/{ids.Count}-{cc} : {sa.AppId,7} {title}");

                        // DB反映
                        await DataBase.UpdateDatabase(sa, cc);
                    }
                    catch (Exception e)
                    {
                        WebHookSender.SendWebHook($"{id + "-" + title} : {e.Message}");
                    }

                    // APIリミット回避のため待機
                    stopwatch.Stop();
                    WaitSleep((int)stopwatch.ElapsedMilliseconds);
                }
            }
        }

        /// <summary>指定時間を強制的に経過させる</summary>
        /// <param name="elapsed">これまでの経過時間ミリ秒</param>
        private static void WaitSleep(int elapsed)
        {
            var gap = int.Parse(Properties.Resources.SleepMilSec) - elapsed;
            if (gap > 0) Thread.Sleep(gap);
        }
    }
}