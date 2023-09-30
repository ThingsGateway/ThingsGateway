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

using System.Collections.Generic;

using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;

namespace ThingsGateway.Foundation.Demo;
/// <summary>
/// OPC
/// </summary>
public partial class OPCDAClientPage
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    /// <summary>
    /// OPC
    /// </summary>
    public ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient OPC;

    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<List<ItemReadResult>> ValueAction;

    private readonly OPCNode node = new();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        OPC.SafeDispose();
    }
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        OPC = new ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient(LogOut);
        OPC.DataChangedHandler += Info_DataChangedHandler;
        OPC.Init(node);
        base.OnInitialized();
    }

    private void Connect()
    {
        try
        {
            OPC.Disconnect();
            GetOPCClient().Connect();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }
    private void DisConnect()
    {
        try
        {
            OPC.Disconnect();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }

    private ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient GetOPCClient()
    {
        //载入配置
        OPC.Init(node);
        return OPC;
    }

    private void Info_DataChangedHandler(List<ItemReadResult> values)
    {
        ValueAction?.Invoke(values);
    }

    private void LogOut(byte logLevel, object source, string message, Exception exception) => LogAction?.Invoke((LogLevel)logLevel, source, message, exception);
}