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
using TouchSocket.Sockets;

namespace ThingsGateway.Demo
{
    public partial class DemoChannelPage
    {
        private ChannelData ChannelData { get; set; }

        private string ChannelName { get; set; }

        private async Task HandleSelectClick((ChannelData Item, bool Selected) item)
        {
            if (item.Selected)
            {
                if (ChannelData != item.Item)
                {
                    ChannelData?.Channel?.Close();
                    ChannelData = item.Item;
                    await OnEditClick.InvokeAsync(ChannelData);
                }
            }
        }

        private async Task HandleChannelNameChanged(string channelName)
        {
            ChannelName = channelName;
            var channelData = ChannelConfigs.Default.ChannelDatas.FirstOrDefault(a => a.Name == channelName);
            if (channelData != null)
            {
                if (ChannelData != channelData)
                {
                    ChannelData?.Channel?.Close();
                    ChannelData = channelData;
                    ChannelConfigs.Default.Save(true, out _);
                    await OnEditClick.InvokeAsync(ChannelData);
                }
            }
        }

        private async Task HandleDeleteClick()
        {
            try
            {
                ChannelData?.Channel?.Close();
                ChannelConfigs.Default.ChannelDatas.Remove(ChannelData);
                ChannelData = null;
                ChannelName = null;
                ChannelConfigs.Default.Save(true, out _);
                await InvokeStateHasChangedAsync();
                await OnEditClick.InvokeAsync(ChannelData);
            }
            finally
            {
            }
        }

        private async Task HandleAddClick()
        {
            var data = await PopupService.OpenAsync(typeof(ChannelEditPage), new Dictionary<string, object?>()
            {
            });
            if (data != null)
            {
                var channelData = (ChannelData)data;
                ChannelData.CreateChannel(channelData);
                ChannelConfigs.Default.ChannelDatas.Add(channelData);
                ChannelConfigs.Default.Save(true, out _);
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task HandleEditClick()
        {
            var data = await PopupService.OpenAsync(typeof(ChannelEditPage), new Dictionary<string, object?>()
        {
           { nameof(ChannelEditPage.ChannelData),ChannelData.Map() }
        });

            if (data != null)
            {
                ChannelData?.Channel?.Close();
                ChannelConfigs.Default.ChannelDatas.Remove(ChannelData);
                ChannelData = (ChannelData)data;
                ChannelData.CreateChannel(ChannelData);
                ChannelConfigs.Default.ChannelDatas.Add(ChannelData);
                ChannelConfigs.Default.Save(true, out _);
                await HandleChannelNameChanged(ChannelData.Name);
                await InvokeAsync(StateHasChanged);
                await OnEditClick.InvokeAsync(ChannelData);
                await InvokeAsync(StateHasChanged);
            }
        }

        private void OnDisconnectClick()
        {
            if (ChannelData != null)
            {
                try
                {
                    ChannelData.Channel.Close(string.Format(FoundationConst.ProactivelyDisconnect, nameof(OnDisconnectClick)));
                }
                catch (Exception ex)
                {
                    ChannelData.Channel.Logger.Exception(ex);
                }
            }
        }

        [Parameter, EditorRequired]
        public EventCallback<ChannelData> OnEditClick { get; set; }

        [Parameter]
        public EventCallback OnConnectClick { get; set; }
    }
}