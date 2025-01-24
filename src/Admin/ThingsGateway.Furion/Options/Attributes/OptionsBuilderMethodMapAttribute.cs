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

namespace ThingsGateway.Options;

/// <summary>
/// 选项构建器方法映射特性
/// </summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
internal sealed class OptionsBuilderMethodMapAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="methodName">映射方法名</param>
    /// <param name="voidReturn">无返回值</param>
    internal OptionsBuilderMethodMapAttribute(string methodName, bool voidReturn)
    {
        MethodName = methodName;
        VoidReturn = voidReturn;
    }

    /// <summary>
    /// 方法名称
    /// </summary>
    internal string MethodName { get; set; }

    /// <summary>
    /// 有无返回值
    /// </summary>
    internal bool VoidReturn { get; set; }
}