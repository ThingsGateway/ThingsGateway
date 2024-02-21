//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Components;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Demo
{
    public partial class ChannelEditPage
    {
        [Parameter]
        public ChannelData ChannelData
        {
            get;
            set;
        }

        private async Task HandleSaveClick(ChannelData channelData)
        {
            var data = channelData.Map();
            data.Name = $"{data.ChannelType}-{data.Name}";
            await ClosePopupAsync(data);
        }

        private void HandleChannelChanged(ThingsGateway.Foundation.ChannelTypeEnum channelTypeEnum)
        {
            ChannelData.TouchSocketConfig = new();
            ChannelData.ChannelType = channelTypeEnum;
        }

        protected override Task OnParametersSetAsync()
        {
            ChannelData ??= new();
            ChannelData.Name = ChannelData.Name?.Replace($"{ChannelData.ChannelType}-", "");
            return base.OnParametersSetAsync();
        }
    }
}