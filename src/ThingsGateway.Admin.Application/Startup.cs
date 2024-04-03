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
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

using System.Reflection;
using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core;
using ThingsGateway.Core.Extension;
using ThingsGateway.Logging;

using UAParser;

using Yitter.IdGenerator;

namespace Microsoft.Extensions.DependencyInjection;

[AppStartup(2)]
public class Startup : AppStartup
{
    public void ConfigureAdminApp(IServiceCollection services)
    {
        services.AddSingleton<ICacheService, MemoryCacheService>();

        //检查ConfigId
        CheckSameConfigId();

        //遍历配置
        DbContext.DbConfigs?.ForEach(it =>
        {
            var connection = DbContext.Db.GetConnection(it.ConfigId);//获取数据库连接对象
            connection.DbMaintenance.CreateDatabase();//创建数据库,如果存在则不创建
        });

        // 配置雪花Id算法机器码
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = 1// 取值范围0~63
        });

        var fullName = Assembly.GetExecutingAssembly().FullName;//获取程序集全名
        CodeFirstUtils.CodeFirst(fullName!);//CodeFirst

        #region 控制台美化

        services.AddConsoleFormatter(options =>
        {
            options.MessageFormat = (logMsg) =>
            {
                //如果不是LoggingMonitor日志才格式化
                if (logMsg.LogName != "System.Logging.LoggingMonitor")
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("【日志级别】：" + logMsg.LogLevel);
                    stringBuilder.AppendLine("【日志类名】：" + logMsg.LogName);
                    stringBuilder.AppendLine("【日志时间】：" + DateTime.Now.ToDefaultDateTimeFormat());
                    stringBuilder.AppendLine("【日志内容】：" + logMsg.Message);
                    if (logMsg.Exception != null)
                    {
                        stringBuilder.AppendLine("【异常信息】：" + logMsg.Exception);
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    return logMsg.Message;
                }
            };
            options.WriteHandler = (logMsg, scopeProvider, writer, fmtMsg, opt) =>
            {
                ConsoleColor consoleColor = ConsoleColor.White;
                switch (logMsg.LogLevel)
                {
                    case LogLevel.Information:
                        consoleColor = ConsoleColor.DarkGreen;
                        break;

                    case LogLevel.Warning:
                        consoleColor = ConsoleColor.DarkYellow;
                        break;

                    case LogLevel.Error:
                        consoleColor = ConsoleColor.DarkRed;
                        break;
                }
                writer.WriteWithColor(fmtMsg, ConsoleColor.Black, consoleColor);
            };
        });

        #endregion 控制台美化

        var parser = Parser.GetDefault();

        #region api日志

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
                var client = parser.Parse(userAgent);
                // 获取控制器/操作描述器
                var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                //操作名称默认是控制器名加方法名,自定义操作名称要在action上加Description特性
                var option = $"{controllerActionDescriptor.ControllerName}/{controllerActionDescriptor.ActionName}";

                var desc = App.CreateLocalizerByType(controllerActionDescriptor.ControllerTypeInfo.AsType())[controllerActionDescriptor.MethodInfo.Name];
                //获取特性
                option = desc.Value;//则将操作名称赋值为控制器上写的title

                logContext.Set(LoggingConst.CateGory, option);//传操作名称
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

        #endregion api日志

        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IImportExportService, ImportExportService>();
        services.AddSingleton<IVerificatInfoCacheService, VerificatInfoCacheService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ISysDictService, SysDictService>();
        services.AddSingleton<ISysOperateLogService, SysOperateLogService>();
        services.AddSingleton<IRelationService, RelationService>();
        services.AddSingleton<ISysResourceService, SysResourceService>();
        services.AddSingleton<ISysRoleService, SysRoleService>();
        services.AddSingleton<ISignalrNoticeService, SignalrNoticeService>();
        services.AddSingleton<ISysUserService, SysUserService>();
        services.AddSingleton<IUserCenterService, UserCenterService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IUnifyResultProvider, UnifyResultProvider>();

        services.AddHostedService<AdminTaskService>();
    }

    public void UseAdminCore(IApplicationBuilder app)
    {
        //删除在线用户统计
        var verificatInfoCacheService = app.ApplicationServices.GetService<IVerificatInfoCacheService>();
        var verificatInfos = verificatInfoCacheService.HashGetAll();
        //获取当前客户端ID所在的verificat信息
        foreach (var infos in verificatInfos.Values)
        {
            foreach (var item in infos)
            {
                item.ClientIds.Clear();
            }
        }
        verificatInfoCacheService.HashSet(verificatInfos);//更新
    }

    /// <summary>
    /// 检查是否有相同的ConfigId
    /// </summary>
    /// <returns></returns>
    private void CheckSameConfigId()
    {
        var configIdGroup = DbContext.DbConfigs.GroupBy(it => it.ConfigId);
        foreach (var configId in configIdGroup)
        {
            if (configId.Count() > 1) throw new($"Sqlsugar connect configId: {configId.Key} Duplicate!");
        }
    }
}