//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class ChannelComponent : ComponentBase
{
    [Parameter]
    public string ClassString { get; set; }
    private ChannelObject channelObject = new();
    [Parameter]
    public EventCallback<ChannelObject> OnConnectClick { get; set; }
    [Parameter]
    public EventCallback<(ChannelObject, string)> OnConfimClick { get; set; }

    [Parameter]
    public EventCallback OnDisConnectClick { get; set; }

    [Parameter]
    public Func<TouchSocketConfig, IChannelOptions, IChannel> OnCreateClick { get; set; }

    public ChannelOptionsDefault? Model { get; set; } = new();

    public void PublicStateHasChanged()
    {
        StateHasChanged();
    }

    private IChannel? Channel => channelObject.Channel;

    [Inject]
    private IStringLocalizer<ChannelComponent> Localizer { get; set; }

    [Inject]
    private ToastService ToastService { get; set; }

    private async Task DisconnectClick()
    {
        try
        {
            if (Channel != null)
            {
                await Channel.CloseAsync($"nameof(DisconnectClick)");
                if (OnDisConnectClick.HasDelegate)
                    await OnDisConnectClick.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Channel?.Logger?.LogWarning(ex);
        }
    }

    ValidateForm ValidateForm { get; set; }

    private async Task ConnectClick()
    {
        try
        {
            var validate = await ValidateForm.ValidateAsync();
            if (!validate) return;
            await DisconnectClick();
            Channel?.SafeDispose();
            channelObject.Reset(null);

            if (Channel == null)
            {
                var config = new TouchSocket.Core.TouchSocketConfig();
                var logMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
                var path = Model.Id.ToString().GetDebugLogPath();
                var logger = TextFileLogger.GetMultipleFileLogger(path);
                logger.LogLevel = LogLevel.Trace;
                logMessage.AddLogger(logger);
                config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
                Model.Config = config;

                if (OnCreateClick != null)
                    channelObject.Reset(OnCreateClick(config, Model));
                else
                    channelObject.Reset(config.GetChannel(Model));

                if (OnConfimClick.HasDelegate)
                    await OnConfimClick.InvokeAsync((channelObject, path));

                await Channel.SetupAsync(config);
            }

            await Channel.ConnectAsync(default);

            if (OnConnectClick.HasDelegate)
                await OnConnectClick.InvokeAsync(channelObject);
        }
        catch (Exception ex)
        {
            Channel?.Logger?.LogWarning(ex);
        }
    }
}
