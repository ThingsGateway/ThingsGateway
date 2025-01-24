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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ThingsGateway.Logging;

/// <summary>
/// 支持忽略特定属性的 CamelCase 序列化
/// </summary>
internal sealed class CamelCasePropertyNamesContractResolverWithIgnoreProperties : CamelCasePropertyNamesContractResolver
{
    /// <summary>
    /// 被忽略的属性名称
    /// </summary>
    private readonly string[] _names;

    /// <summary>
    /// 被忽略的属性类型
    /// </summary>
    private readonly Type[] _type;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="names"></param>
    /// <param name="types"></param>
    public CamelCasePropertyNamesContractResolverWithIgnoreProperties(string[] names, Type[] types)
    {
        _names = names ?? Array.Empty<string>();
        _type = types ?? Array.Empty<Type>();
    }

    /// <summary>
    /// 重写需要序列化的属性名
    /// </summary>
    /// <param name="type"></param>
    /// <param name="memberSerialization"></param>
    /// <returns></returns>
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var allProperties = base.CreateProperties(type, memberSerialization);

        return allProperties.Where(p =>
                !_names.Contains(p.PropertyName, StringComparer.OrdinalIgnoreCase)
                && !_type.Contains(p.PropertyType)).ToList();
    }
}

/// <summary>
/// 支持忽略特定属性的 Default 序列化
/// </summary>
internal sealed class DefaultContractResolverWithIgnoreProperties : DefaultContractResolver
{
    /// <summary>
    /// 被忽略的属性名称
    /// </summary>
    private readonly string[] _names;

    /// <summary>
    /// 被忽略的属性类型
    /// </summary>
    private readonly Type[] _type;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="names"></param>
    /// <param name="types"></param>
    public DefaultContractResolverWithIgnoreProperties(string[] names, Type[] types)
    {
        _names = names ?? Array.Empty<string>();
        _type = types ?? Array.Empty<Type>();
    }

    /// <summary>
    /// 重写需要序列化的属性名
    /// </summary>
    /// <param name="type"></param>
    /// <param name="memberSerialization"></param>
    /// <returns></returns>
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var allProperties = base.CreateProperties(type, memberSerialization);

        return allProperties.Where(p =>
                !_names.Contains(p.PropertyName, StringComparer.OrdinalIgnoreCase)
                && !_type.Contains(p.PropertyType)).ToList();
    }
}