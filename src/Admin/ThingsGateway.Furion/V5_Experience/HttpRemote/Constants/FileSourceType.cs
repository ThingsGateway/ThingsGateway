// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

namespace ThingsGateway.HttpRemote;

/// <summary>
///     指定多部分表单内容文件的来源类型
/// </summary>
public enum FileSourceType
{
    /// <summary>
    ///     缺省值
    /// </summary>
    /// <remarks>不用作为文件的来源。</remarks>
    None = 0,

    /// <summary>
    ///     本地文件路径
    /// </summary>
    Path,

    /// <summary>
    ///     Base64 字符串文件
    /// </summary>
    Base64String,

    /// <summary>
    ///     互联网文件地址
    /// </summary>
    Remote,

    /// <summary>
    ///     <see cref="Stream" />
    /// </summary>
    Stream,

    /// <summary>
    ///     字节数组
    /// </summary>
    ByteArray
}