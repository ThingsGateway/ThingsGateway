//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Demo
{
    public partial class ModbusMaster
    {
        private void OnEditClick(ChannelData channelData)
        {
            ChannelData = channelData;
            if (channelData != null)
            {
                OnChannelChanged(ChannelData);
            }
            else
            {
                _plc?.Dispose();
                _plc = null;
                AdapterDebugPage.Plc = null;
            }
        }

        private async Task OnConnectClick()
        {
            if (ChannelData != null)
            {
                try
                {
                    await ChannelData.Channel.ConnectAsync(_plc.ConnectTimeout, default);
                }
                catch (Exception ex)
                {
                    ChannelData.Channel.Logger.Exception(ex);
                }
            }
        }

        private ChannelData ChannelData { get; set; }

        private AdapterDebugPage AdapterDebugPage { get; set; }
        private ThingsGateway.Foundation.Modbus.ModbusMaster _plc;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                if (ChannelData != null)
                    OnChannelChanged(ChannelData);
            }
        }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        private string LogPath;

        private void OnChannelChanged(ChannelData channelData)
        {
            _plc = new ThingsGateway.Foundation.Modbus.ModbusMaster(channelData.Channel);
            AdapterDebugPage.Plc = _plc;
            LogPath = channelData.Id.GetDebugLogPath();
        }
    }
}