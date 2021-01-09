using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // 全Appのリストを取得
            list = await TargetManager.FetchAllAppsList();

            // DBを更新
            await AppUpdate.UpdateData(list);
        }
    }
}