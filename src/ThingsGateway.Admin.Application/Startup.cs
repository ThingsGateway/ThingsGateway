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

using Furion.Schedule;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using System.Reflection;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// AppStartup启动类
/// </summary>
[AppStartup(97)]
public class Startup : AppStartup
{
    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName);//CodeFirst

        // 任务调度
        services.AddSchedule(options =>
        {
            options.AddJob(App.EffectiveTypes.ScanToBuilders());
        });

        //事件总线
        services.AddEventBus();

        //删除在线用户统计
        var verificatInfos = UserTokenCacheUtil.HashGetAll();
        //获取当前客户端ID所在的verificat信息
        foreach (var infos in verificatInfos.Values)
        {
            foreach (var item in infos)
            {
                item.ClientIds.Clear();
            }
        }
        UserTokenCacheUtil.HashSet(verificatInfos);//更新
    }

    /// <inheritdoc/>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}