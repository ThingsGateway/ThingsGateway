
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Application;

internal partial class ReverseCallbackServer : RpcServer
{
    private EasyLock easyLock = new();

    [DmtpRpc(true)]//使用方法名作为调用键
    public async Task<GatewayState> GetGatewayStateAsync(bool isStart)
    {
        try
        {
            await easyLock.WaitAsync().ConfigureAwait(false);

            //冗余双方站点可能存在同时执行冗余切换的情况
            {
                GatewayState result = new();
                result.IsStart = HostedServiceUtil.ManagementHostedService.StartCollectDeviceEnable;
                result.IsPrimary = HostedServiceUtil.ManagementHostedService.Options?.Redundancy?.IsPrimary == true;
                return result;
            }
        }
        finally
        {
            easyLock.Release();
        }
    }

    [DmtpRpc(true)]//使用方法名作为调用键
    public async Task UpdateGatewayDataAsync(Dictionary<string, DeviceDataWithValue> deviceDatas, Dictionary<string, VariableDataWithValue> variableDatas)
    {
        await Task.CompletedTask;
        foreach (var deviceData in deviceDatas)
        {
            if (GlobalData.CollectDevices.TryGetValue(deviceData.Key, out var value))
            {
                value.ActiveTime = deviceData.Value.ActiveTime;
                value.DeviceStatus = deviceData.Value.DeviceStatus;
                value.LastErrorMessage = deviceData.Value.LastErrorMessage;
            }
        }
        foreach (var variableData in variableDatas)
        {
            if (GlobalData.Variables.TryGetValue(variableData.Key, out var value))
            {
                value.SetValue(variableData.Value.RawValue, variableData.Value.CollectTime, variableData.Value.IsOnline);
                value.SetErrorMessage(variableData.Value.LastErrorMessage);
            }
        }
    }
}
