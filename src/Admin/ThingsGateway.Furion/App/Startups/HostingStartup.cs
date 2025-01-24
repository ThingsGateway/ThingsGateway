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

using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ThingsGateway.HostingStartup))]

namespace ThingsGateway;

/// <summary>
/// 配置程序启动时自动注入
/// </summary>
[SuppressSniffer]
public sealed class HostingStartup : IHostingStartup
{
    /// <summary>
    /// 配置应用启动
    /// </summary>
    /// <param name="builder"></param>
    public void Configure(IWebHostBuilder builder)
    {
        InternalApp.ConfigureApplication(builder);
    }
}