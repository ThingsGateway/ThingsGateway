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

namespace ThingsGateway.Foundation.Extension;

/// <summary>
/// ThingsGatewayBitConverterExtensions
/// </summary>
public static class ThingsGatewayBitConverterExtensions
{
    #region 获取对应数据类型的数据

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static object GetDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, string address, byte[] bytes, DataTypeEnum dataTypeEnum, int offset = 0)
    {
        IThingsGatewayBitConverter byteConverter;
        if (!string.IsNullOrEmpty(address))
            byteConverter = ByteTransformUtil.GetTransByAddress(ref address, thingsGatewayBitConverter);
        else
            byteConverter = thingsGatewayBitConverter;
        switch (dataTypeEnum)
        {
            case DataTypeEnum.String:
                return byteConverter.ToString(bytes, offset, bytes.Length - offset);
            case DataTypeEnum.Boolean:
                return byteConverter.ToBoolean(bytes, offset);
            case DataTypeEnum.Byte:
                return byteConverter.ToByte(bytes, offset);
            case DataTypeEnum.Int16:
                return byteConverter.ToInt16(bytes, offset);
            case DataTypeEnum.UInt16:
                return byteConverter.ToUInt16(bytes, offset);
            case DataTypeEnum.Int32:
                return byteConverter.ToInt32(bytes, offset);
            case DataTypeEnum.UInt32:
                return byteConverter.ToUInt32(bytes, offset);
            case DataTypeEnum.Int64:
                return byteConverter.ToInt64(bytes, offset);
            case DataTypeEnum.UInt64:
                return byteConverter.ToUInt64(bytes, offset);
            case DataTypeEnum.Single:
                return byteConverter.ToSingle(bytes, offset);
            case DataTypeEnum.Double:
                return byteConverter.ToDouble(bytes, offset);
            default:
                return Task.FromResult(new OperResult($"{dataTypeEnum}数据类型未实现"));
        }

    }

    #endregion

}
