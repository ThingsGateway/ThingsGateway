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

namespace ThingsGateway.DependencyInjection;

/// <summary>
/// 外部注册类型模型
/// </summary>
[SuppressSniffer]
public sealed class ExternalService
{
    /// <summary>
    /// 接口类型，格式："程序集名称;接口完整名称"
    /// </summary>
    public string Interface { get; set; }

    /// <summary>
    /// 实例类型，格式："程序集名称;接口完整名称"
    /// </summary>
    public string Service { get; set; }

    /// <summary>
    /// 注册类型
    /// </summary>
    public RegisterType RegisterType { get; set; }

    /// <summary>
    /// 添加服务方式，存在不添加，或继续添加
    /// </summary>
    public InjectionActions Action { get; set; } = InjectionActions.Add;

    /// <summary>
    /// 注册选项
    /// </summary>
    public InjectionPatterns Pattern { get; set; } = InjectionPatterns.All;

    /// <summary>
    /// 注册别名
    /// </summary>
    /// <remarks>多服务时使用</remarks>
    public string Named { get; set; }

    /// <summary>
    /// 排序，排序越大，则在后面注册
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 代理类型，格式："程序集名称;接口完整名称"
    /// </summary>
    public string Proxy { get; set; }
}