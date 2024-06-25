//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class Dlt645_2007Request
{
    #region Request

    /// <summary>
    /// 数据标识
    /// </summary>
    public byte[] DataId { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 反转解析
    /// </summary>
    public bool Reverse { get; set; } = true;

    /// <summary>
    /// 站号信息
    /// </summary>
    public byte[] Station { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 数据
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; set; }

    #endregion Request
}
