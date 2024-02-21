//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// ThingsGatewayBitConverterExtensions
/// </summary>
public static class ThingsGatewayBitConverterExtension
{
    #region 获取对应数据类型的数据

    /// <summary>
    /// 根据数据类型获取实际值
    /// </summary>
    public static object GetDataFormBytes(this IThingsGatewayBitConverter thingsGatewayBitConverter, byte[] bytes, DataTypeEnum dataType, int offset = 0)
    {
        switch (dataType)
        {
            case DataTypeEnum.String:
                return thingsGatewayBitConverter.ToString(bytes, offset, bytes.Length - offset);

            case DataTypeEnum.Boolean:
                return thingsGatewayBitConverter.ToBoolean(bytes, offset);

            case DataTypeEnum.Byte:
                return thingsGatewayBitConverter.ToByte(bytes, offset);

            case DataTypeEnum.Int16:
                return thingsGatewayBitConverter.ToInt16(bytes, offset);

            case DataTypeEnum.UInt16:
                return thingsGatewayBitConverter.ToUInt16(bytes, offset);

            case DataTypeEnum.Int32:
                return thingsGatewayBitConverter.ToInt32(bytes, offset);

            case DataTypeEnum.UInt32:
                return thingsGatewayBitConverter.ToUInt32(bytes, offset);

            case DataTypeEnum.Int64:
                return thingsGatewayBitConverter.ToInt64(bytes, offset);

            case DataTypeEnum.UInt64:
                return thingsGatewayBitConverter.ToUInt64(bytes, offset);

            case DataTypeEnum.Single:
                return thingsGatewayBitConverter.ToSingle(bytes, offset);

            case DataTypeEnum.Double:
                return thingsGatewayBitConverter.ToDouble(bytes, offset);

            default:
                return Task.FromResult(new OperResult(string.Format(FoundationConst.DataTypeNotSupported, dataType)));
        }
    }

    #endregion 获取对应数据类型的数据
}