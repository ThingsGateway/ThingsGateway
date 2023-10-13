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

namespace ThingsGateway.Foundation.Adapter.DLT645;

internal static class PackHelper
{
    public static List<T> LoadSourceRead<T, T2>(IReadWrite device, List<T2> deviceVariables, int maxPack) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new()
    {
        var byteConverter = device.ThingsGatewayBitConverter;
        var result = new List<T>();
        //需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.VariableAddress;

            IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, byteConverter);
            item.ThingsGatewayBitConverter = transformParameter;
            //item.VariableAddress = address;
            item.Index = device.GetBitOffset(item.VariableAddress);

            result.Add(new()
            {
                DeviceVariableRunTimes = new() { item },
                VariableAddress = address,
                Length = 1,
            });
        }
        return result;

    }


}
