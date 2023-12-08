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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// LoggerContainerExtension
    /// </summary>
    public static class LoggerContainerExtension
    {
        #region GroupLogger

        /// <summary>
        /// 添加控制台日志到日志组。
        /// </summary>
        /// <returns></returns>
        public static void AddConsoleLogger(this LoggerGroup loggerGroup)
        {
            loggerGroup.AddLogger(ConsoleLogger.Default);
        }

        /// <summary>
        /// 添加委托日志到日志组。
        /// </summary>
        /// <param name="loggerGroup"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void AddEasyLogger(this LoggerGroup loggerGroup, Action<LogLevel, object, string, Exception> action)
        {
            loggerGroup.AddLogger(new EasyLogger(action));
        }

        /// <summary>
        /// 添加委托日志到日志组。
        /// </summary>
        /// <param name="loggerGroup"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void AddEasyLogger(this LoggerGroup loggerGroup, Action<string> action)
        {
            loggerGroup.AddLogger(new EasyLogger(action));
        }

        /// <summary>
        /// 添加文件日志到日志组。
        /// </summary>
        /// <param name="loggerGroup"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static void AddFileLogger(this LoggerGroup loggerGroup, string rootPath = "logs")
        {
            loggerGroup.AddLogger(new FileLogger(rootPath));
        }

        #endregion GroupLogger

        #region Obsolete

        /// <summary>
        /// 添加控制台日志到日志组。
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IRegistrator AddConsoleLogger(this IRegistrator container)
        {
            AddLogger(container, ConsoleLogger.Default);
            return container;
        }

        /// <summary>
        /// 添加委托日志到日志组。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegistrator AddEasyLogger(this IRegistrator container, Action<LogLevel, object, string, Exception> action)
        {
            AddLogger(container, new EasyLogger(action));
            return container;
        }

        /// <summary>
        /// 添加委托日志到日志组。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegistrator AddEasyLogger(this IRegistrator container, Action<string> action)
        {
            AddLogger(container, new EasyLogger(action));
            return container;
        }

        /// <summary>
        /// 添加文件日志到日志组。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static IRegistrator AddFileLogger(this IRegistrator container, string rootPath = "logs")
        {
            AddLogger(container, new FileLogger(rootPath));
            return container;
        }

        #endregion

        /// <summary>
        /// 添加日志到容器。
        /// </summary>
        /// <param name="registrator"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IRegistrator AddLogger(this IRegistrator registrator, ILog logger)
        {
            registrator.RegisterSingleton<ILog>(logger);
            return registrator;
        }

        /// <summary>
        /// 添加日志组
        /// </summary>
        /// <param name="registrator"></param>
        /// <param name="loggerAction"></param>
        /// <returns></returns>
        public static IRegistrator AddLogger(this IRegistrator registrator, Action<LoggerGroup> loggerAction)
        {
            var loggerGroup = new LoggerGroup();
            loggerAction.Invoke(loggerGroup);
            registrator.RegisterSingleton<ILog>(loggerGroup);
            return registrator;
        }

        /// <summary>
        /// 设置单例日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="registrator"></param>
        /// <returns></returns>
        public static IRegistrator SetSingletonLogger<TLogger>(this IRegistrator registrator) where TLogger : class, ILog
        {
            registrator.RegisterSingleton<ILog, TLogger>();
            return registrator;
        }

        /// <summary>
        /// 设置单例实例日志。
        /// </summary>
        /// <typeparam name="TLogger"></typeparam>
        /// <param name="registrator"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IRegistrator SetSingletonLogger<TLogger>(this IRegistrator registrator, TLogger logger) where TLogger : class, ILog
        {
            registrator.RegisterSingleton<ILog, TLogger>(logger);
            return registrator;
        }
    }
}