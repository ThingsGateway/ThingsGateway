#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Net;
using System.Reflection;

using ThingsGateway.Foundation;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 读写扩展
/// </summary>
public static class ReadWriteHelpers
{
    /// <summary>
    /// 根据<see cref="OperResult.IsSuccess"/>执行action
    /// </summary>
    public static OperResult<T> DealWithReadResult<T>(OperResult<T> read, Action<T> action)
    {
        if (!read.IsSuccess || action == null)
            return read;
        action(read.Content);
        return read;
    }

    /// <summary>
    /// 根据<see cref="PropertyInfo"/> 数据类型转化返回值类型
    /// </summary>
    /// <param name="p"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object ObjToTypeValue(PropertyInfo p, string value)
    {
        object _value = null;
        if (p.PropertyType == typeof(bool))
            _value = value.GetBoolValue();
        else if (p.PropertyType == typeof(byte))
            _value = byte.Parse(value);
        else if (p.PropertyType == typeof(sbyte))
            _value = sbyte.Parse(value);
        else if (p.PropertyType == typeof(short))
            _value = short.Parse(value);
        else if (p.PropertyType == typeof(ushort))
            _value = ushort.Parse(value);
        else if (p.PropertyType == typeof(int))
            _value = int.Parse(value);
        else if (p.PropertyType == typeof(uint))
            _value = uint.Parse(value);
        else if (p.PropertyType == typeof(long))
            _value = long.Parse(value);
        else if (p.PropertyType == typeof(ulong))
            _value = ulong.Parse(value);
        else if (p.PropertyType == typeof(float))
            _value = float.Parse(value);
        else if (p.PropertyType == typeof(double))
            _value = double.Parse(value);
        else if (p.PropertyType == typeof(decimal))
            _value = decimal.Parse(value);
        else if (p.PropertyType == typeof(DateTime))
            _value = DateTime.Parse(value);
        else if (p.PropertyType == typeof(string))
            _value = value;
        else if (p.PropertyType == typeof(IPAddress))
            _value = IPAddress.Parse(value);
        else if (p.PropertyType.BaseType == typeof(Enum))
            _value = Enum.Parse(p.PropertyType, value);
        return _value;

    }
    /// <summary>
    /// 在返回的字节数组中解析每个变量的值
    /// 根据每个变量的<see cref="CollectVariableRunTime.Index"/>
    /// 不支持变长字符串类型变量，一定不能存在于变量List中
    /// </summary>
    /// <param name="buffer">返回的字节数组</param>
    /// <param name="values">设备变量List</param>
    /// <param name="startIndex">开始序号</param>
    public static void PraseStructContent(byte[] buffer, IList<CollectVariableRunTime> values, int startIndex = 0)
    {
        foreach (CollectVariableRunTime organizedVariable in values)
        {
            var deviceValue = organizedVariable;
            string address = deviceValue.VariableAddress;
            //从变量地址中获取DataFormat，得到新的ThingsGatewayBitConverter
            IThingsGatewayBitConverter byteConverter = deviceValue.ThingsGatewayBitConverter;
            Type propertyType = organizedVariable.DataType;
            int index = organizedVariable.Index;
            if (propertyType == typeof(bool))
            {
                if (index + startIndex > buffer.Length * 8)
                    throw new Exception($"返回数据长度{buffer.Length}不足以转换为对应索引位{index + startIndex}的数据，请检查数据类型");
            }
            else
            {
                if (index + startIndex > buffer.Length)
                    throw new Exception($"返回数据长度{buffer.Length}不足以转换为对应索引位{index + startIndex}的数据，请检查数据类型");
            }

            if (propertyType == typeof(byte))
            {
                byte num = byteConverter.ToByte(buffer, index + startIndex);

                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(short))
            {
                short num = byteConverter.ToInt16(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(ushort))
            {
                ushort num = byteConverter.ToUInt16(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(int))
            {
                int num = byteConverter.ToInt32(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(uint))
            {
                uint num = byteConverter.ToUInt32(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(long))
            {
                long num = byteConverter.ToInt64(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(ulong))
            {
                ulong num = byteConverter.ToUInt64(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(float))
            {
                float num = byteConverter.ToSingle(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(double))
            {
                double num = byteConverter.ToDouble(buffer, index + startIndex);
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(bool))
            {
                bool num = byteConverter.ToBoolean(buffer, index + (startIndex * 8));
                organizedVariable.SetValue(num);
            }
            else if (propertyType == typeof(string))
            {
                //从变量地址中获取DataFormat，得到新的ThingsGatewayBitConverter和字符串信息
                if (deviceValue.DataTypeEnum == DataTypeEnum.String)
                {
                    string num = byteConverter.ToString(buffer, index + startIndex, deviceValue.StringLength);
                    organizedVariable.SetValue(num);
                }
                else if (deviceValue.DataTypeEnum == DataTypeEnum.Bcd)
                {
                    string num = byteConverter.ToBcdString(buffer, index + startIndex, deviceValue.StringLength, deviceValue.StringBcdFormat);
                    organizedVariable.SetValue(num);
                }
                else if (deviceValue.DataTypeEnum == DataTypeEnum.DateTime)
                {
                    DateTime num = DateTime.Parse(byteConverter.ToString(buffer, index + startIndex, deviceValue.StringLength));
                    organizedVariable.SetValue(num);
                }

            }

        }
    }
}
