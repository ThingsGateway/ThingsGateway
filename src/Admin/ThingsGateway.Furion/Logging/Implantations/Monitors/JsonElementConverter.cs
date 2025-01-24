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
using Newtonsoft.Json.Linq;

using System.Text.Encodings.Web;

using ThingsGateway.JsonSerialization;

namespace ThingsGateway.Logging;

/// <summary>
/// 解决 JsonElement 问题
/// </summary>
[SuppressSniffer]
public class JsonElementConverter : JsonConverter<System.Text.Json.JsonElement>
{
    private static readonly System.Text.Json.JsonSerializerOptions CachedJsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static JsonElementConverter()
    {
        CachedJsonSerializerOptions.Converters.Add(new SystemTextJsonLongToStringJsonConverter());
        CachedJsonSerializerOptions.Converters.Add(new SystemTextJsonNullableLongToStringJsonConverter());
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override System.Text.Json.JsonElement ReadJson(JsonReader reader, Type objectType, System.Text.Json.JsonElement existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = JValue.ReadFrom(reader).Value<string>();
        return (System.Text.Json.JsonElement)System.Text.Json.JsonSerializer.Deserialize<object>(value);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, System.Text.Json.JsonElement value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, System.Text.Json.JsonSerializer.Serialize(value, CachedJsonSerializerOptions));
    }
}
