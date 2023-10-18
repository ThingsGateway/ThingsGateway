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

using ThingsGateway.Foundation.Adapter.OPCUA;

namespace ThingsGateway.Foundation.Demo;

/// <summary>
/// OPCUAClientPage
/// </summary>
public partial class OPCUAClientPage
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;
    /// <summary>
    /// OPC
    /// </summary>
    public ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient OPC;

    private readonly OPCNode node = new();


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        OPC.OpcStatusChange -= OPC_OpcStatusChange;
        OPC.SafeDispose();
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        OPC = new ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient();
        OPC.OpcStatusChange += OPC_OpcStatusChange;
        base.OnInitialized();
    }

    private void OPC_OpcStatusChange(object sender, OpcUaStatusEventArgs e)
    {
        if (e.Error)
            LogAction?.Invoke(LogLevel.Warning, null, e.Text, null);
    }

    private async Task ConnectAsync()
    {
        try
        {
            OPC.Disconnect();
            await GetOPCClient().ConnectAsync();
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
    private ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient GetOPCClient()
    {
        OPC.OPCNode = node;
        return OPC;
    }

}