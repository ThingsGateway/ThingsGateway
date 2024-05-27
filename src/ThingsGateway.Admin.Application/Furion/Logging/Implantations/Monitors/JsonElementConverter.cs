//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Text.Encodings.Web;

using ThingsGateway.JsonSerialization;

namespace ThingsGateway.Logging;

/// <summary>
/// 解决 JsonElement 问题
/// </summary>
public class JsonElementConverter : JsonConverter<System.Text.Json.JsonElement>
{
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
        return (System.Text.Json.JsonElement)System.Text.Json.JsonSerializer.Deserialize<object>(value!)!;
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, System.Text.Json.JsonElement value, JsonSerializer serializer)
    {
        var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        jsonSerializerOptions.Converters.Add(new SystemTextJsonLongToStringJsonConverter());
        jsonSerializerOptions.Converters.Add(new SystemTextJsonNullableLongToStringJsonConverter());

        serializer.Serialize(writer, System.Text.Json.JsonSerializer.Serialize(value, jsonSerializerOptions));
    }
}
