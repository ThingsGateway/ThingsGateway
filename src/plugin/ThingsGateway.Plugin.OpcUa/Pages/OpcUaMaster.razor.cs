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
using ThingsGateway.Foundation.OpcUa;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class OpcUaMaster : IDisposable
{
    public LoggerGroup? LogMessage;
    private readonly OpcUaProperty OpcUaProperty = new();
    private ThingsGateway.Foundation.OpcUa.OpcUaMaster _plc;
    private string LogPath;
    private string RegisterAddress;
    private string WriteValue;
    private bool ShowSubvariable;

    /// <inheritdoc/>
    ~OpcUaMaster()
    {
        this.SafeDispose();
    }

    private AdapterDebugComponent AdapterDebugComponent { get; set; }

    [Inject]
    private IStringLocalizer<OpcUaProperty> OpcUaPropertyLocalizer { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _plc?.SafeDispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        _plc = new ThingsGateway.Foundation.OpcUa.OpcUaMaster();
        _plc.OpcUaProperty = OpcUaProperty;

        LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        var logger = TextFileLogger.Create(_plc.GetHashCode().ToLong().GetDebugLogPath());
        logger.LogLevel = LogLevel.Trace;
        LogMessage.AddLogger(logger);

        _plc.LogEvent = (a, b, c, d) => LogMessage.Log((LogLevel)a, b, c, d);
        _plc.DataChangedHandler += (a) => LogMessage.Trace(a.ToJsonString());
        base.OnInitialized();
    }

    private async Task Connect()
    {
        try
        {
            _plc.Disconnect();
            LogPath = _plc?.GetHashCode().ToLong().GetDebugLogPath();
            await GetOpc().ConnectAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            LogMessage?.Log(LogLevel.Error, null, ex.Message, ex);
        }
    }

    private void Disconnect()
    {
        try
        {
            _plc.Disconnect();
        }
        catch (Exception ex)
        {
            LogMessage?.Log(LogLevel.Error, null, ex.Message, ex);
        }
    }

    private ThingsGateway.Foundation.OpcUa.OpcUaMaster GetOpc()
    {
        //载入配置
        _plc.OpcUaProperty = OpcUaProperty;
        return _plc;
    }

    private async Task Add()
    {
        try
        {
            if (_plc.Connected)
                await _plc.AddSubscriptionAsync(Guid.NewGuid().ToString(), [RegisterAddress]);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

    private async Task ReadAsync()
    {
        if (_plc.Connected)
        {
            try
            {
                var data = await _plc.ReadJTokenValueAsync([RegisterAddress]);

                LogMessage?.LogInformation($" {data[0].Item1}：{data[0].Item3}");
            }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex);
            }
        }
    }

    private void Remove()
    {
        if (_plc.Connected)
            _plc.RemoveSubscription("");
    }

    [Inject]
    private DialogService DialogService { get; set; }

    private async Task ShowImport()
    {
        var op = new DialogOption()
        {
            Title = OpcUaPropertyLocalizer["ShowImport"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<OpcUaImportVariable>(new Dictionary<string, object?>
        {
            [nameof(OpcUaImportVariable.Plc)] = _plc,
            [nameof(OpcUaImportVariable.ShowSubvariable)] = ShowSubvariable,
        });
        await DialogService.Show(op);
    }

    private async Task WriteAsync()
    {
        if (_plc.Connected)
        {
            var data = await _plc.WriteNodeAsync(
                new()
                {
                        {RegisterAddress, WriteValue.GetJTokenFromString()}
                }
                );

            foreach (var item in data)
            {
                if (item.Value.Item1)
                    LogMessage?.LogInformation(item.ToJsonString());
                else
                    LogMessage?.LogWarning(item.ToJsonString());
            }
        }
    }
}