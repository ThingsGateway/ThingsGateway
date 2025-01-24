﻿// ------------------------------------------------------------------------
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ThingsGateway;

/// <summary>
/// Inject 配置选项
/// </summary>
public sealed class InjectOptions
{
    /// <summary>
    /// 外部程序集名称
    /// </summary>
    public string AssemblyName { get; set; }

    /// <summary>
    /// 是否自动注册 BackgroundService
    /// </summary>
    public bool AutoRegisterBackgroundService { get; set; } = true;

    /// <summary>
    /// 配置 ConfigurationScanDirectories
    /// </summary>
    /// <param name="directories"></param>
    public void ConfigurationScanDirectories(params string[] directories)
    {
        InternalConfigurationScanDirectories = directories ?? Array.Empty<string>();
    }

    /// <summary>
    /// 配置 IgnoreConfigurationFiles
    /// </summary>
    /// <param name="files"></param>
    public void IgnoreConfigurationFiles(params string[] files)
    {
        InternalIgnoreConfigurationFiles = files ?? Array.Empty<string>();
    }

    /// <summary>
    /// 配置 ConfigureAppConfiguration
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configure)
    {
        AppConfigurationConfigure = configure;
    }

    /// <summary>
    /// 配置 ConfigureAppConfiguration（Web）
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureWebAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configure)
    {
        WebAppConfigurationConfigure = configure;
    }

    /// <summary>
    /// 配置 ConfigureServices
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
    {
        ServicesConfigure = configure;
    }

    /// <summary>
    /// 配置 ConfigureServices（Web）
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureWebServices(Action<WebHostBuilderContext, IServiceCollection> configure)
    {
        WebServicesConfigure = configure;
    }

    /// <summary>
    /// 配置配置文件扫描目录
    /// </summary>
    internal static IEnumerable<string> InternalConfigurationScanDirectories { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// 配置配置文件忽略注册文件
    /// </summary>
    internal static IEnumerable<string> InternalIgnoreConfigurationFiles { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// AppConfiguration 配置
    /// </summary>
    internal static Action<HostBuilderContext, IConfigurationBuilder> AppConfigurationConfigure { get; private set; }

    /// <summary>
    /// AppConfiguration 配置（Web）
    /// </summary>
    internal static Action<WebHostBuilderContext, IConfigurationBuilder> WebAppConfigurationConfigure { get; private set; }

    /// <summary>
    /// Services 配置
    /// </summary>
    internal static Action<HostBuilderContext, IServiceCollection> ServicesConfigure { get; private set; }

    /// <summary>
    /// Services 配置（Web）
    /// </summary>
    internal static Action<WebHostBuilderContext, IServiceCollection> WebServicesConfigure { get; private set; }
}