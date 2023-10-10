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
using System.Reflection;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// Tcp命令行插件。
    /// </summary>
    public abstract class TcpCommandLinePlugin : PluginBase, ITcpReceivedPlugin
    {
        private readonly ILog m_logger;
        private readonly Dictionary<string, Method> m_pairs = new Dictionary<string, Method>();

        /// <summary>
        /// Tcp命令行插件。
        /// </summary>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected TcpCommandLinePlugin(ILog logger)
        {
            this.m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Converter = new StringConverter();
            var ms = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(a => a.Name.EndsWith("Command"));
            foreach (var item in ms)
            {
                this.m_pairs.Add(item.Name.Replace("Command", string.Empty), new Method(item));
            }
        }

        /// <summary>
        /// 字符串转换器，默认支持基础类型和Json。可以自定义。
        /// </summary>
        public StringConverter Converter { get; }

        /// <summary>
        /// 是否返回执行异常。
        /// </summary>
        public bool ReturnException { get; set; } = true;

        /// <summary>
        /// 当有执行异常时，不返回异常。
        /// </summary>
        /// <returns></returns>
        public TcpCommandLinePlugin NoReturnException()
        {
            this.ReturnException = false;
            return this;
        }

        /// <inheritdoc/>
        public async Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            try
            {
                var strs = e.ByteBlock.ToString().Split(' ');
                if (strs.Length > 0 && this.m_pairs.TryGetValue(strs[0], out var method))
                {
                    var ps = method.Info.GetParameters();
                    var os = new object[ps.Length];
                    var index = 0;
                    for (var i = 0; i < ps.Length; i++)
                    {
                        if (ps[i].ParameterType.IsInterface && typeof(ITcpClientBase).IsAssignableFrom(ps[i].ParameterType))
                        {
                            os[i] = client;
                        }
                        else
                        {
                            os[i] = this.Converter.ConvertFrom(strs[index + 1], ps[i].ParameterType);
                            index++;
                        }
                    }
                    e.Handled = true;

                    try
                    {
                        object result;
                        switch (method.TaskType)
                        {
                            case TaskReturnType.Task:
                                await method.InvokeAsync(this, os);
                                result = default;
                                break;
                            case TaskReturnType.TaskObject:
                                result = await method.InvokeObjectAsync(this, os);
                                break;
                            case TaskReturnType.None:
                            default:
                                result = method.Invoke(this, os);
                                break;
                        }
                        if (method.HasReturn)
                        {
                            client.Send(this.Converter.ConvertTo(result));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (this.ReturnException)
                        {
                            client.Send(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.m_logger.Log(LogLevel.Error, this, ex.Message, ex);
            }

            await e.InvokeNext();
        }
    }
}