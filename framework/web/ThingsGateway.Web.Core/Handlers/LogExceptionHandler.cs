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
        /// <inheritdoc/>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            await Task.CompletedTask;
            if (context.Filters.Any(it => it is LoggingMonitorAttribute))
            {
                return;
            }
            //OPENAPI异常已经被LoggingMonitor捕获，
            //其他异常一般都会在程序内直接处理，所以这里先备用，不存在实际代码
            return;
        }
    }
}