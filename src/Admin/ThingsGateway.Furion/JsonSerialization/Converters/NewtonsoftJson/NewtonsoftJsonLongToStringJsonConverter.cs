﻿// ------------------------------------------------------------------------
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
using Newtonsoft.Json.Linq;

namespace ThingsGateway.JsonSerialization;

/// <summary>
/// 解决 long 精度问题
/// </summary>
[SuppressSniffer]
public class NewtonsoftJsonLongToStringJsonConverter : JsonConverter<long>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public NewtonsoftJsonLongToStringJsonConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="overMaxLengthOf17"></param>
    public NewtonsoftJsonLongToStringJsonConverter(bool overMaxLengthOf17 = false)
    {
        OverMaxLengthOf17 = overMaxLengthOf17;
    }

    /// <summary>
    /// 是否超过最大长度 17 再处理
    /// </summary>
    public bool OverMaxLengthOf17 { get; set; }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jt = JValue.ReadFrom(reader);

        return jt.Type == JTokenType.Null   // 处理 public long? Property { get; set;} = 0; 情况，也就是类型是 long? 但是也给了默认值
            ? existingValue
            : jt.Value<long>();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
    {
        if (OverMaxLengthOf17)
        {
            if (value.ToString().Length <= 17) writer.WriteValue(value);
            else writer.WriteValue(value.ToString());
        }
        else writer.WriteValue(value.ToString());
    }
}

/// <summary>
/// 解决 long? 精度问题
/// </summary>
[SuppressSniffer]
public class NewtonsoftJsonNullableLongToStringJsonConverter : JsonConverter<long?>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public NewtonsoftJsonNullableLongToStringJsonConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="overMaxLengthOf17"></param>
    public NewtonsoftJsonNullableLongToStringJsonConverter(bool overMaxLengthOf17 = false)
    {
        OverMaxLengthOf17 = overMaxLengthOf17;
    }

    /// <summary>
    /// 是否超过最大长度 17 再处理
    /// </summary>
    public bool OverMaxLengthOf17 { get; set; }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override long? ReadJson(JsonReader reader, Type objectType, long? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jt = JValue.ReadFrom(reader);
        return jt.Value<long?>();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, long? value, JsonSerializer serializer)
    {
        if (value == null) writer.WriteNull();
        else
        {
            var newValue = value.Value;
            if (OverMaxLengthOf17)
            {
                if (newValue.ToString().Length <= 17) writer.WriteValue(newValue);
                else writer.WriteValue(newValue.ToString());
            }
            else writer.WriteValue(newValue.ToString());
        }
    }
}