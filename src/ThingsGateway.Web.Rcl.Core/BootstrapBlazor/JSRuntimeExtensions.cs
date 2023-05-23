#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

namespace ThingsGateway.Web.Rcl.Core
{
    /// <summary>
    /// JSRuntime 扩展操作类
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class JSRuntimeExtensions
    {
        /// <summary>
        /// 调用 JSInvoke 方法
        /// </summary>
        /// <param name="jsRuntime">IJSRuntime 实例</param>
        /// <param name="el">Element 实例或者组件 Id</param>
        /// <param name="func">Javascript 方法</param>
        /// <param name="args">Javascript 参数</param>
        public static async ValueTask<TValue> InternalInvokeAsync<TValue>(this IJSRuntime jsRuntime, string func = null, params object[] args)
        {
            TValue ret = default;
            var paras = new List<object>();

            if (args != null)
            {
                paras.AddRange(args);
            }

            try
            {
                ret = await jsRuntime.InvokeAsync<TValue>($"$.{func}", paras.ToArray()).ConfigureAwait(false);
            }
            catch (JSDisconnectedException) { }
            catch (JSException) { }
            catch (AggregateException) { }
            catch (InvalidOperationException) { }
            catch (TaskCanceledException) { }
            return ret;
        }

        /// <summary>
        /// 调用 JSInvoke 方法
        /// </summary>
        /// <param name="jsRuntime">IJSRuntime 实例</param>
        /// <param name="el">Element 实例或者组件 Id</param>
        /// <param name="func">Javascript 方法</param>
        /// <param name="args">Javascript 参数</param>
        public static async ValueTask InternalInvokeVoidAsync(this IJSRuntime jsRuntime, string func = null, params object[] args)
        {
            var paras = new List<object>();
            if (args != null)
            {
                paras.AddRange(args);
            }
            try
            {
                await jsRuntime.InvokeVoidAsync($"{func}", paras.ToArray()).ConfigureAwait(false);
            }
            catch (JSDisconnectedException) { }
            catch (JSException) { }
            catch (AggregateException) { }
            catch (InvalidOperationException) { }
            catch (TaskCanceledException) { }
        }
    }
}