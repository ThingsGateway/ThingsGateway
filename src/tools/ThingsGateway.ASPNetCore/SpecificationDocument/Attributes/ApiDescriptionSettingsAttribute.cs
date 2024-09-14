// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.ASPNetCore;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// 接口描述设置
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ApiDescriptionSettingsAttribute : ApiExplorerSettingsAttribute
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
}
