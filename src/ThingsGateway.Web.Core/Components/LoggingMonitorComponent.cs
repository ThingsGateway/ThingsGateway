using Microsoft.AspNetCore.Mvc.Controllers;

namespace ThingsGateway.Web.Core
{
    /// <summary>
    /// LoggingMonitor操作日志写入数据库插件
    /// </summary>
    public class LoggingMonitorComponent : IServiceComponent
    {
        public void Load(IServiceCollection services, ComponentContext componentContext)
        {
            //Monitor日志配置
            services.AddMonitorLogging(options =>
             {
                 options.JsonIndented = true;// 是否美化 JSON
                 options.GlobalEnabled = false;//全局启用
                 options.ConfigureLogger((logger, logContext, context) =>
                  {
                      var httpContext = context.HttpContext;//获取httpContext
                                                            //获取头
                      var userAgent = httpContext.Request.Headers["User-Agent"];
                      if (string.IsNullOrEmpty(userAgent)) userAgent = "Other";//如果没有这个头就指定一个
                                                                               //获取客户端信息
                      var client = UserAgent.Parser.Parse(userAgent);
                      // 获取控制器/操作描述器
                      var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                      //操作名称默认是控制器名加方法名,自定义操作名称要在action上加Description特性
                      var option = $"{controllerActionDescriptor.ControllerName}/{controllerActionDescriptor.ActionName}";
                      //获取特性
                      var monitor = controllerActionDescriptor.MethodInfo.GetCustomAttribute<DescriptionAttribute>();
                      if (monitor != null)//如果有LoggingMonitor特性
                          option = monitor.Description;//则将操作名称赋值为控制器上写的title
                      logContext.Set(LoggingConst.Operation, option);//传操作名称
                      logContext.Set(LoggingConst.Client, client);//客户端信息
                      logContext.Set(LoggingConst.Path, httpContext.Request.Path.Value);//请求地址
                      logContext.Set(LoggingConst.Method, httpContext.Request.Method);//请求方法
                  });
             });
            //日志写入数据库配置
            services.AddDatabaseLogging<DatabaseLoggingWriter>(options =>
            {
                options.WriteFilter = (logMsg) =>
                {
                    return logMsg.LogName == "System.Logging.LoggingMonitor";//只写入LoggingMonitor日志
                };
            });
        }
    }
}