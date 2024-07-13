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

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.OpcDa;
using ThingsGateway.Foundation.OpcDa.Da;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class OpcDaMaster : IDisposable
{
    public LoggerGroup? LogMessage;
    private readonly OpcDaProperty OpcDaProperty = new();
    private ThingsGateway.Foundation.OpcDa.OpcDaMaster _plc;
    private string LogPath;
    private string RegisterAddress;
    private string WriteValue;

    /// <inheritdoc/>
    ~OpcDaMaster()
    {
        this.SafeDispose();
    }

    private AdapterDebugComponent AdapterDebugComponent { get; set; }

    [Inject]
    private IStringLocalizer<OpcDaProperty> OpcDaPropertyLocalizer { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _plc?.SafeDispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        _plc = new ThingsGateway.Foundation.OpcDa.OpcDaMaster();

        _plc.Init(OpcDaProperty);
        LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        var logger = TextFileLogger.Create(_plc.GetHashCode().ToLong().GetDebugLogPath());
        logger.LogLevel = LogLevel.Trace;
        LogMessage.AddLogger(logger);

        _plc.LogEvent = (a, b, c, d) => LogMessage.Log((LogLevel)a, b, c, d);
        _plc.DataChangedHandler += (a, b, c) => LogMessage.Trace(c.ToJsonString());
        base.OnInitialized();
    }

    private void Add()
    {
        var tags = new Dictionary<string, List<OpcItem>>();
        var tag = new OpcItem(RegisterAddress);
        tags.Add(Guid.NewGuid().ToString(), new List<OpcItem>() { tag });
        try
        {
            _plc.AddItems(tags);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

    private void Connect()
    {
        try
        {
            _plc.Disconnect();
            LogPath = _plc?.GetHashCode().ToLong().GetDebugLogPath();
            GetOpc().Connect();
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

    private ThingsGateway.Foundation.OpcDa.OpcDaMaster GetOpc()
    {
        //载入配置
        _plc.Init(OpcDaProperty);
        return _plc;
    }

    private async Task ReadAsync()
    {
        try
        {
            _plc.ReadItemsWithGroup();
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        await Task.CompletedTask;
    }

    private void Remove()
    {
        _plc.RemoveItems(new List<string>() { RegisterAddress });
    }

    [Inject]
    private DialogService DialogService { get; set; }

    private async Task ShowImport()
    {
        var op = new DialogOption()
        {
            Title = OpcDaPropertyLocalizer["ShowImport"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<OpcDaImportVariable>(new Dictionary<string, object?>
        {
            [nameof(OpcDaImportVariable.Plc)] = _plc,
        });
        await DialogService.Show(op);
    }

    private async Task WriteAsync()
    {
        try
        {
            JToken tagValue = WriteValue.GetJTokenFromString();
            var obj = tagValue.GetObjectFromJToken();

            var data = _plc.WriteItem(
                new()
                {
                {RegisterAddress,  obj}
                }
                );
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    if (item.Value.Item1)
                        LogMessage?.LogInformation(item.ToJsonString());
                    else
                        LogMessage?.LogWarning(item.ToJsonString());
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        await Task.CompletedTask;
    }
}
