//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Dlt645;

internal static class PackHelper
{
    /// <summary>
    /// 打包变量，添加到<see href="deviceVariableSourceReads"></see>
    /// </summary>
    /// <param name="device"></param>
    /// <param name="deviceVariables"></param>
    /// <param name="maxPack">最大打包长度</param>
    /// <param name="defaultIntervalTime">默认间隔时间</param>
    /// <returns></returns>
    public static List<T> LoadSourceRead<T>(ProtocolBase device, List<IVariable> deviceVariables, int maxPack, int defaultIntervalTime) where T : IVariableSourceT<IVariable>, new()
    {
        var byteConverter = device.ThingsGatewayBitConverter;
        var result = new List<T>();
        //需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.RegisterAddress;

            IThingsGatewayBitConverter transformParameter = ByteTransUtil.GetTransByAddress(ref address, byteConverter);
            item.ThingsGatewayBitConverter = transformParameter;
            //item.Address = address;
            item.Index = device.GetBitOffset(item.RegisterAddress);
            var r = new T()
            {
                RegisterAddress = address,
                Length = 1,
                TimeTick = new(item.IntervalTime ?? defaultIntervalTime)
            };
            r.AddVariable(item);
            result.Add(r);
        }
        return result;
    }
}