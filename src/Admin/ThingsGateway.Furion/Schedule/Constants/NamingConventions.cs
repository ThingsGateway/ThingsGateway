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

namespace ThingsGateway.Schedule;

/// <summary>
/// 命名转换器
/// </summary>
/// <remarks>用于生成持久化 SQL 语句</remarks>
[SuppressSniffer]
public enum NamingConventions
{
    /// <summary>
    /// 驼峰命名法
    /// </summary>
    /// <remarks>第一个单词首字母小写</remarks>
    CamelCase = 0,

    /// <summary>
    /// 帕斯卡命名法
    /// </summary>
    /// <remarks>每一个单词首字母大写</remarks>
    Pascal = 1,

    /// <summary>
    /// 下划线命名法
    /// </summary>
    /// <remarks>每次单词使用下划线连接且首字母都是小写</remarks>
    UnderScoreCase = 2
}