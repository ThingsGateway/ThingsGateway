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

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// IHost 主机拓展类
/// </summary>
public static class IHostExtensions
{
    /// <summary>
    /// 获取主机启动地址
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetServerAddresses(this IHost host)
    {
        var server = host.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        return addressesFeature?.Addresses;
    }

    /// <summary>
    /// 获取主机启动地址
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static string GetServerAddress(this IHost host)
    {
        return host.GetServerAddresses()?.FirstOrDefault();
    }

    /// <summary>
    /// 获取主机启动地址
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetServerAddresses(this IServer server)
    {
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        return addressesFeature?.Addresses;
    }

    /// <summary>
    /// 获取主机启动地址
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    public static string GetServerAddress(this IServer server)
    {
        return server.GetServerAddresses()?.FirstOrDefault();
    }
}