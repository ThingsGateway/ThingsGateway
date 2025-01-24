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

using ThingsGateway;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// <see cref="IConfiguration"/> 拓展
/// </summary>
public static class IConfigurationExtenstions
{
    /// <summary>
    /// 刷新配置对象
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IConfiguration Reload(this IConfiguration configuration)
    {
        if (App.RootServices == null) return configuration;

        var newConfiguration = App.GetService<IConfiguration>(App.RootServices);
        InternalApp.Configuration = newConfiguration;

        return newConfiguration;
    }
}