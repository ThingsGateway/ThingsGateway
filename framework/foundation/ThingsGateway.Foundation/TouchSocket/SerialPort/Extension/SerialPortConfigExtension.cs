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

namespace ThingsGateway.Foundation.SerialPorts
{
    /// <summary>
    /// SerialPortConfigExtension
    /// </summary>
    public static class SerialPortConfigExtension
    {
        /// <summary>
        /// 设置串口适配器
        /// </summary>
        public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> SerialDataHandlingAdapterProperty =
            DependencyProperty<Func<SingleStreamDataHandlingAdapter>>.Register("SerialDataHandlingAdapter", () => new NormalDataHandlingAdapter());

        /// <summary>
        /// 串口属性。
        /// </summary>
        public static readonly DependencyProperty<SerialPortOption> SerialPortOptionProperty =
            DependencyProperty<SerialPortOption>.Register("SerialPortOption", new SerialPortOption());

        /// <summary>
        /// 设置(串口系)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialDataHandlingAdapter(this TouchSocketConfig config, Func<SingleStreamDataHandlingAdapter> value)
        {
            config.SetValue(SerialDataHandlingAdapterProperty, value);
            return config;
        }

        /// <summary>
        /// 设置串口属性。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialPortOption(this TouchSocketConfig config, SerialPortOption value)
        {
            config.SetValue(SerialPortOptionProperty, value);
            return config;
        }

        #region 连接

        /// <inheritdoc cref="IConnectObject.Connect(int, System.Threading.CancellationToken)"/>
        public static TClient Connect<TClient>(this TClient client, string portName, int timeout = 5000) where TClient : ISerialPortClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetSerialPortOption(new SerialPortOption()
                {
                    PortName = portName
                });
                client.Setup(config);
            }
            else
            {
                config = client.Config;
                if (config.TryGetValue(SerialPortOptionProperty, out var serialPortOption))
                {
                    serialPortOption.PortName = portName;
                }
                else
                {
                    config.SetSerialPortOption(new SerialPortOption()
                    {
                        PortName = portName
                    });
                }
            }
            client.Connect(timeout);
            return client;
        }

        /// <inheritdoc cref="IConnectObject.ConnectAsync(int, System.Threading.CancellationToken)"/>
        public static async Task<TClient> ConnectAsync<TClient>(this TClient client, string portName, int timeout = 5000) where TClient : ISerialPortClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetSerialPortOption(new SerialPortOption()
                {
                    PortName = portName
                });
                client.Setup(config);
            }
            else
            {
                config = client.Config;
                if (config.TryGetValue(SerialPortOptionProperty, out var serialPortOption))
                {
                    serialPortOption.PortName = portName;
                }
                else
                {
                    config.SetSerialPortOption(new SerialPortOption()
                    {
                        PortName = portName
                    });
                }
            }
            await client.ConnectAsync(timeout);
            return client;
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Result TryConnect<TClient>(this TClient client, int timeout = 5000) where TClient : ISerialPortClient
        {
            try
            {
                client.Connect(timeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<Result> TryConnectAsync<TClient>(this TClient client, int timeout = 5000) where TClient : ISerialPortClient
        {
            try
            {
                await client.ConnectAsync(timeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        #endregion 连接
    }
}