#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Microsoft.AspNetCore.Components;

using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.OpcDa;

using TouchSocket.Core;

namespace ThingsGateway.Demo;

public partial class OpcDaMasterConnectPage : IDisposable
{
    public ThingsGateway.Foundation.OpcDa.OpcDaMaster Plc;

    private readonly OpcDaConfig config = new();

    protected override void Dispose(bool disposing)
    {
        Plc.SafeDispose();
        base.Dispose(disposing);
    }

    [Parameter]
    public EventCallback OnConnectClick { get; set; }

    public LoggerGroup? LogMessage;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        Plc = new ThingsGateway.Foundation.OpcDa.OpcDaMaster();

        Plc.Init(config);
        LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        var logger = TextFileLogger.Create(Plc.GetHashCode().ToString().GetDebugLogPath());
        logger.LogLevel = LogLevel.Trace;
        LogMessage.AddLogger(logger);

        Plc.LogEvent = (a, b, c, d) => LogMessage.Log((LogLevel)a, b, c, d);
        Plc.DataChangedHandler += (a) => LogMessage.Trace(a.ToJsonString(true));
        base.OnInitialized();
    }

    private void Connect()
    {
        try
        {
            Plc.Disconnect();
            GetOpc().Connect();
        }
        catch (Exception ex)
        {
            LogMessage?.Log(LogLevel.Error, null, ex.Message, ex);
        }
    }

    private void DisConnect()
    {
        try
        {
            Plc.Disconnect();
        }
        catch (Exception ex)
        {
            LogMessage?.Log(LogLevel.Error, null, ex.Message, ex);
        }
    }

    private ThingsGateway.Foundation.OpcDa.OpcDaMaster GetOpc()
    {
        //载入配置
        Plc.Init(config);
        return Plc;
    }
}