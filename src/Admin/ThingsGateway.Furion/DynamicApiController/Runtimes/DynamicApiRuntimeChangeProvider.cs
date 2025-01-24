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

using Microsoft.AspNetCore.Mvc.ApplicationParts;

using System.Reflection;

namespace ThingsGateway.DynamicApiController;

/// <summary>
/// 动态 WebAPI 运行时感知提供器
/// </summary>
internal sealed class DynamicApiRuntimeChangeProvider : IDynamicApiRuntimeChangeProvider
{
    /// <summary>
    /// 应用程序部件管理器
    /// </summary>
    private readonly ApplicationPartManager _applicationPartManager;

    /// <summary>
    /// MVC 控制器感知提供器
    /// </summary>
    private readonly MvcActionDescriptorChangeProvider _mvcActionDescriptorChangeProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="applicationPartManager">应用程序部件管理器</param>
    /// <param name="mvcActionDescriptorChangeProvider">MVC 控制器感知提供器</param>
    public DynamicApiRuntimeChangeProvider(ApplicationPartManager applicationPartManager
        , MvcActionDescriptorChangeProvider mvcActionDescriptorChangeProvider)
    {
        _applicationPartManager = applicationPartManager;
        _mvcActionDescriptorChangeProvider = mvcActionDescriptorChangeProvider;
    }

    /// <summary>
    /// 添加程序集
    /// </summary>
    /// <param name="assemblies">程序集</param>
    public void AddAssemblies(params Assembly[] assemblies)
    {
        if (assemblies != null && assemblies.Length > 0)
        {
            foreach (var assembly in assemblies)
            {
                _applicationPartManager.ApplicationParts.Add(new AssemblyPart(assembly));
            }
        }
    }

    /// <summary>
    /// 添加程序集并立即感知变化
    /// </summary>
    /// <param name="assemblies">程序集</param>
    public void AddAssembliesWithNotifyChanges(params Assembly[] assemblies)
    {
        if (assemblies != null && assemblies.Length > 0)
        {
            AddAssemblies(assemblies);
            NotifyChanges();
        }
    }

    /// <summary>
    /// 移除程序集
    /// </summary>
    /// <param name="assemblyNames">程序集名称</param>
    public void RemoveAssemblies(params string[] assemblyNames)
    {
        if (assemblyNames != null && assemblyNames.Length > 0)
        {
            foreach (var assemblyName in assemblyNames)
            {
                var applicationPart = _applicationPartManager.ApplicationParts.FirstOrDefault(p => p.Name == assemblyName);
                if (applicationPart != null) _applicationPartManager.ApplicationParts.Remove(applicationPart);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    /// <summary>
    /// 移除程序集
    /// </summary>
    /// <param name="assemblies">程序集</param>
    public void RemoveAssemblies(params Assembly[] assemblies)
    {
        if (assemblies != null && assemblies.Length > 0)
        {
            RemoveAssemblies(assemblies.Select(ass => ass.GetName().Name).ToArray());
        }
    }

    /// <summary>
    /// 移除程序集并立即感知变化
    /// </summary>
    /// <param name="assemblyNames">程序集名称</param>
    public void RemoveAssembliesWithNotifyChanges(params string[] assemblyNames)
    {
        if (assemblyNames != null && assemblyNames.Length > 0)
        {
            RemoveAssemblies(assemblyNames);
            NotifyChanges();
        }
    }

    /// <summary>
    /// 移除程序集并立即感知变化
    /// </summary>
    /// <param name="assemblies">程序集</param>
    public void RemoveAssembliesWithNotifyChanges(params Assembly[] assemblies)
    {
        if (assemblies != null && assemblies.Length > 0)
        {
            RemoveAssemblies(assemblies);
            NotifyChanges();
        }
    }

    /// <summary>
    /// 感知变化
    /// </summary>
    public void NotifyChanges()
    {
        _mvcActionDescriptorChangeProvider.NotifyChanges();
    }
}