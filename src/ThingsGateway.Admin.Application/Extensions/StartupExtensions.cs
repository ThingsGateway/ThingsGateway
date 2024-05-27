//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Collections.Concurrent;
using System.Reflection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Application;

public static class StartupExtensions
{
    private static ConcurrentBag<AppStartup> AppStartups = new();

    /// <summary>
    /// 反射获取所有AppStartup的继承类，执行名称为第一个参数是<see cref="IServiceCollection"/>的方法
    /// </summary>
    /// <param name="service"></param>
    public static void ConfigureServices(this WebApplicationBuilder service)
    {
        AddStartups(service);
    }

    /// <summary>
    /// ConfigureServices获取的全部实例中，执行名称为第一个参数是<see cref="IApplicationBuilder"/>的方法
    /// </summary>
    public static void UseServices(this IApplicationBuilder builder)
    {
        UseStartups(AppStartups, builder);
    }

    /// <summary>
    /// 添加 Startup 自动扫描
    /// </summary>
    internal static void AddStartups(this WebApplicationBuilder builder)
    {
        App.Configuration = builder.Configuration;
        if (builder.Environment is IWebHostEnvironment webHostEnvironment)
            App.WebRootPath = webHostEnvironment.WebRootPath;
        App.ContentRootPath = builder.Environment.ContentRootPath;
        App.IsDevelopment = builder.Environment.IsDevelopment();

        // 扫描所有继承 AppStartup 的类
        var startups = App.EffectiveTypes
            .Where(u => typeof(AppStartup).IsAssignableFrom(u) && u.IsClass && !u.IsAbstract && !u.IsGenericType)
            .OrderByDescending(u => GetStartupOrder(u));

        // 注册自定义 startup
        foreach (var type in startups)
        {
            var startup = Activator.CreateInstance(type) as AppStartup;
            AppStartups.Add(startup!);

            // 获取所有符合依赖注入格式的方法，如返回值void，且第一个参数是 IServiceCollection 类型
            var serviceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(u => u.ReturnType == typeof(void)
                    && u.GetParameters().Length > 0
                    && u.GetParameters().First().ParameterType == typeof(IServiceCollection));

            if (!serviceMethods.Any()) continue;

            // 自动安装属性调用
            foreach (var method in serviceMethods)
            {
                method.Invoke(startup, new[] { builder.Services });
            }
        }
    }

    /// <summary>
    /// 批量将自定义 AppStartup 添加到 Startup.cs 的 Configure 中
    /// </summary>
    /// <param name="startups"></param>
    /// <param name="app"></param>
    private static void UseStartups(IEnumerable<AppStartup> startups, IApplicationBuilder app)
    {
        App.RootServices = app.ApplicationServices;
        App.CacheService = app.ApplicationServices.GetRequiredService<ICacheService>();

        // 遍历所有
        foreach (var startup in startups)
        {
            var type = startup.GetType();

            // 获取所有符合依赖注入格式的方法，如返回值 void，且第一个参数是 IApplicationBuilder 类型
            var configureMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(u => u.ReturnType == typeof(void)
                    && u.GetParameters().Length > 0
                    && u.GetParameters().First().ParameterType == typeof(IApplicationBuilder));

            if (!configureMethods.Any()) continue;

            // 自动安装属性调用
            foreach (var method in configureMethods)
            {
                method.Invoke(startup, ResolveMethodParameterInstances(app, method));
            }
        }
        AppStartups.Clear();
    }

    /// <summary>
    /// 获取 Startup 排序
    /// </summary>
    /// <param name="type">排序类型</param>
    /// <returns>int</returns>
    private static int GetStartupOrder(Type type)
    {
        return !type.IsDefined(typeof(AppStartupAttribute), true) ? 0 : type.GetCustomAttribute<AppStartupAttribute>(true)!.Order;
    }

    /// <summary>
    /// 解析方法参数实例
    /// </summary>
    /// <param name="app"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    private static object[] ResolveMethodParameterInstances(IApplicationBuilder app, MethodInfo method)
    {
        // 获取方法所有参数
        var parameters = method.GetParameters();
        var parameterInstances = new object[parameters.Length];
        parameterInstances[0] = app;

        // 解析服务
        for (var i = 1; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            parameterInstances[i] = app.ApplicationServices.GetRequiredService(parameter.ParameterType);
        }

        return parameterInstances;
    }
}

public abstract class AppStartup
{
}

/// <summary>
/// 注册服务启动配置
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AppStartupAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="order"></param>
    public AppStartupAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }
}
