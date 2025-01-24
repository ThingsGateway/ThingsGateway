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

namespace ThingsGateway.DynamicApiController;

/// <summary>
/// 参数路由模板
/// </summary>
internal sealed class ParameterRouteTemplate
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ParameterRouteTemplate()
    {
        ControllerStartTemplates = new List<string>();
        ControllerEndTemplates = new List<string>();
        ActionStartTemplates = new List<string>();
        ActionEndTemplates = new List<string>();
    }

    /// <summary>
    /// 控制器之前的参数
    /// </summary>
    public IList<string> ControllerStartTemplates { get; set; }

    /// <summary>
    /// 控制器之后的参数
    /// </summary>
    public IList<string> ControllerEndTemplates { get; set; }

    /// <summary>
    /// 行为之前的参数
    /// </summary>
    public IList<string> ActionStartTemplates { get; set; }

    /// <summary>
    /// 行为之后的参数
    /// </summary>
    public IList<string> ActionEndTemplates { get; set; }
}