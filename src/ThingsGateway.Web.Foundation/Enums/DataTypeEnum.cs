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

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 数据类型
/// </summary>
public enum DataTypeEnum
{
    /// <inheritdoc/>
    Object,
    /// <inheritdoc/>
    Bcd,
    /// <inheritdoc/>
    DateTime,
    /// <inheritdoc/>
    String,

    /// <inheritdoc/>
    Boolean,
    /// <inheritdoc/>
    Byte,
    /// <inheritdoc/>
    SByte,
    /// <inheritdoc/>
    Int16,
    /// <inheritdoc/>
    UInt16,
    /// <inheritdoc/>
    Int32,
    /// <inheritdoc/>
    UInt32,
    /// <inheritdoc/>
    Int64,
    /// <inheritdoc/>
    UInt64,
    /// <inheritdoc/>
    Single,
    /// <inheritdoc/>
    Double,
}
/// <summary>
/// 数据类型信息
/// </summary>
public static class DataTypeExtension
{
    /// <summary>
    /// 获取DOTNET RUNTIME TYPE
    /// </summary>
    /// <param name="coreDataType"></param>
    /// <returns></returns>
    public static Type GetNetType(this DataTypeEnum coreDataType)
    {
        switch (coreDataType)
        {
            case DataTypeEnum.Object:
                return typeof(object);
            case DataTypeEnum.Bcd:
                return typeof(string);
            case DataTypeEnum.DateTime:
                return typeof(DateTime);
            case DataTypeEnum.String:
                return typeof(string);
            case DataTypeEnum.Boolean:
                return typeof(bool);
            case DataTypeEnum.Byte:
                return typeof(byte);
            case DataTypeEnum.SByte:
                return typeof(sbyte);
            case DataTypeEnum.Int16:
                return typeof(short);
            case DataTypeEnum.UInt16:
                return typeof(ushort);
            case DataTypeEnum.Int32:
                return typeof(int);
            case DataTypeEnum.UInt32:
                return typeof(uint);
            case DataTypeEnum.Int64:
                return typeof(long);
            case DataTypeEnum.UInt64:
                return typeof(ulong);
            case DataTypeEnum.Single:
                return typeof(float);

            case DataTypeEnum.Double:
                return typeof(double);
            default:
                return typeof(string);
        }
    }

    /// <summary>
    /// 获取实际字节长度，不足1写1
    /// </summary>
    /// <param name="coreDataType"></param>
    /// <returns></returns>
    public static int GetByteLength(this DataTypeEnum coreDataType)
    {
        switch (coreDataType)
        {
            case DataTypeEnum.Object:
                return 0;
            case DataTypeEnum.Bcd:
                return 0;
            case DataTypeEnum.DateTime:
                return 0;
            case DataTypeEnum.String:
                return 0;
            case DataTypeEnum.Boolean:
                return 1;
            case DataTypeEnum.Byte:
                return 1;
            case DataTypeEnum.SByte:
                return 1;
            case DataTypeEnum.Int16:
                return 2;
            case DataTypeEnum.UInt16:
                return 2;
            case DataTypeEnum.Int32:
                return 4;
            case DataTypeEnum.UInt32:
                return 4;
            case DataTypeEnum.Int64:
                return 8;
            case DataTypeEnum.UInt64:
                return 8;
            case DataTypeEnum.Single:
                return 4;
            case DataTypeEnum.Double:
                return 8;
            default:
                return 0;
        }
    }

}