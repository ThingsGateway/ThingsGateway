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

using System.Runtime.InteropServices;

namespace ThingsGateway.Utilities;

/// <summary>
///     提供运行时实用方法
/// </summary>
public static class RuntimeUtility
{
    /// <summary>
    ///     获取操作系统描述
    /// </summary>
    public static string OSDescription => RuntimeInformation.OSDescription;

    /// <summary>
    ///     获取操作系统基本名称
    /// </summary>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string GetOSName()
    {
        // 检查操作系统是否是 Windows 平台
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "Windows";
        }

        // 检查操作系统是否是 Linux 平台
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "Linux";
        }

        // 检查操作系统是否是 macOS 平台
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "macOS";
        }

        return "Unknown";
    }
}