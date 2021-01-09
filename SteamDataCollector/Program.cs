using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamDataCollector
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // 特集Appのリストを取得・更新
            List<string> list = await TargetManager.FetchFeaturedList();
            await AppUpdate.UpdateData(list);

            // 特集カテゴリーAppのリストを取得・更新
            list = await TargetManager.FetchFeaturedCategoryList();
            await AppUpdate.UpdateData(list);

            // 全Appのリストを取得・更新
            list = await TargetManager.FetchAllAppsList();
            await AppUpdate.UpdateData(list);
        }
    }
}