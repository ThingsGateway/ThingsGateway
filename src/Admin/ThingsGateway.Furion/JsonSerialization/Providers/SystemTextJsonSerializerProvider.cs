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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace ThingsGateway.JsonSerialization;

/// <summary>
/// System.Text.Json 序列化提供器（默认实现）
/// </summary>
[Injection(Order = -999)]
public class SystemTextJsonSerializerProvider : IJsonSerializerProvider, ISingleton
{
    /// <summary>
    /// 获取 JSON 配置选项
    /// </summary>
    private readonly JsonOptions _jsonOptions;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public SystemTextJsonSerializerProvider(IOptions<JsonOptions> options)
    {
        _jsonOptions = options.Value;
    }

    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <param name="value"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    public string Serialize(object value, object jsonSerializerOptions = null)
    {
        return JsonSerializer.Serialize(value, GetJsonSerializerOptions(jsonSerializerOptions));
    }

    /// <summary>
    /// 反序列化字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    public T Deserialize<T>(string json, object jsonSerializerOptions = null)
    {
        return JsonSerializer.Deserialize<T>(json, GetJsonSerializerOptions(jsonSerializerOptions));
    }

    /// <summary>
    /// 反序列化字符串
    /// </summary>
    /// <param name="json"></param>
    /// <param name="returnType"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    public object Deserialize(string json, Type returnType, object jsonSerializerOptions = null)
    {
        return JsonSerializer.Deserialize(json, returnType, GetJsonSerializerOptions(jsonSerializerOptions));
    }

    /// <summary>
    /// 返回读取全局配置的 JSON 选项
    /// </summary>
    /// <returns></returns>
    public object GetSerializerOptions()
    {
        return _jsonOptions?.JsonSerializerOptions;
    }

    /// <summary>
    /// 获取默认的序列化配置
    /// </summary>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    private JsonSerializerOptions GetJsonSerializerOptions(object jsonSerializerOptions = null)
    {
        var jsonSerializerOptionsValue = (jsonSerializerOptions ?? GetSerializerOptions() ?? new JsonSerializerOptions()) as JsonSerializerOptions;

#if !NET6_0 && !NET7_0
        if (!jsonSerializerOptionsValue.IsReadOnly && !jsonSerializerOptionsValue.PropertyNameCaseInsensitive)
        {
            // 默认不区分大小写匹配
            jsonSerializerOptionsValue.PropertyNameCaseInsensitive = true;
        }
#else
        // 默认不区分大小写匹配
        if (!jsonSerializerOptionsValue.PropertyNameCaseInsensitive) jsonSerializerOptionsValue.PropertyNameCaseInsensitive = true;
#endif

        return jsonSerializerOptionsValue;
    }
}
