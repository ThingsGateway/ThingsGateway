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

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsGateway.Schedule;

/// <summary>
/// DateTime 类型序列化/反序列化处理
/// </summary>
internal sealed class DateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"><see cref="Utf8JsonReader"/></param>
    /// <param name="typeToConvert">需要转换的类型</param>
    /// <param name="options">序列化配置选项</param>
    /// <returns><see cref="DateTime"/></returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Convert.ToDateTime(reader.GetString());
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"><see cref="Utf8JsonWriter"/></param>
    /// <param name="value"><see cref="DateTime"/></param>
    /// <param name="options">序列化配置选项</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}