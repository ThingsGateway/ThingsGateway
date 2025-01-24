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
/// <inheritdoc/>
/// </summary>
internal enum S7WordLength : byte
{
    /// <inheritdoc/>
    Bit = 0x01,

    /// <inheritdoc/>
    Byte = 0x02,

    /// <inheritdoc/>
    Char = 0x03,

    /// <inheritdoc/>
    Word = 0x04,

    /// <inheritdoc/>
    Int = 0x05,

    /// <inheritdoc/>
    DWord = 0x06,

    /// <inheritdoc/>
    DInt = 0x07,

    /// <inheritdoc/>
    Real = 0x08,

    /// <inheritdoc/>
    Counter = 0x1C,

    /// <inheritdoc/>
    Timer = 0x1D,
}
