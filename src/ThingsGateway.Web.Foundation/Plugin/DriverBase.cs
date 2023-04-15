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
        /// <inheritdoc cref="DriverBase"/>
        public DriverBase(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
        /// 是否连接成功
        /// </summary>
        /// <returns></returns>
        public abstract OperResult IsConnected();
        /// <summary>
        /// <see cref="TouchSocket"/> 日志输出
        /// </summary>
        protected void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
        {
            if (IsLogOut)
                PluginExtension.Log_Out(_logger, arg1, arg2, arg3, arg4);
        }
    }
}