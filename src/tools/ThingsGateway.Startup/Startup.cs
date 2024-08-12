//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;

using Yitter.IdGenerator;

namespace ThingsGateway;

[AppStartup(10000000)]
public class Startup : AppStartup
{
    public void ConfigureApp(IServiceCollection services)
    {
        services.AddSingleton<ICacheService, MemoryCacheService>();
        // 配置雪花Id算法机器码
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = 1// 取值范围0~63
        });

        services.AddSingleton(typeof(IDataService<>), typeof(BaseService<>));
        services.AddSingleton<ISugarAopService, SugarAopService>();

    }

    public void UseService(IServiceProvider serviceProvider)
    {
    }
}
