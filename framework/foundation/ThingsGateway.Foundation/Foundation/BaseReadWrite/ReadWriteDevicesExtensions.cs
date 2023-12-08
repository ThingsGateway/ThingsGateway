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

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// 读写扩展方法
/// </summary>
public static class ReadWriteDevicesExtensions
{
    /// <summary>
    /// 在返回的字节数组中解析每个变量的值
    /// 根据每个变量的<see cref="IDeviceVariableRunTime.Index"/>
    /// 不支持变长字符串类型变量，一定不能存在于变量List中
    /// </summary>
    /// <param name="values">设备变量List</param>
    /// <param name="plc">设备</param>
    /// <param name="buffer">返回的字节数组</param>
    /// <returns>解析结果</returns>
    public static void PraseStructContent<T>(this IList<T> values, IReadWrite plc, byte[] buffer) where T : IDeviceVariableRunTime
    {
        foreach (var organizedVariable in values)
        {
            IThingsGatewayBitConverter byteConverter = organizedVariable.ThingsGatewayBitConverter;
            var dataType = organizedVariable.DataTypeEnum;
            var index = organizedVariable.Index;
            switch (dataType)
            {
                case DataTypeEnum.String:
                    Set(organizedVariable, byteConverter.ToString(buffer, index, byteConverter.Length ?? 1));
                    break;

                case DataTypeEnum.Boolean:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToBoolean(buffer, index, byteConverter.Length.Value, plc.BitReverse(organizedVariable.Address)) :
                    byteConverter.ToBoolean(buffer, index, plc.BitReverse(organizedVariable.Address))
                    );
                    break;

                case DataTypeEnum.Byte:
                    Set(organizedVariable,
                    byteConverter.Length > 1 ?
                    byteConverter.ToByte(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToByte(buffer, index));
                    break;

                case DataTypeEnum.Int16:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToInt16(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToInt16(buffer, index));
                    break;

                case DataTypeEnum.UInt16:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToUInt16(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToUInt16(buffer, index));
                    break;

                case DataTypeEnum.Int32:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToInt32(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToInt32(buffer, index));
                    break;

                case DataTypeEnum.UInt32:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToUInt32(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToUInt32(buffer, index));
                    break;

                case DataTypeEnum.Int64:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToInt64(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToInt64(buffer, index));
                    break;

                case DataTypeEnum.UInt64:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToUInt64(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToUInt64(buffer, index));
                    break;

                case DataTypeEnum.Single:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToSingle(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToSingle(buffer, index));
                    break;

                case DataTypeEnum.Double:
                    Set(organizedVariable,
                     byteConverter.Length > 1 ?
                    byteConverter.ToDouble(buffer, index, byteConverter.Length.Value) :
                    byteConverter.ToDouble(buffer, index));
                    break;

                default:
                    Set(organizedVariable, byteConverter.ToString(buffer, index, byteConverter.Length ?? 1));
                    break;
            }
        }
        static void Set(IDeviceVariableRunTime organizedVariable, object num)
        {
            organizedVariable.SetValue(num);
        }
    }
}