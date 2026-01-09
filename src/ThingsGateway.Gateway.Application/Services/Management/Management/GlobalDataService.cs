//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Gateway.Application;

public class GlobalDataService : IGlobalDataService
{
    public async Task<IEnumerable<SelectedItem>> GetCurrentUserDeviceSelectedItemsAsync(string searchText, int startIndex, int count)
    {
        var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
        var items = devices.WhereIf(!searchText.IsNullOrWhiteSpace(), a => a.Name.Contains(searchText)).Skip(startIndex).Take(count)
           .Select(a => new SelectedItem(a.Name, a.Name));

        return items;
    }

    public Task<QueryData<SelectedItem>> GetCurrentUserDeviceVariableSelectedItemsAsync(string deviceText, string searchText, int startIndex, int count)
    {
        var ret = new QueryData<SelectedItem>()
        {
            IsSorted = false,
            IsFiltered = false,
            IsAdvanceSearch = false,
            IsSearch = !searchText.IsNullOrWhiteSpace()
        };

        if ((!deviceText.IsNullOrWhiteSpace()) && GlobalData.TryGetDeviceRuntime(deviceText, out var device))
        {
            var items = device.ReadOnlyVariableRuntimes.WhereIf(!searchText.IsNullOrWhiteSpace(), a => a.Value.Name.Contains(searchText)).Select(a => a.Value).Skip(startIndex).Take(count)
               .Select(a => new SelectedItem(a.Name, a.Name)).ToList();

            ret.TotalCount = items.Count;
            ret.Items = items;
            return Task.FromResult(ret);
        }
        else
        {
            ret.TotalCount = 0;
            ret.Items = new List<SelectedItem>();
            return Task.FromResult(ret);
        }
    }
}