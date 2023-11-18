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

using Furion;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// AppStartup启动类
/// </summary>
[AppStartup(9998)]
public class Startup : AppStartup
{
    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        DbContext.InitDbContext();
        // 配置雪花Id算法机器码
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = 4// 取值范围0~63
        });

        services.AddComponent<LoggingConsoleComponent>();//启动控制台日志格式化组件
        ThingsGateway.Core.TypeExtensions.DefaultFuncs.Add(a => a.GetCustomAttribute<SqlSugar.SugarColumn>()?.ColumnDescription);
    }
}