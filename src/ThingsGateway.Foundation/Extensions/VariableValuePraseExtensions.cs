
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation;

/// <summary>
/// VariableValuePraseExtensions
/// </summary>
public static class VariableValuePraseExtensions
{
    /// <summary>
    /// 在返回的字节数组中解析每个变量的值
    /// 根据每个变量的<see cref="IVariable.Index"/>
    /// 不支持变长字符串类型变量，不能存在于变量List中
    /// </summary>
    /// <param name="variables">设备变量List</param>
    /// <param name="plc">设备</param>
    /// <param name="buffer">返回的字节数组</param>
    /// <param name="exWhenAny">任意一个失败时抛出异常</param>
    /// <returns>解析结果</returns>
    public static void PraseStructContent<T>(this IEnumerable<T> variables, IProtocol plc, byte[] buffer, bool exWhenAny = false) where T : IVariable
    {
        foreach (var variable in variables)
        {
            IThingsGatewayBitConverter byteConverter = variable.ThingsGatewayBitConverter;
            var dataType = variable.DataType;
            var index = variable.Index;
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToBoolean(buffer, index, byteConverter.ArrayLength.Value, plc.BitReverse(variable.RegisterAddress)) :
                    byteConverter.ToBoolean(buffer, index, plc.BitReverse(variable.RegisterAddress))
                    );
                    break;

                case DataTypeEnum.Byte:
                    Set(variable,
                    byteConverter.ArrayLength > 1 ?
                    byteConverter.ToByte(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToByte(buffer, index));
                    break;

                case DataTypeEnum.Int16:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToInt16(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToInt16(buffer, index));
                    break;

                case DataTypeEnum.UInt16:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToUInt16(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToUInt16(buffer, index));
                    break;

                case DataTypeEnum.Int32:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToInt32(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToInt32(buffer, index));
                    break;

                case DataTypeEnum.UInt32:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToUInt32(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToUInt32(buffer, index));
                    break;

                case DataTypeEnum.Int64:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToInt64(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToInt64(buffer, index));
                    break;

                case DataTypeEnum.UInt64:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToUInt64(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToUInt64(buffer, index));
                    break;

                case DataTypeEnum.Single:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToSingle(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToSingle(buffer, index));
                    break;

                case DataTypeEnum.Double:
                    Set(variable,
                     byteConverter.ArrayLength > 1 ?
                    byteConverter.ToDouble(buffer, index, byteConverter.ArrayLength.Value) :
                    byteConverter.ToDouble(buffer, index));
                    break;

                case DataTypeEnum.String:
                default:
                    if (byteConverter.ArrayLength > 1)
                    {
                        List<String> strings = new();
                        for (int i = 0; i < byteConverter.ArrayLength; i++)
                        {
                            var data = byteConverter.ToString(buffer, index + i * byteConverter.StringLength ?? 1, byteConverter.StringLength ?? 1);
                            strings.Add(data);
                        }
                        Set(variable, strings.ToArray());
                    }
                    else
                    {
                        Set(variable, byteConverter.ToString(buffer, index, byteConverter.StringLength ?? 1));
                    }
                    break;
            }
        }
        void Set(IVariable organizedVariable, object num)
        {
            var result = organizedVariable.SetValue(num);
            if (!result.IsSuccess && exWhenAny)
                throw result.Exception ?? new Exception(result.ErrorMessage);
        }
    }
}
