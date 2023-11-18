﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 插件泛型基础事件委托
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="client"></param>
    /// <param name="e"></param>
    public delegate void PluginEventHandler<TClient, TEventArgs>(TClient client, TEventArgs e) where TEventArgs : PluginEventArgs;

    /// <summary>
    /// ThingsGateway.Foundation.Core基础泛型事件委托
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="client"></param>
    /// <param name="e"></param>
    public delegate void TouchSocketEventHandler<TClient, TEventArgs>(TClient client, TEventArgs e) where TEventArgs : TouchSocketEventArgs;
}
