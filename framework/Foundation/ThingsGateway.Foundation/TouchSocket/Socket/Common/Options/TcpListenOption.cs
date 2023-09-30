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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 监听配置
    /// </summary>
    public class TcpListenOption
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 监听地址
        /// </summary>
        public IPHost IpHost { get; set; }

        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// 接收类型
        /// </summary>
        public ReceiveType ReceiveType { get; set; } = ReceiveType.Iocp;

        /// <summary>
        /// 是否使用地址复用
        /// </summary>
        public bool ReuseAddress { get; set; }

        /// <summary>
        /// Tcp处理并发连接时最大半连接队列
        /// </summary>
        public int Backlog { get; set; } = 100;

        /// <summary>
        /// 禁用延迟发送
        /// </summary>
        public bool? NoDelay { get; set; }

        /// <summary>
        /// 是否使用ssl加密
        /// </summary>
        public bool UseSsl => this.ServiceSslOption != null;

        /// <summary>
        /// 用于Ssl加密的证书
        /// </summary>
        public ServiceSslOption ServiceSslOption { get; set; }

        /// <summary>
        /// 配置Tcp适配器
        /// </summary>
        public Func<SingleStreamDataHandlingAdapter> Adapter { get; set; } =
            () => new NormalDataHandlingAdapter();
    }
}
