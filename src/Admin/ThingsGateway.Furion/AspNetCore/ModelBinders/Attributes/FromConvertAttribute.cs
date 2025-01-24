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

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// 自定义参数绑定转换特性
/// </summary>
/// <remarks>供模型绑定使用</remarks>
[SuppressSniffer, AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FromConvertAttribute : Attribute
{
    /// <summary>
    /// 是否允许空字符串
    /// </summary>
    public bool AllowStringEmpty { get; set; } = false;

    /// <summary>
    /// 模型转换绑定器
    /// </summary>
    public Type ModelConvertBinder { get; set; }

    /// <summary>
    /// 额外数据
    /// </summary>
    public object Extras { get; set; }

    /// <summary>
    /// 完全自定义
    /// </summary>
    /// <remarks>框架内部不做任何处理</remarks>
    public bool Customize { get; set; } = false;
}