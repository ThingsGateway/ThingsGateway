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

using System.Text.Json;

namespace ThingsGateway.JsonSerialization;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    internal static DateTime ConvertToDateTime(ref Utf8JsonReader reader)
    {
        // 处理时间戳自动转换
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var longValue))
        {
            return longValue.ToDateTime();
        };

        var stringValue = reader.GetString();

        // 处理时间戳自动转换
        if (long.TryParse(stringValue, out var longValue2))
        {
            return longValue2.ToDateTime();
        }

        return Convert.ToDateTime(stringValue);
    }

    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    internal static DateTime ConvertToDateTime(ref JsonReader reader)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            return JValue.ReadFrom(reader).Value<long>().ToDateTime();
        }

        var stringValue = JValue.ReadFrom(reader).Value<string>();

        // 处理时间戳自动转换
        if (long.TryParse(stringValue, out var longValue2))
        {
            return longValue2.ToDateTime();
        }

        return Convert.ToDateTime(stringValue);
    }
}
