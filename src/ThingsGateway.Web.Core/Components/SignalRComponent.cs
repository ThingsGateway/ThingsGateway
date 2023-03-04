using Microsoft.AspNetCore.SignalR;

namespace ThingsGateway.Web.Core
{
    public sealed class SignalRComponent : IServiceComponent
    {
        public void Load(IServiceCollection services, ComponentContext componentContext)
        {
            services.AddSignalR();//注册SignalR
            services.AddSingleton<IUserIdProvider, UserIdProvider>();//用户ID提供器
        }
    }
}