//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Application;

internal partial class ReverseCallbackServer : RpcServer
{
    [DmtpRpc(true)]//使用方法名作为调用键
    public GatewayState GetGatewayState(bool isStart)
    {
        GatewayState result = new();
        result.IsStart = HostedServiceUtil.ManagementHostedService.StartCollectDeviceEnable;
        result.IsPrimary = HostedServiceUtil.ManagementHostedService.Options?.Redundancy?.IsPrimary == true;
        return result;
    }

    [DmtpRpc(true)]//使用方法名作为调用键
    public Task UpdateGatewayDataAsync(List<DeviceDataWithValue> deviceDatas, List<VariableDataWithValue> variableDatas)
    {

        foreach (var deviceData in deviceDatas)
        {
            if (GlobalData.CollectDevices.TryGetValue(deviceData.Name, out var value))
            {
                value.ActiveTime = deviceData.ActiveTime;
                value.DeviceStatus = deviceData.DeviceStatus;
                value.LastErrorMessage = deviceData.LastErrorMessage;
            }
        }
        var dict = GlobalData.Variables.ToDictionary(a => a.Key, a => a.Value);
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
