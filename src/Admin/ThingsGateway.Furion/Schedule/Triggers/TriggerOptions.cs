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
/// 作业触发器配置选项
/// </summary>
[SuppressSniffer]
public sealed class TriggerOptions
{
    /// <summary>
    /// 构造函数
    /// </summary>
    internal TriggerOptions()
    {
    }

    /// <summary>
    /// 重写 <see cref="ConvertToSQL"/>
    /// </summary>
    public Func<string, string[], Trigger, PersistenceBehavior, NamingConventions, string> ConvertToSQL
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
    /// <see cref="ConvertToSQL"/> 静态配置
    /// </summary>
    internal static Func<string, string[], Trigger, PersistenceBehavior, NamingConventions, string> ConvertToSQLConfigure { get; private set; }
}