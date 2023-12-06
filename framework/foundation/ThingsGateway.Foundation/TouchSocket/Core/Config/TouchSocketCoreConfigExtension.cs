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
    /// TouchSocketCoreConfigExtension
    /// </summary>
    public static class TouchSocketCoreConfigExtension
    {
        #region 插件

        /// <summary>
        /// 配置插件。
        /// </summary>
        public static readonly DependencyProperty<Action<IPluginManager>> ConfigurePluginsProperty =
            DependencyProperty<Action<IPluginManager>>.Register("ConfigurePlugins", null);

        /// <summary>
        /// 是否使用新插件管理器。
        /// </summary>
        public static readonly DependencyProperty<bool> NewPluginManagerProperty =
            DependencyProperty<bool>.Register("NewPluginManager", false);

        /// <summary>
        /// 配置插件。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig ConfigurePlugins(this TouchSocketConfig config, Action<IPluginManager> value)
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
        /// 使用新的插件管理器。
        /// <para>
        /// 一般的，当在容器<see cref="IContainer"/>中注入<see cref="IPluginManager"/>时。会使用容器中的<see cref="IPluginManager"/>。
        /// 但是有时候，我们希望个别配置能够独立使用插件管理器。所以可以使用此配置。
        /// </para>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TouchSocketConfig UseNewPluginManager(this TouchSocketConfig config)
        {
            config.SetValue(NewPluginManagerProperty, true);
            return config;
        }

        #endregion 插件

        #region 容器

        /// <summary>
        /// 配置容器注入。
        /// </summary>
        public static readonly DependencyProperty<Action<IRegistrator>> ConfigureContainerProperty =
            DependencyProperty<Action<IRegistrator>>.Register("ConfigureContainer", null);

        /// <summary>
        /// 容器注册
        /// </summary>
        public static readonly DependencyProperty<IRegistrator> RegistratorProperty =
            DependencyProperty<IRegistrator>.Register("Registrator", null);

        /// <summary>
        /// 容器提供者
        /// </summary>
        public static readonly DependencyProperty<IResolver> ResolverProperty =
            DependencyProperty<IResolver>.Register("Resolver", null);


        /// <summary>
        /// 配置容器注入。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig ConfigureContainer(this TouchSocketConfig config, Action<IRegistrator> value)
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
        /// 设置<see cref="IResolver"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetResolver(this TouchSocketConfig config, IResolver value)
        {
            config.SetValue(ResolverProperty, value);
            return config;
        }

        /// <summary>
        /// 设置<see cref="IRegistrator"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetRegistrator(this TouchSocketConfig config, IRegistrator value)
        {
            config.SetValue(RegistratorProperty, value);
            return config;
        }

        #endregion 容器
    }
}