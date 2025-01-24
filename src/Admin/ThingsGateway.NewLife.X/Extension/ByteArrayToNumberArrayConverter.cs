//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace ThingsGateway.NewLife.Extension;

public class ByteArrayToNumberArrayConverter : JsonConverter<byte[]>
{
    public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        // 将 byte[] 转换为数值数组
        writer.WriteStartArray();
        foreach (var b in value)
        {
            writer.WriteValue(b);
        }
        writer.WriteEndArray();
    }

    public override byte[] ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // 从数值数组读取 byte[]
        if (reader.TokenType == JsonToken.StartArray)
        {
            var byteList = new System.Collections.Generic.List<byte>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }

                if (reader.TokenType == JsonToken.Integer)
                {
                    byteList.Add(Convert.ToByte(reader.Value));
                }
            }
            return byteList.ToArray();
        }
        throw new JsonSerializationException("Invalid JSON format for byte array.");
    }

    public override bool CanRead => true;
}
