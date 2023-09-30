#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
using System.Text.Json;

#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endif
using System.Text;

namespace ThingsGateway.Foundation;




#if NET6_0_OR_GREATER

/// <inheritdoc/>
public class EncodingConverter : JsonConverter<Encoding>
{
    /// <inheritdoc/>
    public override Encoding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 从 JSON 中读取编码名称，并创建对应的 Encoding 对象
        string encodingName = reader.GetString();
        return Encoding.GetEncoding(encodingName);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Encoding value, JsonSerializerOptions options)
    {
        // 将 Encoding 对象的编码名称作为字符串写入 JSON
        writer.WriteStringValue(value.WebName);
    }
}
#else
/// <inheritdoc/>
public class EncodingConverter : CustomCreationConverter<Encoding>
{
    /// <inheritdoc/>
    public override Encoding Create(Type objectType)
    {
        // 在此处创建一个默认的 Encoding 对象
        return Encoding.Default;
    }

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            // 从 JSON 字符串中读取编码名称，并创建相应的 Encoding 对象
            string encodingName = (string)reader.Value;
            return Encoding.GetEncoding(encodingName);
        }
        return base.ReadJson(reader, objectType, existingValue, serializer);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is Encoding encoding)
        {
            // 将 Encoding 对象的编码名称作为字符串写入 JSON
            writer.WriteValue(encoding.WebName);
        }
        else
        {
            base.WriteJson(writer, value, serializer);
        }
    }
}

#endif