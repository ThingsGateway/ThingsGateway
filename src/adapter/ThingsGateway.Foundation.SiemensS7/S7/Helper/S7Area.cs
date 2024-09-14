//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// 区域
/// </summary>
internal enum S7Area : byte
{
    /// <inheritdoc/>
    PE = 0x81,

    /// <inheritdoc/>
    PA = 0x82,

    /// <inheritdoc/>
    MK = 0x83,

    /// <inheritdoc/>
    DB = 0x84,

    /// <inheritdoc/>
    CT = 0x1C,

    /// <inheritdoc/>
    TM = 0x1D,

    /// <inheritdoc/>
    AI = 0X06,

    /// <inheritdoc/>
    AQ = 0x07,
}
