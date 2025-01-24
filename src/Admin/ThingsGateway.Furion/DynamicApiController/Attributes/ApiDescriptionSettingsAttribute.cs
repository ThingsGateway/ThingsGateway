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

using ThingsGateway.DynamicApiController;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// 接口描述设置
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ApiDescriptionSettingsAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiDescriptionSettingsAttribute() : base()
    {
        Order = 0;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public ApiDescriptionSettingsAttribute(bool enabled) : base()
    {
        IgnoreApi = !enabled;
        Order = 0;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="groups">分组列表</param>
    public ApiDescriptionSettingsAttribute(params string[] groups) : base()
    {
        GroupName = string.Join(Penetrates.GroupSeparator, groups);
        Groups = groups;
        Order = 0;
    }

    /// <summary>
    /// 自定义名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 保留原有名称（Boolean 类型）
    /// </summary>
    public object KeepName { get; set; }

    /// <summary>
    /// 切割骆驼命名（Boolean 类型）
    /// </summary>
    public object SplitCamelCase { get; set; }

    /// <summary>
    /// 小驼峰命名（首字符小写）
    /// </summary>
    public object AsLowerCamelCase { get; set; }

    /// <summary>
    /// 保留路由谓词（Boolean 类型）
    /// </summary>
    public object KeepVerb { get; set; }

    /// <summary>
    /// 小写路由（Boolean 类型）
    /// </summary>
    public object LowercaseRoute { get; set; }

    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 分组
    /// </summary>
    public string[] Groups { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 配置控制器区域（只对控制器有效）
    /// </summary>
    public string Area { get; set; }

    /// <summary>
    /// 额外描述，支持 HTML
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 强制携带路由前缀，即使使用 [Route] 重写，仅对 Class/Controller 有效
    /// </summary>
    public object ForceWithRoutePrefix { get; set; }

    /// <summary>
    /// 禁止子类继承
    /// </summary>
    public bool DisableInherite { get; set; } = false;

    /// <summary>
    /// 分组名
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    /// 是否忽略 API
    /// </summary>
    public bool IgnoreApi { get; set; }
}