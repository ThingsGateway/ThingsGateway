//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Management;

internal partial class ReverseCallbackServer : RpcServer
{
    private IRedundancyHostedService _redundancyHostedService;
    public ReverseCallbackServer(IRedundancyHostedService redundancyHostedService)
    {
        _redundancyHostedService = redundancyHostedService;
    }

    [DmtpRpc(MethodInvoke = true)]
    public async Task<OperResultClass> SetGatewayState(bool isStart)
    {
        try
        {
            await _redundancyHostedService.RedundancyStopAsync();
            await _redundancyHostedService.RedundancyStartAsync(isStart);
            return new();
        }
        catch (Exception ex)
        {
            return new OperResultClass(ex.Message);
        }

    }


    [DmtpRpc(MethodInvoke = true)]
    public Task UpdateGatewayDataAsync(List<DeviceDataWithValue> deviceDatas, List<VariableDataWithValue> variableDatas)
    {

        foreach (var deviceData in deviceDatas)
        {
            if (GlobalData.ReadOnlyCollectDevices.TryGetValue(deviceData.Name, out var value))
            {
                value.SetDeviceStatus(deviceData.ActiveTime, lastErrorMessage: deviceData.LastErrorMessage, deviceStatus: deviceData.DeviceStatus);
            }
        }
        var dict = GlobalData.ReadOnlyVariables.ToDictionary(a => a.Key, a => a.Value);
        foreach (var variableData in variableDatas)
        {
            if (dict.TryGetValue(variableData.Name, out var value))
            {
                value.SetValue(variableData.RawValue, variableData.CollectTime, variableData.IsOnline);
                value.SetErrorMessage(variableData.LastErrorMessage);
            }
        }
        return Task.CompletedTask;
    }
}
