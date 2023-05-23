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
    /// Ajax服务类
    /// </summary>
    public class AjaxService
    {
        /// <summary>
        /// 获得 回调委托缓存集合
        /// </summary>
        private List<(IComponent Key, Func<AjaxOption, Task<string>> Callback)> Cache { get; } = new();

        /// <summary>
        /// 获得 跳转其他页面的回调委托缓存集合
        /// </summary>
        private List<(IComponent Key, Func<string, Task> Callback)> GotoCache { get; } = new();

        /// <summary>
        /// 调用Ajax方法发送请求
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public async Task<string> GetMessageAsync(AjaxOption option)
        {
            var cb = Cache.FirstOrDefault().Callback;
            return cb == null ? null : await cb.Invoke(option);
        }

        /// <summary>
        /// 调用 Goto 方法跳转其他页面
        /// </summary>
        /// <param name="url"></param>
        public async Task GotoAsync(string url)
        {
            var cb = GotoCache.FirstOrDefault().Callback;
            if (cb != null)
            {
                await cb.Invoke(url);
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        internal void Register(IComponent key, Func<AjaxOption, Task<string>> callback) => Cache.Add((key, callback));

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        internal void RegisterGoto(IComponent key, Func<string, Task> callback) => GotoCache.Add((key, callback));

        /// <summary>
        /// 注销事件
        /// </summary>
        internal void UnRegister(IComponent key)
        {
            var item = Cache.FirstOrDefault(i => i.Key == key);
            if (item.Key != null)
            {
                Cache.Remove(item);
            }
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        internal void UnRegisterGoto(IComponent key)
        {
            var item = GotoCache.FirstOrDefault(i => i.Key == key);
            if (item.Key != null)
            {
                GotoCache.Remove(item);
            }
        }
    }
}