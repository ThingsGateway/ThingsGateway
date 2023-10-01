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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 重连插件
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = true)]
    public sealed class ReconnectionPlugin<TClient> : PluginBase where TClient : class, ITcpClient
    {
        /// <inheritdoc/>
        protected override void Loaded(IPluginsManager pluginsManager)
        {
            base.Loaded(pluginsManager);
            pluginsManager.Add<object, ConfigEventArgs>(nameof(ILoadedConfigPlugin.OnLoadedConfig), this.OnLoadedConfig);
            pluginsManager.Add<TClient, DisconnectEventArgs>(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this.OnTcpDisconnected);
        }
        private bool m_polling;

        /// <summary>
        /// 每个周期可执行的委托。用于检验客户端活性。返回true表示存活，返回
        /// </summary>
        public Func<TClient, int, Task<bool?>> ActionForCheck { get; set; }

        /// <summary>
        /// ActionForConnect
        /// </summary>
        public Func<TClient, Task<bool>> ActionForConnect { get; set; }

        /// <summary>
        /// 检验时间间隔
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(1);

        private Task OnLoadedConfig(object sender, ConfigEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!this.m_polling)
                {
                    return;
                }
                if (sender is TClient client)
                {
                    var failCount = 0;
                    while (true)
                    {
                        if (this.DisposedValue)
                        {
                            return;
                        }
                        await Task.Delay(this.Tick);
                        try
                        {
                            var b = await this.ActionForCheck.Invoke(client, failCount);
                            if (b == null)
                            {
                                continue;
                            }
                            else if (b == false)
                            {
                                if (await this.ActionForConnect.Invoke(client))
                                {
                                    failCount = 0;
                                }
                            }
                            else
                            {
                                failCount = 0;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            });

            return e.InvokeNext();
        }

        private Task OnTcpDisconnected(TClient client, DisconnectEventArgs e)
        {
            Task.Run(async () =>
            {
                if (e.Manual)
                {
                    return;
                }

                while (true)
                {
                    if (await this.ActionForConnect.Invoke(client))
                    {
                        return;
                    }
                }
            });

            return e.InvokeNext();
        }

        /// <summary>
        /// 每个周期可执行的委托。返回值为True标识客户端存活。返回False，表示失活，立即重连。返回null时，表示跳过此次检验。
        /// </summary>
        /// <param name="actionForCheck"></param>
        /// <returns></returns>
        public ReconnectionPlugin<TClient> SetActionForCheck(Func<TClient, int, Task<bool?>> actionForCheck)
        {
            this.ActionForCheck = actionForCheck;
            return this;
        }

        /// <summary>
        /// 设置连接动作
        /// </summary>
        /// <param name="tryConnect"></param>
        /// <returns>无论如何，只要返回True，则结束本轮尝试</returns>
        public ReconnectionPlugin<TClient> SetConnectAction(Func<TClient, Task<bool>> tryConnect)
        {
            this.ActionForConnect = tryConnect;
            return this;
        }

        /// <summary>
        /// 检验时间间隔
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        public ReconnectionPlugin<TClient> SetTick(TimeSpan tick)
        {
            this.Tick = tick;
            return this;
        }

        /// <summary>
        /// 使用轮询保持活性。
        /// </summary>
        public ReconnectionPlugin<TClient> UsePolling()
        {
            this.ActionForCheck = (client, failCount) =>
            {
                return Task.FromResult(client?.Online);
            };
            this.m_polling = true;
            return this;
        }
    }
}