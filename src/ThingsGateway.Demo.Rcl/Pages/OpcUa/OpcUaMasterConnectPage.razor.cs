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
using ThingsGateway.Foundation.OpcUa;

using TouchSocket.Core;

namespace ThingsGateway.Demo;

public partial class OpcUaMasterConnectPage : IDisposable
{
    public ThingsGateway.Foundation.OpcUa.OpcUaMaster Plc;

    private readonly OpcUaConfig config = new();

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
        Plc = new ThingsGateway.Foundation.OpcUa.OpcUaMaster();
        Plc.OpcUaConfig = config;
        LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        var logger = TextFileLogger.Create(Plc.GetHashCode().ToLong().GetDebugLogPath());
        logger.LogLevel = LogLevel.Trace;
        LogMessage.AddLogger(logger);

        Plc.OpcStatusChange += Plc_OpcStatusChange;
        Plc.DataChangedHandler += (a) => LogMessage.Trace((a.variableNode.NodeId, a.jToken).ToJsonString(true));
        base.OnInitialized();
    }

    private void Plc_OpcStatusChange(object? sender, OpcUaStatusEventArgs e)
    {
        LogMessage?.Log((LogLevel)e.LogLevel, null, e.Text, null);
    }

    private async Task Connect()
    {
        try
        {
            Plc.Disconnect();
            await GetOpc().ConnectAsync(CancellationToken.None);
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

    private ThingsGateway.Foundation.OpcUa.OpcUaMaster GetOpc()
    {
        //载入配置
        Plc.OpcUaConfig = config;
        return Plc;
    }
}