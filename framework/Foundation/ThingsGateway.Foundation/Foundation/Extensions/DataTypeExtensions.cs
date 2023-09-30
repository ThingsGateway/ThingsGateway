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

namespace ThingsGateway.Foundation;


/// <summary>
/// 数据类型信息
/// </summary>
public static class DataTypeExtensions
{
    /// <summary>
    /// 获取DOTNET RUNTIME TYPE
    /// </summary>
    /// <param name="coreDataType"></param>
    /// <returns></returns>
    public static Type GetSystemType(this DataTypeEnum coreDataType)
    {
        return coreDataType switch
        {
            DataTypeEnum.Object => typeof(object),
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

    /// <summary>
    /// 获取实际字节长度，不能确定返回0
    /// </summary>
    /// <param name="coreDataType"></param>
    /// <returns></returns>
    public static int GetByteLength(this DataTypeEnum coreDataType)
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
            _ => 0,
        };
    }

}