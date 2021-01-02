using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // 全Appのリストを取得
            List<string> list = await TargetManager.GetTargetList();

            // DBを更新
            await AppUpdate.UpdateData(list);
        }
    }
}