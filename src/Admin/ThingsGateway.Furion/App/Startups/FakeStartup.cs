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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable CA1822 // 将成员标记为 static

namespace ThingsGateway;

/// <summary>
/// 模拟 Startup，解决 .NET5 下不设置 UseStartup 时出现异常问题
/// </summary>
[SuppressSniffer]
public sealed class FakeStartup
{
    /// <summary>
    /// 配置服务
    /// </summary>
    public void ConfigureServices(IServiceCollection _)
    {
    }

    /// <summary>
    /// 配置请求
    /// </summary>
    public void Configure(IApplicationBuilder _)
    {
    }
}