//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
using System.Text.Json;

#endif

using Newtonsoft.Json;

using System.Text;

namespace ThingsGateway.Foundation;

#if NET6_0_OR_GREATER

/// <inheritdoc/>
public class EncodingConverter : System.Text.Json.Serialization.JsonConverter<Encoding>
{
    /// <inheritdoc/>
    public override Encoding? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 从 JSON 中读取编码名称，并创建对应的 Encoding 对象
        string? encodingName = reader.GetString();
        return Encoding.GetEncoding(encodingName ?? Encoding.UTF8.WebName);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Encoding value, JsonSerializerOptions options)
    {
        // 将 Encoding 对象的编码名称作为字符串写入 JSON
        writer.WriteStringValue(value?.WebName);
    }
}
#endif

/// <inheritdoc/>
public class NewtonsoftEncodingConverter : Newtonsoft.Json.JsonConverter<Encoding>
{
    /// <inheritdoc/>
    public override Encoding? ReadJson(JsonReader reader, Type objectType, Encoding? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        // 从 JSON 字符串中读取编码名称，并创建相应的 Encoding 对象
        string? encodingName = reader.Value as string;
        return Encoding.GetEncoding(encodingName ?? Encoding.UTF8.WebName);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, Encoding? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        writer.WriteValue(value.WebName);
    }
}
