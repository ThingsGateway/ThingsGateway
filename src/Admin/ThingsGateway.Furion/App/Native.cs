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

using Microsoft.Extensions.DependencyInjection;

using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;

using ThingsGateway;

namespace System;

/// <summary>
/// 用于原生应用（WinForm/WPF）创建窗口
/// </summary>
[SuppressSniffer]
public static class Native
{
    /// <summary>
    /// 创建原生应用（WinForm/WPF）窗口
    /// </summary>
    /// <typeparam name="TWindow"></typeparam>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static TWindow CreateInstance<TWindow>(params object[] parameters)
        where TWindow : class
    {
        return CreateInstance(typeof(TWindow), parameters) as TWindow;
    }

    /// <summary>
    /// 创建原生应用（WinForm/WPF）组件窗口
    /// </summary>
    /// <param name="windowType"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static object CreateInstance(Type windowType, params object[] parameters)
    {
        // 获取构造函数
        var constructors = windowType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        // 如果构造函数为空，则直接创建返回
        if (constructors.Length == 0) return Activator.CreateInstance(windowType);

        // 检查是否包含多个公开构造函数
        if (constructors.Length > 1) throw new InvalidOperationException($"Multiple constructors accepting all given argument types have been found in type '{windowType.Namespace}.{windowType.Name}'. There should only be one applicable constructor.");

        // 获取唯一构造函数参数
        var parameterInfos = constructors[0].GetParameters();

        // 准备构造函数参数
        var ctorParameters = new List<object>();

        // 创建服务作用域
#pragma warning disable CA2000 // 丢失范围之前释放对象
        var serviceScope = App.RootServices.CreateScope();
#pragma warning restore CA2000 // 丢失范围之前释放对象

        // 遍历构造函数参数
        for (var i = 0; i < parameterInfos.Length; i++)
        {
            var parameterInfo = parameterInfos[i];

            var serviceType = parameterInfo.ParameterType;
            object serviceInstance;

            // 获取服务注册生命周期
            var serviceLifetime = App.GetServiceLifetime(serviceType);

            // 如果构造函数不是服务类型，则直接跳出
            if (serviceLifetime == null) break;

            // 如果是单例，直接从根服务解析
            if (serviceLifetime == ServiceLifetime.Singleton)
            {
                serviceInstance = App.RootServices.GetService(serviceType);
            }
            // 否则通过作用域解析
            else
            {
                serviceInstance = serviceScope.ServiceProvider.GetService(serviceType);
            }

            ctorParameters.Add(serviceInstance);
        }

        // 创建窗口实例
        var windowInstance = Activator.CreateInstance(windowType, ctorParameters.Concat(parameters).ToArray());

        // 获取 Owner 属性并绑定关闭事件
        var ownerProperty = windowType.GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public);
        if (ownerProperty != null
            && (ownerProperty.PropertyType.FullName.StartsWith("System.Windows.Forms.Form")
                || ownerProperty.PropertyType.FullName.StartsWith("System.Windows.Window")))
        {
            var propertyType = ownerProperty.PropertyType;

            // 监听窗口关闭事件
            void ClosedHandler(object s, EventArgs e)
            {
                // 释放作用域
                serviceScope.Dispose();
            }

            var closedEventInfo = windowType.GetEvent("Closed", BindingFlags.Instance | BindingFlags.Public);
            closedEventInfo.AddEventHandler(windowInstance, new EventHandler((Action<object, EventArgs>)ClosedHandler));
        }

        return windowInstance;
    }

    private static readonly object _portLock = new();

    /// <summary>
    /// 获取一个空闲端口
    /// </summary>
    /// <returns></returns>
    public static int GetIdlePort()
    {
        const int fromPort = 10000;
        const int toPort = 65535;

        do
        {
            lock (_portLock)
            {
                var randomPort = RandomNumberGenerator.GetInt32(fromPort, toPort + 1);
                if (!IsPortInUse(randomPort))
                {
                    return randomPort;
                }
            }

            // 减少CPU资源消耗
            Thread.Sleep(10);
        } while (true);
    }

    private static bool IsPortInUse(int port)
    {
        return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == port);
    }
}
