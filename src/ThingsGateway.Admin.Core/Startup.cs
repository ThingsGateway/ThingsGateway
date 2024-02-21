//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using System.Reflection;

using ThingsGateway.Core.Extension;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// AppStartup启动类
/// </summary>
[AppStartup(98)]
public class Startup : AppStartup
{
    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        //检查ConfigId
        CheckSameConfigId();

        // 配置雪花Id算法机器码
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = 1// 取值范围0~63
        });

        services.AddComponent<LoggingConsoleComponent>();//启动控制台日志格式化组件

        //遍历配置
        DbContext.DbConfigs.ForEach(it =>
        {
            var connection = DbContext.Db.GetConnection(it.ConfigId);//获取数据库连接对象
            connection.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
        });

        TypeExtension.DefaultDisplayNameFuncs.Add(a => a.GetCustomAttribute<SugarColumn>()?.ColumnDescription);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }

    /// <summary>
    /// 检查是否有相同的ConfigId
    /// </summary>
    /// <returns></returns>
    private static void CheckSameConfigId()
    {
        var configIdGroup = DbContext.DbConfigs.GroupBy(it => it.ConfigId).ToList();
        foreach (var configId in configIdGroup)
        {
            if (configId.ToList().Count > 1) throw new($"Sqlsugar连接配置ConfigId:{configId.Key}重复!");
        }
    }
}