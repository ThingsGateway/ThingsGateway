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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.JsonSerialization;

/// <summary>
/// 解决 long 精度问题
/// </summary>
[SuppressSniffer]
public class SystemTextJsonLongToStringJsonConverter : JsonConverter<long>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SystemTextJsonLongToStringJsonConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="overMaxLengthOf17"></param>
    public SystemTextJsonLongToStringJsonConverter(bool overMaxLengthOf17 = false)
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
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.String
                ? long.Parse(reader.GetString())
                : reader.GetInt64();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        if (OverMaxLengthOf17)
        {
            if (value.ToString().Length <= 17) writer.WriteNumberValue(value);
            else writer.WriteStringValue(value.ToString());
        }
        else writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// 解决 long? 精度问题
/// </summary>
[SuppressSniffer]
public class SystemTextJsonNullableLongToStringJsonConverter : JsonConverter<long?>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public SystemTextJsonNullableLongToStringJsonConverter()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="overMaxLengthOf17"></param>
    public SystemTextJsonNullableLongToStringJsonConverter(bool overMaxLengthOf17 = false)
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
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.String
                ? long.Parse(reader.GetString())
                : reader.GetInt64();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value == null) writer.WriteNullValue();
        else
        {
            var newValue = value.Value;
            if (OverMaxLengthOf17)
            {
                if (newValue.ToString().Length <= 17) writer.WriteNumberValue(newValue);
                else writer.WriteStringValue(newValue.ToString());
            }
            else writer.WriteStringValue(newValue.ToString());
        }
    }
}