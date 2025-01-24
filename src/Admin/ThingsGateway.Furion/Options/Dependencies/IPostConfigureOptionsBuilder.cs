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

using Microsoft.Extensions.Options;

namespace ThingsGateway.Options;

/// <summary>
/// 选项后期配置依赖接口
/// </summary>
/// <typeparam name="TOptions">选项类型</typeparam>
[OptionsBuilderMethodMap(nameof(OptionsBuilder<TOptions>.PostConfigure), true)]
public interface IPostConfigureOptionsBuilder<TOptions> : IOptionsBuilderDependency<TOptions>
    where TOptions : class
{
    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options">选项实例</param>
    void PostConfigure(TOptions options);
}

/// <summary>
/// 选项后期配置依赖接口
/// </summary>
/// <typeparam name="TOptions">选项类型</typeparam>
/// <typeparam name="TDep">依赖服务</typeparam>
[OptionsBuilderMethodMap(nameof(OptionsBuilder<TOptions>.PostConfigure), true)]
public interface IPostConfigureOptionsBuilder<TOptions, TDep> : IOptionsBuilderDependency<TOptions>
    where TOptions : class
    where TDep : class
{
    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options">选项实例</param>
    /// <param name="dep">依赖服务</param>
    void PostConfigure(TOptions options, TDep dep);
}

/// <summary>
/// 选项后期配置依赖接口
/// </summary>
/// <typeparam name="TOptions">选项类型</typeparam>
/// <typeparam name="TDep1">依赖服务</typeparam>
/// <typeparam name="TDep2">依赖服务</typeparam>
[OptionsBuilderMethodMap(nameof(OptionsBuilder<TOptions>.PostConfigure), true)]
public interface IPostConfigureOptionsBuilder<TOptions, TDep1, TDep2> : IOptionsBuilderDependency<TOptions>
    where TOptions : class
    where TDep1 : class
    where TDep2 : class
{
    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options">选项实例</param>
    /// <param name="dep1">依赖服务</param>
    /// <param name="dep2">依赖服务</param>
    void PostConfigure(TOptions options
        , TDep1 dep1
        , TDep2 dep2);
}

/// <summary>
/// 选项后期配置依赖接口
/// </summary>
/// <typeparam name="TOptions">选项类型</typeparam>
/// <typeparam name="TDep1">依赖服务</typeparam>
/// <typeparam name="TDep2">依赖服务</typeparam>
/// <typeparam name="TDep3">依赖服务</typeparam>
[OptionsBuilderMethodMap(nameof(OptionsBuilder<TOptions>.PostConfigure), true)]
public interface IPostConfigureOptionsBuilder<TOptions, TDep1, TDep2, TDep3> : IOptionsBuilderDependency<TOptions>
    where TOptions : class
    where TDep1 : class
    where TDep2 : class
    where TDep3 : class
{
    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options">选项实例</param>
    /// <param name="dep1">依赖服务</param>
    /// <param name="dep2">依赖服务</param>
    /// <param name="dep3">依赖服务</param>
    void PostConfigure(TOptions options
        , TDep1 dep1
        , TDep2 dep2
        , TDep3 dep3);
}

/// <summary>
/// 选项后期配置依赖接口
/// </summary>
/// <typeparam name="TOptions">选项类型</typeparam>
/// <typeparam name="TDep1">依赖服务</typeparam>
/// <typeparam name="TDep2">依赖服务</typeparam>
/// <typeparam name="TDep3">依赖服务</typeparam>
/// <typeparam name="TDep4">依赖服务</typeparam>
[OptionsBuilderMethodMap(nameof(OptionsBuilder<TOptions>.PostConfigure), true)]
public interface IPostConfigureOptionsBuilder<TOptions, TDep1, TDep2, TDep3, TDep4> : IOptionsBuilderDependency<TOptions>
    where TOptions : class
    where TDep1 : class
    where TDep2 : class
    where TDep3 : class
    where TDep4 : class
{
    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options">选项实例</param>
    /// <param name="dep1">依赖服务</param>
    /// <param name="dep2">依赖服务</param>
    /// <param name="dep3">依赖服务</param>
    /// <param name="dep4">依赖服务</param>
    void PostConfigure(TOptions options
        , TDep1 dep1
        , TDep2 dep2
        , TDep3 dep3
        , TDep4 dep4);
}

/// <summary>
/// 选项后期配置依赖接口
/// </summary>
/// <typeparam name="TOptions">选项类型</typeparam>
/// <typeparam name="TDep1">依赖服务</typeparam>
/// <typeparam name="TDep2">依赖服务</typeparam>
/// <typeparam name="TDep3">依赖服务</typeparam>
/// <typeparam name="TDep4">依赖服务</typeparam>
/// <typeparam name="TDep5">依赖服务</typeparam>
[OptionsBuilderMethodMap(nameof(OptionsBuilder<TOptions>.PostConfigure), true)]
public interface IPostConfigureOptionsBuilder<TOptions, TDep1, TDep2, TDep3, TDep4, TDep5> : IOptionsBuilderDependency<TOptions>
    where TOptions : class
    where TDep1 : class
    where TDep2 : class
    where TDep3 : class
    where TDep4 : class
    where TDep5 : class
{
    /// <summary>
    /// 选项后期配置
    /// </summary>
    /// <param name="options">选项实例</param>
    /// <param name="dep1">依赖服务</param>
    /// <param name="dep2">依赖服务</param>
    /// <param name="dep3">依赖服务</param>
    /// <param name="dep4">依赖服务</param>
    /// <param name="dep5">依赖服务</param>
    void PostConfigure(TOptions options
        , TDep1 dep1
        , TDep2 dep2
        , TDep3 dep3
        , TDep4 dep4
        , TDep5 dep5);
}