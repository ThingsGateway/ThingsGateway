
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------





using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using ThingsGateway.Foundation;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Debug;

public partial class ChannelDataDebugComponent : ComponentBase
{
    [Parameter]
    public string ClassString { get; set; }

    [Parameter]
    public EventCallback OnConnectClick { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<ChannelData> OnEditClick { get; set; }

    private ChannelData? ChannelData { get; set; }

    private IEnumerable<SelectedItem> ChannelDataItems { get; set; }

    private long ChannelId { get; set; }

    [Inject]
    private DialogService DialogService { get; set; }

#if DriverDebug

    [Inject]
    private IStringLocalizer<ChannelDataDebugComponent> Localizer { get; set; }

#else

    [Inject]
    private IStringLocalizer<Gateway.Application.Channel> Localizer { get; set; }

#endif

    [Inject]
    private ToastService ToastService { get; set; }

    private void CheckInput(ChannelData input)
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

    public static IEnumerable<SelectedItem> BuildChannelSelectList(IEnumerable<ChannelData> items)
    {
        var data = items
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Name)
            {
            }
        ).ToList();
        return data;
    }

    protected override Task OnParametersSetAsync()
    {
        Refresh();
        return base.OnParametersSetAsync();
    }

    private void Refresh()
    {
        ChannelDataItems = BuildChannelSelectList(ChannelConfig.Default.ChannelDatas);
    }

    private async Task HandleAddClick()
    {
        ChannelData channel = new();
        var op = new DialogOption()
        {
            Title = Localizer["SaveChannel"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelDataEditComponent>(new Dictionary<string, object?>
        {
            [nameof(ChannelDataEditComponent.Model)] = channel,
            [nameof(ChannelDataEditComponent.OnValidSubmit)] = async () =>
            {
                CheckInput(channel);
                ChannelData.CreateChannel(channel);
                ChannelConfig.Default.ChannelDatas.Add(channel);
                ChannelConfig.Default.Save(true, out _);
                Refresh();
                await InvokeAsync(StateHasChanged);
            },
        });
        await DialogService.Show(op);
    }

    private async Task HandleDeleteClick()
    {
        ChannelData?.Channel?.Close();
        ChannelConfig.Default.ChannelDatas.Remove(ChannelData);
        ChannelData = null;
        ChannelConfig.Default.Save(true, out _);
        Refresh();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleEditClick()
    {
        var op = new DialogOption()
        {
            Title = Localizer["SaveChannel"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelDataEditComponent>(new Dictionary<string, object?>
        {
            [nameof(ChannelDataEditComponent.Model)] = ChannelData,
            [nameof(ChannelDataEditComponent.OnValidSubmit)] = async () =>
            {
                CheckInput(ChannelData);
                await SetChannelData(ChannelData);
                ChannelConfig.Default.Save(true, out _);
                Refresh();
                await InvokeAsync(StateHasChanged);
            },
        });
        await DialogService.Show(op);
        Refresh();
    }

    private void OnDisconnectClick()
    {
        if (ChannelData != null)
        {
            try
            {
                ChannelData.Channel.Close(DefaultResource.Localizer["ProactivelyDisconnect", nameof(OnDisconnectClick)]);
            }
            catch (Exception ex)
            {
                ChannelData.Channel.Logger.Exception(ex);
            }
        }
    }

    private async Task ConnectClick()
    {
        if (ChannelData != null)
        {
            try
            {
                if (OnConnectClick.HasDelegate)
                    await OnConnectClick.InvokeAsync();
            }
            catch (ObjectDisposedException)
            {
                await SetChannelData(ChannelData);
                try
                {
                    if (OnConnectClick.HasDelegate)
                        await OnConnectClick.InvokeAsync();
                }
                catch (Exception ex)
                {
                    ChannelData.Channel.Logger.Exception(ex);
                }
            }
            catch (Exception ex)
            {
                ChannelData.Channel.Logger.Exception(ex);
            }
        }
    }

    private async Task OnSelectedItemChanged(SelectedItem item)
    {
        var channelData = ChannelConfig.Default.ChannelDatas.FirstOrDefault(a => a.Id == item.Value.ToLong());
        if (ChannelData != channelData)
        {
            await SetChannelData(channelData);
        }
    }

    [Inject]
    private IDispatchService<ChannelData>? DispatchService { get; set; }

    private async Task SetChannelData(ChannelData? channelData)
    {
        ChannelData = channelData;
        ChannelData.CreateChannel(ChannelData);

        if (ChannelData != null)
            await OnEditClick.InvokeAsync(ChannelData);

        DispatchService.Dispatch(new DispatchEntry<ChannelData>()
        {
            Name = ChannelData.Id.ToString(),
            Entry = ChannelData
        });
    }
}
