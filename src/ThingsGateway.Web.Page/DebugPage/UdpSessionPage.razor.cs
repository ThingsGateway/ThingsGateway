#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
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
    public partial class UdpSessionPage
    {
        public Action<string> LogAction;

        private TouchSocketConfig config;

        private string ip = "127.0.0.1";

        private int port = 502;

        private TGUdpSession tgUdpSession { get; set; } = new();

        public void Dispose()
        {
            tgUdpSession.SafeDispose();
        }
        public TGUdpSession GetTGUdpSession()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetRemoteIPHost(new IPHost(ip + ":" + port)).SetBufferLength(300);
            config.SetBindIPHost(new IPHost(0));
            //载入配置
            tgUdpSession.Setup(config);
            return tgUdpSession;
        }

        protected override void OnInitialized()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetRemoteIPHost(new IPHost(ip + ":" + port)).SetBufferLength(300);
            config.SetBindIPHost(new IPHost(0));
            tgUdpSession = config.Container.Resolve<TGUdpSession>();
            base.OnInitialized();
        }

        private void LogOut(string str)
        {
            LogAction?.Invoke(str);
        }
    }
}