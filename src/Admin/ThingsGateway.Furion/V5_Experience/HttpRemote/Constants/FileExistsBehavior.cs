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
///     指定当目标文件已存在时的行为
/// </summary>
public enum FileExistsBehavior
{
    /// <summary>
    ///     创建新文件
    /// </summary>
    /// <remarks>如果文件已存在则抛出异常。</remarks>
    CreateNew = 0,

    /// <summary>
    ///     覆盖现有文件
    /// </summary>
    Overwrite,

    /// <summary>
    ///     保留现有文件
    /// </summary>
    /// <remarks>不进行任何操作。</remarks>
    Skip
}