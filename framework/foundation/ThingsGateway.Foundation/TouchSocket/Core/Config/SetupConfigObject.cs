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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 具有设置配置的对象
    /// </summary>
    public abstract class SetupConfigObject : ConfigObject, ISetupConfigObject
    {
        private TouchSocketConfig m_config;

        /// <inheritdoc/>
        public override TouchSocketConfig Config => this.m_config;

        /// <inheritdoc/>
        public IResolver Resolver { get; private set; }

        /// <inheritdoc/>
        public IPluginManager PluginManager { get; private set; }

        /// <inheritdoc/>
        public void Setup(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginManager?.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(this.Config);
            this.PluginManager?.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));
        }

        /// <inheritdoc/>
        public async Task SetupAsync(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            await this.PluginManager.RaiseAsync(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config)).ConfigureFalseAwait();
            this.LoadConfig(config);
            //return EasyTask.CompletedTask;
            await this.PluginManager.RaiseAsync(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config)).ConfigureFalseAwait();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.m_config = config ?? throw new ArgumentNullException(nameof(config));

            if (!config.TryGetValue(TouchSocketCoreConfigExtension.ResolverProperty, out var resolver))
            {
                if (!config.TryGetValue(TouchSocketCoreConfigExtension.RegistratorProperty, out var registrator))
                {
                    registrator = new Container();
                }

                if (!registrator.IsRegistered(typeof(ILog)))
                {
                    registrator.RegisterSingleton<ILog>(new LoggerGroup());
                }

                if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IRegistrator> actionContainer)
                {
                    actionContainer.Invoke(registrator);
                }

                resolver = registrator.BuildResolver();
            }

            IPluginManager pluginManager;
            if ((!this.Config.GetValue(TouchSocketCoreConfigExtension.NewPluginManagerProperty)) && resolver.IsRegistered<IPluginManager>())
            {
                pluginManager = resolver.Resolve<IPluginManager>();
            }
            else
            {
                pluginManager = new PluginManager(resolver);
            }

            if (this.Config.GetValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty) is Action<IPluginManager> actionPluginManager)
            {
                pluginManager.Enable = true;
                actionPluginManager.Invoke(pluginManager);
            }

            this.Logger ??= resolver.Resolve<ILog>();

            this.PluginManager = pluginManager;
            this.Resolver = resolver;
        }
    }
}