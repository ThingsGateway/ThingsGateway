//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// 数据类型信息
/// </summary>
public static class DataTypeExtensions
{
    /// <summary>
    /// 获取实际字节长度，不能确定返回0，bool返回1
    /// </summary>
    /// <param name="coreDataType"></param>
    /// <returns></returns>
    public static byte GetByteLength(this DataTypeEnum coreDataType)
    {
        return coreDataType switch
        {
            DataTypeEnum.Boolean => 1,
            DataTypeEnum.Byte => 1,
            DataTypeEnum.Int16 => 2,
            DataTypeEnum.UInt16 => 2,
            DataTypeEnum.Int32 => 4,
            DataTypeEnum.UInt32 => 4,
            DataTypeEnum.Int64 => 8,
            DataTypeEnum.UInt64 => 8,
            DataTypeEnum.Single => 4,
            DataTypeEnum.Double => 8,
            DataTypeEnum.Decimal => 16,
            _ => 0,
        };
    }

    /// <summary>
    /// 获取DataTypeEnum
    /// </summary>
    /// <param name="coreType"></param>
    /// <returns></returns>
    public static DataTypeEnum GetDataType(this TypeCode coreType)
    {
        return coreType switch
        {
            TypeCode.String => DataTypeEnum.String,
            TypeCode.Boolean => DataTypeEnum.Boolean,
            TypeCode.Byte => DataTypeEnum.Byte,
            TypeCode.Int16 => DataTypeEnum.Int16,
            TypeCode.UInt16 => DataTypeEnum.UInt16,
            TypeCode.Int32 => DataTypeEnum.Int32,
            TypeCode.UInt32 => DataTypeEnum.UInt32,
            TypeCode.Int64 => DataTypeEnum.Int64,
            TypeCode.UInt64 => DataTypeEnum.UInt64,
            TypeCode.Single => DataTypeEnum.Single,
            TypeCode.Double => DataTypeEnum.Double,
            _ => DataTypeEnum.Object,
        };
    }

    /// <summary>
    /// 获取DOTNET RUNTIME TYPE
    /// </summary>
    /// <param name="coreDataType"></param>
    /// <returns></returns>
    public static Type GetSystemType(this DataTypeEnum coreDataType)
    {
        return coreDataType switch
        {
            DataTypeEnum.String => typeof(string),
            DataTypeEnum.Boolean => typeof(bool),
            DataTypeEnum.Byte => typeof(byte),
            DataTypeEnum.Int16 => typeof(short),
            DataTypeEnum.UInt16 => typeof(ushort),
            DataTypeEnum.Int32 => typeof(int),
            DataTypeEnum.UInt32 => typeof(uint),
            DataTypeEnum.Int64 => typeof(long),
            DataTypeEnum.UInt64 => typeof(ulong),
            DataTypeEnum.Single => typeof(float),
            DataTypeEnum.Double => typeof(double),
            _ => typeof(object),
        };
    }
}
