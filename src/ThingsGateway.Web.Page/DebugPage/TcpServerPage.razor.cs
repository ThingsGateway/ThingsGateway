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

using System;

using ThingsGateway.Foundation;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Web.Page
{
    public partial class TcpServerPage
    {
        public Action<string> LogAction;

        private TouchSocketConfig config;

        private string ip = "127.0.0.1";

        private int port = 502;

        private TcpService tgTcpServer { get; set; } = new();

        public void Dispose()
        {
            tgTcpServer.SafeDispose();
        }
        public TcpService GetTGTcpServer()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetListenIPHosts(new IPHost[] { new IPHost(ip + ":" + port) });
            config.SetBufferLength(300);
            //载入配置
            tgTcpServer.Setup(config);
            return tgTcpServer;
        }

        protected override void OnInitialized()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetListenIPHosts(new IPHost[] { new IPHost(ip + ":" + port) });
            config.SetBufferLength(300);
            tgTcpServer = config.Container.Resolve<TcpService>();
            base.OnInitialized();
        }

        private void LogOut(string str)
        {
            LogAction?.Invoke(str);
        }
    }
}