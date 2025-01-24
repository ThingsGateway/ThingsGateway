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

namespace ThingsGateway.Extensions;

/// <summary>
///     <see cref="Utf8JsonReader" /> 拓展类
/// </summary>
internal static class Utf8JsonReaderExtensions
{
    /// <summary>
    ///     获取 JSON 原始输入数据
    /// </summary>
    /// <param name="reader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal static string GetRawText(this ref Utf8JsonReader reader)
    {
        // 将 Utf8JsonReader 转换为 JsonDocument
        using var jsonDocument = JsonDocument.ParseValue(ref reader);

        return jsonDocument.RootElement.Clone().GetRawText();
    }
}