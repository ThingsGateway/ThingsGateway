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

using ThingsGateway.Application;
using ThingsGateway.Foundation;

namespace ThingsGateway.DLT645;

internal static class DLT645_2007Helper
{
    internal static List<DeviceVariableSourceRead> LoadSourceRead(this List<DeviceVariableRunTime> deviceVariables, IReadWriteDevice device)
    {
        var byteConverter = device.ThingsGatewayBitConverter;
        var result = new List<DeviceVariableSourceRead>();
        //需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.VariableAddress;

            IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, byteConverter);
            item.ThingsGatewayBitConverter = transformParameter;
            item.VariableAddress = address;
            item.Index = device.GetBitOffset(item.VariableAddress);

            result.Add(new()
            {
                DeviceVariables = new() { item },
                Address = address,
                Length = 1,
            });
        }
        return result;

    }


}
