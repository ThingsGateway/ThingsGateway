//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.Core.Json.Extension;

public static class SystemTextJsonExtensions
{
    /// <summary>
    /// 默认Json规则
    /// </summary>
    public static JsonSerializerOptions Options = new JsonSerializerOptions
    {
        WriteIndented = true, // 使用缩进格式化输出
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, // 忽略空值属性
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,//NaN支持
    };

    public static object FromSystemTextJsonString(this string json, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(json, type, Options);
    }

    public static T FromSystemTextJsonString<T>(this string json)
    {
        return (T)FromSystemTextJsonString(json, typeof(T));
    }

    public static string ToSystemTextJsonString(this object item, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return System.Text.Json.JsonSerializer.Serialize(item, jsonSerializerOptions ?? Options);
    }
}
