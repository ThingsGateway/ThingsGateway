using Furion.DependencyInjection;
using Furion.FriendlyException;

using Microsoft.AspNetCore.Mvc.Filters;

namespace ThingsGateway.Web.Core
{
    /// <summary>
    /// 全局异常处理提供器，只会捕获未经trycath处理的程序异常
    /// </summary>
    public class LogExceptionHandler : IGlobalExceptionHandler, ISingleton
    {
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Filters.Any(it => it is LoggingMonitorAttribute))
            {
                return;
            }
            //OPENAPI异常已经被LoggingMonitor捕获，
            //其他异常一般都会在程序内直接处理，所以这里先备用，不存在实际代码
            await Task.CompletedTask;
        }
    }
}