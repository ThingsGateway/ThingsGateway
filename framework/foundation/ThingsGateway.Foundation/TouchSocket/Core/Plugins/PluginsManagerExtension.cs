#region copyright
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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// PluginsManagerExtension
    /// </summary>
    public static class PluginsManagerExtension
    {
        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin">插件类型</typeparam>
        /// <returns>插件类型实例</returns>
        public static TPlugin Add<TPlugin>(this IPluginsManager pluginsManager) where TPlugin : class, IPlugin
        {
            return (TPlugin)pluginsManager.Add(typeof(TPlugin));
        }

        /// <summary>
        /// 添加插件委托
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="pluginsManager"></param>
        /// <param name="name"></param>
        /// <param name="func"></param>
        public static void Add<TSender, TEventArgs>(this IPluginsManager pluginsManager, string name, Func<TSender, TEventArgs, Task> func) where TEventArgs : PluginEventArgs
        {
            Task newFunc(object sender, TouchSocketEventArgs e)
            {
                return func((TSender)sender, (TEventArgs)e);
            }
            pluginsManager.Add(name, newFunc);
        }

        /// <summary>
        /// 添加插件委托
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="pluginsManager"></param>
        /// <param name="name"></param>
        /// <param name="func"></param>
        public static void Add<TEventArgs>(this IPluginsManager pluginsManager, string name, Func<TEventArgs, Task> func) where TEventArgs : PluginEventArgs
        {
            Task newFunc(object sender, TouchSocketEventArgs e)
            {
                return func((TEventArgs)e);
            }
            pluginsManager.Add(name, newFunc);
        }

        /// <summary>
        /// 添加插件委托
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="name"></param>
        /// <param name="func"></param>
        public static void Add(this IPluginsManager pluginsManager, string name, Func<Task> func)
        {
            async Task newFunc(object sender, PluginEventArgs e)
            {
                await func();
                await e.InvokeNext();
            }
            pluginsManager.Add(name, newFunc);
        }

        /// <summary>
        /// 添加插件委托
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void Add<T>(this IPluginsManager pluginsManager, string name, Action<T> action) where T : class
        {
            if (typeof(PluginEventArgs).IsAssignableFrom(typeof(T)))
            {
                async Task newFunc(object sender, PluginEventArgs e)
                {
                    action(e as T);
                    await e.InvokeNext();
                }
                pluginsManager.Add(name, newFunc);
            }
            else
            {
                async Task newFunc(object sender, PluginEventArgs e)
                {
                    action((T)sender);
                    await e.InvokeNext();
                }
                pluginsManager.Add(name, newFunc);
            }
        }

        /// <summary>
        /// 添加插件委托
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void Add(this IPluginsManager pluginsManager, string name, Action action)
        {
            async Task newFunc(object sender, PluginEventArgs e)
            {
                action();
                await e.InvokeNext();
            }
            pluginsManager.Add(name, newFunc);
        }
    }
}