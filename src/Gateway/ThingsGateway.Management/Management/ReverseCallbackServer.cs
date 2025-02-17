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

namespace ThingsGateway.Management;

public partial class ReverseCallbackServer : RpcServer
{
    [DmtpRpc(MethodInvoke = true)]
    public void UpdateGatewayData(List<DeviceDataWithValue> deviceDatas, List<VariableDataWithValue> variableDatas)
    {

        foreach (var deviceData in deviceDatas)
        {
            if (GlobalData.ReadOnlyDevices.TryGetValue(deviceData.Name, out var value))
            {
                value.SetDeviceStatus(deviceData.ActiveTime, deviceData.DeviceStatus == DeviceStatusEnum.OnLine ? false : true, lastErrorMessage: deviceData.LastErrorMessage);
            }
        }
        foreach (var variableData in variableDatas)
        {
            if (GlobalData.ReadOnlyVariables.TryGetValue(variableData.Name, out var value))
            {
                value.SetValue(variableData.RawValue, variableData.CollectTime, variableData.IsOnline);
                value.SetErrorMessage(variableData.LastErrorMessage);
            }
        }
    }
}
