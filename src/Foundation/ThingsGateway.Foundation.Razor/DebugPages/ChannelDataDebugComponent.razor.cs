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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

using ThingsGateway.Foundation;
using ThingsGateway.Razor;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class ChannelDataDebugComponent : ComponentBase
{
    [Parameter]
    public string ClassString { get; set; }

    [Parameter]
    public EventCallback<ChannelData> OnConnectClick { get; set; }

    [Parameter]
    public EventCallback<ChannelData> OnConfimClick { get; set; }

    [Parameter]
    public EventCallback OnDisConnectClick { get; set; }

    private ChannelData? Model { get; set; } = new();

    private IEnumerable<SelectedItem> ChannelDataItems { get; set; }

    [Inject]
    private IStringLocalizer<ChannelDataDebugComponent> Localizer { get; set; }

    [Inject]
    private ToastService ToastService { get; set; }

    public Task ValidSubmit(EditContext editContext)
    {
        CheckInput(Model);
        return Task.CompletedTask;
    }

    private void CheckInput(ChannelData input)
    {
        try
        {
            if (input.ChannelType == ChannelTypeEnum.TcpClient)
            {
                if (string.IsNullOrEmpty(input.RemoteUrl))
                    throw new(Localizer["RemoteUrlNotNull"]);
            }
            else if (input.ChannelType == ChannelTypeEnum.TcpService)
            {
                if (string.IsNullOrEmpty(input.BindUrl))
                    throw new(Localizer["BindUrlNotNull"]);
            }
            else if (input.ChannelType == ChannelTypeEnum.UdpSession)
            {
                if (string.IsNullOrEmpty(input.BindUrl) && string.IsNullOrEmpty(input.RemoteUrl))
                    throw new(Localizer["BindUrlOrRemoteUrlNotNull"]);
            }
            else if (input.ChannelType == ChannelTypeEnum.SerialPort)
            {
                if (string.IsNullOrEmpty(input.PortName))
                    throw new(Localizer["PortNameNotNull"]);
                if (input.BaudRate == null)
                    throw new(Localizer["BaudRateNotNull"]);
                if (input.DataBits == null)
                    throw new(Localizer["DataBitsNotNull"]);
                if (input.Parity == null)
                    throw new(Localizer["ParityNotNull"]);
                if (input.StopBits == null)
                    throw new(Localizer["StopBitsNotNull"]);
            }
            else
            {
                throw new(Localizer["NotOther"]);
            }
        }
        catch (Exception ex)
        {
            ToastService.Warn(ex);
        }
    }

    private async Task DisconnectClick()
    {
        if (Model?.Channel != null)
        {
            try
            {
                await Model.Channel.CloseAsync(DefaultResource.Localizer["ProactivelyDisconnect", nameof(DisconnectClick)]);
                if (OnDisConnectClick.HasDelegate)
                    await OnDisConnectClick.InvokeAsync();
            }
            catch (Exception ex)
            {
                Model.Channel.Logger?.Exception(ex);
            }
        }
    }

    private async Task ConfimClick()
    {
        try
        {
            ChannelData.CreateChannel(Model);
            if (OnConfimClick.HasDelegate)
                await OnConfimClick.InvokeAsync(Model);
        }
        catch (Exception ex)
        {
            Model.Channel?.Logger?.Exception(ex);
        }
    }

    private async Task ConnectClick()
    {
        if (Model != null)
        {
            try
            {
                if (Model.Channel != null)
                    if (OnConnectClick.HasDelegate)
                        await OnConnectClick.InvokeAsync(Model);
            }
            catch (Exception ex)
            {
                Model.Channel.Logger?.Exception(ex);
            }
        }
    }
}
