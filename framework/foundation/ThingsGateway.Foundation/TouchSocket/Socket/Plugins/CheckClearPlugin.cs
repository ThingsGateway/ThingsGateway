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
    /// 检查清理连接插件。服务器与客户端均适用。
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = true)]
    public sealed class CheckClearPlugin<TClient> : PluginBase, ITcpConnectedPlugin<TClient> where TClient : ITcpClientBase
    {
        /// <summary>
        /// 检查清理连接插件。服务器与客户端均适用。
        /// </summary>
        public CheckClearPlugin()
        {
            this.OnClose = (client, type) =>
            {
                if (this.CheckClearType == CheckClearType.OnlyReceive)
                {
                    client.Close("超时无数据Receive交互，主动断开连接");
                }
                else if (this.CheckClearType == CheckClearType.OnlySend)
                {
                    client.Close("超时无数据Send交互，主动断开连接");
                }
                else
                {
                    client.Close("超时无数据交互，主动断开连接");
                }
            };
        }

        /// <summary>
        /// 清理统计类型。默认为：<see cref="CheckClearType.All"/>。当设置为<see cref="CheckClearType.OnlySend"/>时，
        /// 则只检验发送方向是否有数据流动。没有的话则会断开连接。
        /// </summary>
        public CheckClearType CheckClearType { get; set; } = CheckClearType.All;

        /// <summary>
        /// 当因为超出时间限定而关闭。
        /// </summary>
        public Action<TClient, CheckClearType> OnClose { get; set; }

        /// <summary>
        /// 获取或设置清理无数据交互的Client，默认60秒。
        /// </summary>
        public TimeSpan Tick { get; set; } = TimeSpan.FromSeconds(60);

        /// <inheritdoc/>
        public async Task OnTcpConnected(TClient client, ConnectedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(this.Tick.TotalMilliseconds / 10.0)).ConfigureFalseAwait();
                    if (!client.Online)
                    {
                        return;
                    }
                    if (this.CheckClearType == CheckClearType.OnlyReceive)
                    {
                        if (DateTime.Now - client.LastReceivedTime > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                    else if (this.CheckClearType == CheckClearType.OnlySend)
                    {
                        if (DateTime.Now - client.LastSendTime > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                    else
                    {
                        if (DateTime.Now - client.GetLastActiveTime() > this.Tick)
                        {
                            this.OnClose?.Invoke(client, this.CheckClearType);
                        }
                    }
                }
            });

            await e.InvokeNext();
        }

        /// <summary>
        /// 清理统计类型。默认为：<see cref="CheckClearType.All"/>。当设置为<see cref="CheckClearType.OnlySend"/>时，
        /// 则只检验发送方向是否有数据流动。没有的话则会断开连接。
        /// </summary>
        /// <param name="clearType"></param>
        /// <returns></returns>
        public CheckClearPlugin<TClient> SetCheckClearType(CheckClearType clearType)
        {
            this.CheckClearType = clearType;
            return this;
        }

        /// <summary>
        /// 当因为超出时间限定而关闭。
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public CheckClearPlugin<TClient> SetOnClose(Action<TClient, CheckClearType> action)
        {
            this.OnClose = action;
            return this;
        }

        /// <summary>
        /// 设置清理无数据交互的Client，默认60秒。
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public CheckClearPlugin<TClient> SetTick(TimeSpan timeSpan)
        {
            this.Tick = timeSpan;
            return this;
        }
    }
}