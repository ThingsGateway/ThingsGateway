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

namespace ThingsGateway.Extensions;

/// <summary>
///     数值类型拓展类
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    ///     根据指定的单位将字节数进行转换
    /// </summary>
    /// <param name="byteSize">字节数</param>
    /// <param name="unit">单位。可选值为：<c>B</c>, <c>KB</c>, <c>MB</c>, <c>GB</c>, <c>TB</c>, <c>PB</c>, <c>EB</c>。</param>
    /// <returns>
    ///     <see cref="double" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static double ToSizeUnits(this double byteSize, string unit)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        // 非负检查
        if (byteSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteSize),
                $"The `{nameof(byteSize)}` must be non-negative.");
        }

        return unit.ToUpperInvariant() switch
        {
            "B" => byteSize,
            "KB" => byteSize / 1024.0,
            "MB" => byteSize / (1024.0 * 1024),
            "GB" => byteSize / (1024.0 * 1024 * 1024),
            "TB" => byteSize / (1024.0 * 1024 * 1024 * 1024),
            "PB" => byteSize / (1024.0 * 1024 * 1024 * 1024 * 1024),
            "EB" => byteSize / (1024.0 * 1024 * 1024 * 1024 * 1024 * 1024),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), $"Unsupported `{unit}` unit.")
        };
    }

    /// <summary>
    ///     根据指定的单位将字节数进行转换
    /// </summary>
    /// <param name="byteSize">字节数</param>
    /// <param name="unit">单位。可选值为：<c>B</c>, <c>KB</c>, <c>MB</c>, <c>GB</c>, <c>TB</c>, <c>PB</c>, <c>EB</c>。</param>
    /// <returns>
    ///     <see cref="double" />
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static double ToSizeUnits(this long byteSize, string unit) => ToSizeUnits((double)byteSize, unit);
}