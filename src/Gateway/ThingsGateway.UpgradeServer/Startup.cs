//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using ThingsGateway.Admin.Application;
using ThingsGateway.Upgrade;

namespace ThingsGateway.UpgradeServer;

[AppStartup(-1)]
public class Startup : AppStartup
{
    public void ConfigureAdminApp(IServiceCollection services)
    {
        services.AddSingleton<IUpdateZipFileService, UpdateZipFileService>();
        services.AddHostedService<FileHostService>();
        services.AddConfigurableOptions<UpgradeServerOptions>();
    }
    public void UseAdminCore(IServiceProvider serviceProvider)
    {
        //检查ConfigId
        var configIdGroup = DbContext.DbConfigs.GroupBy(it => it.ConfigId);
        foreach (var configId in configIdGroup)
        {
            if (configId.Count() > 1) throw new($"Sqlsugar connect configId: {configId.Key} Duplicate!");
        }

        //遍历配置
        DbContext.DbConfigs?.ForEach(it =>
        {
            var connection = DbContext.Db.GetConnection(it.ConfigId);//获取数据库连接对象
            if (it.InitTable == true)
                connection.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
        });
        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName!);//CodeFirst
    }

}
