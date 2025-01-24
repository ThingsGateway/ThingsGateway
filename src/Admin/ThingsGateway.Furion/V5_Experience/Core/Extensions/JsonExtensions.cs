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

using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;

namespace ThingsGateway.Extensions;

/// <summary>
///     System.Text.Json 拓展类
/// </summary>
internal static class JsonExtensions
{
    /// <summary>
    ///     将 <see cref="JsonNode" /> 转换为目标类型
    /// </summary>
    /// <param name="jsonNode">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    internal static TResult?
        As<TResult>(this JsonNode? jsonNode, JsonSerializerOptions? jsonSerializerOptions = null) =>
        (TResult?)jsonNode.As(typeof(TResult), jsonSerializerOptions);

    /// <summary>
    ///     将 <see cref="JsonNode" /> 转换为目标类型
    /// </summary>
    /// <param name="jsonNode">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    internal static object? As(this JsonNode? jsonNode, Type resultType,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(resultType);

        // 空检查
        if (jsonNode is null)
        {
            return null;
        }

        // 处理目标类型为字符串类型
        if (resultType == typeof(string))
        {
            // 处理转换为字符串类型出现双引号包裹问题
            return jsonNode.GetValueKind() is JsonValueKind.String
                ? jsonNode.GetValue<string>()
                : jsonNode.ToJsonString(jsonSerializerOptions);
        }

        // 处理目标类型为 bool 且值是 "True" 或 "False" 情况
        if (resultType == typeof(bool) && jsonNode.GetValueKind() is JsonValueKind.String)
        {
            // 获取字符串值
            var stringValue = jsonNode.GetValue<string>();

            // 检查字符串是否是 "True" 或 "False"
            if (stringValue == bool.TrueString || stringValue == bool.FalseString)
            {
                return Convert.ToBoolean(stringValue);
            }
        }

        // 处理目标类型为 XElement 类型
        if (resultType == typeof(XElement))
        {
            // 初始化 MemoryStream 实例
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonNode.ToJsonString(jsonSerializerOptions)));

            // 使用 JsonReaderWriterFactory 创建 JsonReader 实例用于解析 JSON 数据
            using var jsonReader =
                JsonReaderWriterFactory.CreateJsonReader(ms, XmlDictionaryReaderQuotas.Max);

            // 将 JsonReader 解析的结果加载到 XElement 实例中
            return XElement.Load(jsonReader);
        }

        // 初始化 MemoryStream 实例
        using var memoryStream = new MemoryStream();

        // 初始化 Utf8JsonWriter 实例
        // 注意：如果使用 using var jsonWriter = ...; 代码方式，则需要手动调用 jsonWriter.Flush(); 方法来确保所有数据都被写入
        using (var jsonWriter = new Utf8JsonWriter(memoryStream))
        {
            // 将 jsonNode 的内容写入到 jsonWriter 中
            jsonNode.WriteTo(jsonWriter);
        }

        // 反序列化输出目标类型实例
        return JsonSerializer.Deserialize(memoryStream.ToArray(), resultType, jsonSerializerOptions);
    }

    /// <summary>
    ///     将 <see cref="JsonNode" /> 转换为数值类型的值
    /// </summary>
    /// <param name="jsonNode">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static object GetNumericValue(this JsonNode jsonNode)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(jsonNode);

        // 定义一个小的误差范围（容差值）
        const double epsilon = 1e-10;

        // 将 JsonNode 转换为 JsonValue
        var jsonValue = jsonNode.AsValue();

        // 尝试将 JsonValue 转换为 double 类型
        if (jsonValue.TryGetValue<double>(out var doubleValue))
        {
            // 检查双精度浮点数与四舍五入后的整数值之间的差异是否大于容差值，如果是则认定这是一个真正的浮点数
            if (Math.Abs(doubleValue - Math.Round(doubleValue)) >= epsilon)
            {
                return doubleValue;
            }

            // 根据数值范围和精度损失情况决定返回 int, long 还是保持原样返回 double
            switch (doubleValue)
            {
                case >= int.MinValue and <= int.MaxValue:
                    var intValue = (int)doubleValue;

                    if (Math.Abs(intValue - doubleValue) < epsilon)
                    {
                        return intValue;
                    }

                    break;
                case >= long.MinValue and <= long.MaxValue:
                    var longValue = (long)doubleValue;

                    if (Math.Abs(longValue - doubleValue) < epsilon)
                    {
                        return longValue;
                    }

                    break;
            }

            return doubleValue;
        }

        // 尝试将 JsonValue 转换为 decimal 类型
        if (jsonValue.TryGetValue<decimal>(out var decimalValue))
        {
            return decimalValue;
        }

        throw new InvalidCastException(
            $"The value `{jsonValue.ToJsonString()}` cannot be converted to a supported numeric type.");
    }

    /// <summary>
    ///     根据提供的命名策略转换 JSON 节点中的对象键名
    /// </summary>
    /// <param name="jsonNode">
    ///     <see cref="JsonNode" />
    /// </param>
    /// <param name="jsonNamingPolicy">
    ///     <see cref="JsonNamingPolicy" />
    /// </param>
    /// <returns>
    ///     <see cref="JsonNode" />
    /// </returns>
    internal static JsonNode? TransformKeysWithNamingPolicy(this JsonNode? jsonNode, JsonNamingPolicy jsonNamingPolicy)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(jsonNamingPolicy);

        switch (jsonNode)
        {
            // 处理 JsonObject 类型
            case JsonObject jsonObject:
                // 初始化新的 JsonObject 实例
                var transformedObject = new JsonObject();

                // 遍历原对象的所有属性，并对每个属性的键名应用命名策略转换
                foreach (var property in jsonObject)
                {
                    // 根据命名策略转换键名
                    var transformedKey = jsonNamingPolicy.ConvertName(property.Key);

                    transformedObject[transformedKey] = property.Value.TransformKeysWithNamingPolicy(jsonNamingPolicy);
                }

                return transformedObject;
            // 处理 JsonArray 类型
            case JsonArray jsonArray:
                // 初始化新的 JsonArray 实例
                var transformedArray = new JsonArray();

                // 遍历数组中的每一项并处理可能存在的嵌套对象或数组情况
                foreach (var item in jsonArray)
                {
                    transformedArray.Add(item.TransformKeysWithNamingPolicy(jsonNamingPolicy));
                }

                return transformedArray;
            // 其他类型直接返回
            default:
                return jsonNode?.DeepClone();
        }
    }
}