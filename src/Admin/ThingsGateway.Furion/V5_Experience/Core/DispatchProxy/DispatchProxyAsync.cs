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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable

using static System.Reflection.AsyncDispatchProxyGenerator;

namespace System.Reflection;

public abstract class DispatchProxyAsync
{
    public static T Create<T, TProxy>() where TProxy : DispatchProxyAsync =>
        (T)CreateProxyInstance(typeof(TProxy), typeof(T));

    public static object Create(Type type, Type proxyType) =>
        CreateProxyInstance(proxyType, type);

    public abstract object Invoke(MethodInfo method, object[] args);

    public abstract Task InvokeAsync(MethodInfo method, object[] args);

    public abstract Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args);
}