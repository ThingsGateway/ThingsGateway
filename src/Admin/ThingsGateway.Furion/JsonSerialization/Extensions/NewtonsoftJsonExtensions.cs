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

using ThingsGateway.JsonSerialization;

namespace Newtonsoft.Json;

/// <summary>
/// Newtonsoft.Json 拓展
/// </summary>
[SuppressSniffer]
public static class NewtonsoftJsonExtensions
{
    /// <summary>
    /// 添加 DateTime/DateTime?/DateTimeOffset/DateTimeOffset? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <param name="localized">自动转换 DateTime/DateTimeOffset 为当地时间</param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateTimeTypeConverters(this IList<JsonConverter> converters, string outputFormat = "yyyy-MM-dd HH:mm:ss", bool localized = false)
    {
        converters.Add(new NewtonsoftJsonDateTimeJsonConverter(outputFormat, localized));
        converters.Add(new NewtonsoftNullableJsonDateTimeJsonConverter(outputFormat, localized));

        converters.Add(new NewtonsoftJsonDateTimeOffsetJsonConverter(outputFormat, localized));
        converters.Add(new NewtonsoftJsonNullableDateTimeOffsetJsonConverter(outputFormat, localized));

        return converters;
    }

    /// <summary>
    /// 添加 long/long? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="overMaxLengthOf17">是否超过最大长度 17 再处理</param>
    /// <remarks></remarks>
    public static IList<JsonConverter> AddLongTypeConverters(this IList<JsonConverter> converters, bool overMaxLengthOf17 = false)
    {
        converters.Add(new NewtonsoftJsonLongToStringJsonConverter(overMaxLengthOf17));
        converters.Add(new NewtonsoftJsonNullableLongToStringJsonConverter(overMaxLengthOf17));

        return converters;
    }


    /// <summary>
    /// 添加 DateOnly/DateOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateOnlyConverters(this IList<JsonConverter> converters, string outputFormat = "yyyy-MM-dd")
    {
        converters.Add(new NewtonsoftJsonDateOnlyJsonConverter(outputFormat));
        converters.Add(new NewtonsoftJsonNullableDateOnlyJsonConverter(outputFormat));

        return converters;
    }

    /// <summary>
    /// 添加 TimeOnly/TimeOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddTimeOnlyConverters(this IList<JsonConverter> converters, string outputFormat = "HH:mm:ss")
    {
        converters.Add(new NewtonsoftJsonTimeOnlyJsonConverter(outputFormat));
        converters.Add(new NewtonsoftJsonNullableTimeOnlyJsonConverter(outputFormat));

        return converters;
    }
}