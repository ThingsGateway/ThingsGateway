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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TouchSocketCoreConfigExtension
    /// </summary>
    public static class TouchSocketCoreConfigExtension
    {
        #region 插件

        /// <summary>
        /// 配置插件。
        /// </summary>
        public static readonly DependencyProperty<Action<IPluginsManager>> ConfigurePluginsProperty =
            DependencyProperty<Action<IPluginsManager>>.Register("ConfigurePlugins", null);

        /// <summary>
        /// 容器
        /// </summary>
        public static readonly DependencyProperty<IPluginsManager> PluginsManagerProperty =
            DependencyProperty<IPluginsManager>.Register("PluginsManager", null);

        /// <summary>
        /// 配置插件。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig ConfigurePlugins(this TouchSocketConfig config, Action<IPluginsManager> value)
        {
            if (config.TryGetValue(ConfigurePluginsProperty, out var action))
            {
                action += value;
                config.SetValue(ConfigurePluginsProperty, action);
            }
            else
            {
                config.SetValue(ConfigurePluginsProperty, value);
            }
            return config;
        }

        /// <summary>
        /// 使用插件。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetPluginsManager(this TouchSocketConfig config, IPluginsManager value)
        {
            config.SetValue(PluginsManagerProperty, value);
            return config;
        }

        #endregion 插件

        #region 容器

        /// <summary>
        /// 配置容器注入。
        /// </summary>
        public static readonly DependencyProperty<Action<IContainer>> ConfigureContainerProperty =
            DependencyProperty<Action<IContainer>>.Register("ConfigureContainer", null);

        /// <summary>
        /// 容器
        /// </summary>
        public static readonly DependencyProperty<IContainer> ContainerProperty =
            DependencyProperty<IContainer>.Register("Container", null);

        /// <summary>
        /// 配置容器注入。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig ConfigureContainer(this TouchSocketConfig config, Action<IContainer> value)
        {
            if (config.TryGetValue(ConfigureContainerProperty, out var action))
            {
                action += value;
                config.SetValue(ConfigureContainerProperty, action);
            }
            else
            {
                config.SetValue(ConfigureContainerProperty, value);
            }
            return config;
        }

        /// <summary>
        /// 设置容器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetContainer(this TouchSocketConfig config, IContainer value)
        {
            config.SetValue(ContainerProperty, value);
            return config;
        }

        #endregion 容器
    }
}