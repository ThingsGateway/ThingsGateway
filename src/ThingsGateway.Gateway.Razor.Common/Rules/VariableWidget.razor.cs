// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Foundation.Common.StringExtension;

namespace ThingsGateway.Gateway.Razor
{
    public partial class VariableWidget
    {
        [Inject]
        IStringLocalizer<ThingsGateway.Gateway.Razor.Common._Imports> Localizer { get; set; }

        [Parameter]
        public VariableNode Node { get; set; }

        private async Task<QueryData<SelectedItem>> OnRedundantDevicesQuery(VirtualizeQueryOption option)
        {
            var ret = new QueryData<SelectedItem>()
            {
                IsSorted = false,
                IsFiltered = false,
                IsAdvanceSearch = false,
                IsSearch = !option.SearchText.IsNullOrWhiteSpace()
            };

            var items = (await GlobalDataService.GetCurrentUserDeviceSelectedItemsAsync(option.SearchText, option.StartIndex, option.Count)).Concat(new List<SelectedItem>() { new SelectedItem("Memory", "Memory") });

            ret.TotalCount = items.Count();
            ret.Items = items;
            return ret;
        }

        private Task OnSelectedItemChanged(SelectedItem item)
        {
            return InvokeAsync(StateHasChanged);
        }


        [Inject]
        IGlobalDataService GlobalDataService { get; set; }

        private Task<QueryData<SelectedItem>> OnRedundantVariablesQuery(VirtualizeQueryOption option)
        {
            return GlobalDataService.GetCurrentUserDeviceVariableSelectedItemsAsync(Node.DeviceText, option.SearchText, option.StartIndex, option.Count);

        }
    }
}