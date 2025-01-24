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
/// SQL 类型
/// </summary>
/// <remarks>用于控制生成 SQL 格式</remarks>
[SuppressSniffer]
public enum SqlTypes
{
    /// <summary>
    /// 标准 SQL
    /// </summary>
    Standard = 0,

    /// <summary>
    /// SqlServer
    /// </summary>
    SqlServer = 1,

    /// <summary>
    /// Sqlite
    /// </summary>
    Sqlite = 2,

    /// <summary>
    /// MySql
    /// </summary>
    MySql = 3,

    /// <summary>
    /// PostgresSQL
    /// </summary>
    PostgresSQL = 4,

    /// <summary>
    /// Oracle
    /// </summary>
    Oracle = 5,

    /// <summary>
    /// Firebird
    /// </summary>
    Firebird = 6
}