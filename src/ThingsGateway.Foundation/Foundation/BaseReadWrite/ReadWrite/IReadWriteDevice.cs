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

namespace ThingsGateway.Foundation;

/// <summary>
/// 读写设备接口
/// </summary>
public interface IReadWriteDevice : IReadWrite, IDisposable
{
    /// <summary>
    /// 多字节数据解析规则
    /// </summary>
    DataFormat DataFormat { get; set; }

    /// <summary>
    /// 数据解析规则
    /// </summary>
    IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

    /// <summary>
    /// 读写超时时间
    /// </summary>
    ushort TimeOut { get; set; }

    /// <summary>
    /// 一个寄存器所占的字节长度
    /// </summary>
    ushort RegisterByteLength { get; set; }
    /// <summary>
    /// 寄存器地址的详细说明
    /// </summary>
    /// <returns></returns>
    string GetAddressDescription();
}