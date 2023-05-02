using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 插件基类
    /// </summary>
    public abstract class DriverBase : IDisposable
    {
        /// <summary>
        /// <see cref="TouchSocketConfig"/> 
        /// </summary>
        public TouchSocketConfig TouchSocketConfig;
        /// <summary>
        /// 底层日志
        /// </summary>
        protected LoggerGroup logMessage;
        /// <inheritdoc cref="DriverBase"/>
        public DriverBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            TouchSocketConfig = new TouchSocketConfig();
            logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new EasyLogger(Log_Out));
            TouchSocketConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
        }
        /// <summary>
        /// 日志
        /// </summary>
        protected ILogger _logger;
        /// <summary>
        /// 服务工厂
        /// </summary>
        protected IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// 插件配置项 ，继承实现<see cref="DriverPropertyBase"/>后，返回继承类，如果不存在，返回null
        /// </summary>
        public abstract DriverPropertyBase DriverPropertys { get; }

        /// <summary>
        /// 是否输出日志
        /// </summary>
        public bool IsLogOut { get; set; }

        /// <inheritdoc/>
        public abstract void Dispose();

        /// <summary>
        /// 报文信息
        /// </summary>
        public ConcurrentList<(long id, string message)> Messages { get; set; } = new();
        /// <summary>
        /// 是否连接成功
        /// </summary>
        /// <returns></returns>
        public abstract OperResult IsConnected();

        /// <summary>
        /// 底层日志输出
        /// </summary>
        protected void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
        {
            if (IsLogOut)
                PluginExtension.Log_Out(_logger, arg1, arg2, arg3, arg4);
        }
        /// <summary>
        /// 设备报文
        /// </summary>
        public void NewMessage(LogType arg1, object arg2, string arg3, Exception arg4)
        {
            if (arg3.StartsWith("报文-") && arg1 == LogType.Trace)
            {
                Messages.Add(new(YitIdHelper.NextId(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff zz") + "-" + arg3));

                if (Messages.Count > 2500)
                {
                    Messages.RemoveRange(0, 500);
                }
            }
        }
    }
}