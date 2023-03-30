using Microsoft.Extensions.Hosting;

using System.Linq;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 获取后台服务扩展类
    /// </summary>
    public static class ServiceExtension
    {
        ///// <summary>
        ///// 获取后台服务
        ///// </summary>
        //public static T GetBackgroundService<T>(this IServiceScopeFactory @this) where T : class, IHostedService
        //{
        //    var hostedService = @this.CreateScope().ServiceProvider.GetServices<IHostedService>().FirstOrDefault(it => it is T) as T;
        //    return hostedService;
        //}
        /// <summary>
        /// 获取后台服务
        /// </summary>
        public static T GetBackgroundService<T>(this object @this) where T : class, IHostedService
        {
            var hostedService = App.RootServices.GetServices<IHostedService>().FirstOrDefault(it => it is T) as T;
            return hostedService;
        }
    }
}
