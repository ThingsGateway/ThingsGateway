//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// 控制码
/// </summary>
public enum ControlCode : byte
{
    /// <summary>
    /// 读数据
    /// </summary>
    Read = 0x11,

    /// <summary>
    /// 读后续数据
    /// </summary>
    ReadSub = 0x12,

    /// <summary>
    /// 读站号
    /// </summary>
    ReadStation = 0x13,

    /// <summary>
    /// 写数据
    /// </summary>
    Write = 0x14,

    /// <summary>
    /// 写站号
    /// </summary>
    WriteStation = 0x15,

    /// <summary>
    /// 广播校时
    /// </summary>
    BroadcastTime = 0x08,

    /// <summary>
    /// 冻结
    /// </summary>
    Freeze = 0x16,

    /// <summary>
    /// 更新波特率
    /// </summary>
    WriteBaudRate = 0x17,

    /// <summary>
    /// 更新密码
    /// </summary>
    WritePassword = 0x18,
}
