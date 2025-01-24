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
/// 作业信息配置选项
/// </summary>
[SuppressSniffer]
public sealed class JobDetailOptions
{
    /// <summary>
    /// 构造函数
    /// </summary>
    internal JobDetailOptions()
    {
    }

    /// <summary>
    /// 重写 <see cref="ConvertToSQL"/>
    /// </summary>
    public Func<string, string[], JobDetail, PersistenceBehavior, NamingConventions, string> ConvertToSQL
    {
        get
        {
            return ConvertToSQLConfigure;
        }
        set
        {
            ConvertToSQLConfigure = value;
        }
    }

    /// <summary>
    /// 启用作业执行详细日志
    /// </summary>
    public bool LogEnabled
    {
        get
        {
            return InternalLogEnabled;
        }
        set
        {
            InternalLogEnabled = value;
        }
    }

    /// <summary>
    /// <see cref="LogEnabled"/> 静态配置
    /// </summary>
    internal static bool InternalLogEnabled { get; private set; }

    /// <summary>
    /// <see cref="ConvertToSQL"/> 静态配置
    /// </summary>
    internal static Func<string, string[], JobDetail, PersistenceBehavior, NamingConventions, string> ConvertToSQLConfigure { get; private set; }
}