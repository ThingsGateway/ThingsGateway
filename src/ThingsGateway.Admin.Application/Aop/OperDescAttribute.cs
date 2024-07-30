//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Rougamo;
using Rougamo.Context;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Json.Extension;

using UAParser;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// Aop拦截器
/// </summary>
public class OperDescAttribute : MoAttribute
{
    /// <summary>
    /// 日志消息队列（线程安全）
    /// </summary>
    private static readonly ConcurrentQueue<SysOperateLog> _logMessageQueue = new();
    private static readonly IAppService AppService;

    static OperDescAttribute()
    {
        // 创建长时间运行的后台任务，并将日志消息队列中数据写入存储中
        Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        AppService = NetCoreApp.RootServices.GetService<IAppService>();
    }

    public OperDescAttribute(string description, bool isRecordPar = true, object localizerType = null)
    {
        Description = description;
        IsRecordPar = isRecordPar;
        LocalizerType = (Type)localizerType;
    }

    /// <summary>
    /// 说明，需配置本地化json文件
    /// </summary>
    public string Description { get; }

    public override Feature Features => Feature.OnException | Feature.OnSuccess;
    public override AccessFlags Flags => AccessFlags.Public | AccessFlags.Method;

    /// <summary>
    /// 是否记录进出参数
    /// </summary>
    public bool IsRecordPar { get; }

    public Type? LocalizerType { get; }

    public override void OnException(MethodContext context)
    {
        //插入异常日志
        SysOperateLog log = GetOperLog(LocalizerType, context);

        log.Category = LogCateGoryEnum.Exception;//操作类型为异常
        log.ExeStatus = false;//操作状态为失败
        if (context.Exception is UserFriendlyException exception)
            log.ExeMessage = exception?.Message;
        else
            log.ExeMessage = context.Exception?.ToString();

        WriteToQueue(log);
    }

    public override void OnSuccess(MethodContext context)
    {
        //插入操作日志
        SysOperateLog log = GetOperLog(LocalizerType, context);
        WriteToQueue(log);
    }

    /// <summary>
    /// 将日志消息写入数据库中
    /// </summary>
    private static async Task ProcessQueue()
    {
        var db = DbContext.Db.GetConnectionScopeWithAttr<SysOperateLog>().CopyNew();
        var appLifetime = NetCoreApp.RootServices!.GetService<IHostApplicationLifetime>()!;
        while (!((appLifetime?.ApplicationStopping ?? default).IsCancellationRequested || (appLifetime?.ApplicationStopped ?? default).IsCancellationRequested))
        {
            try
            {
                var data = _logMessageQueue.ToListWithDequeue(); // 从日志队列中获取数据
                if (data.Count > 0)
                {
                    await db.InsertableWithAttr(data).ExecuteCommandAsync();//入库
                }
                await Task.Delay(3000, appLifetime?.ApplicationStopping ?? default).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    private SysOperateLog GetOperLog(Type? localizerType, MethodContext context)
    {
        var methodBase = context.Method;
        ClientInfo? clientInfo = AppService.ClientInfo;
        string? paramJson = null;
        if (IsRecordPar)
        {
            var args = context.Arguments;
            var parametersInfo = methodBase.GetParameters();
            var parametersDict = new Dictionary<string, object>();

            for (int i = 0; i < parametersInfo.Length; i++)
            {
                parametersDict[parametersInfo[i].Name!] = args[i];
            }
            paramJson = parametersDict.ToSystemTextJsonString();
        }
        var result = context.ReturnValue;
        var resultJson = IsRecordPar ? result?.ToSystemTextJsonString() : null;
        //操作日志表实体
        var log = new SysOperateLog
        {
            Name = (localizerType == null ? NetCoreApp.CreateLocalizerByType(typeof(OperDescAttribute)) : NetCoreApp.CreateLocalizerByType(localizerType))![Description],
            Category = LogCateGoryEnum.Operate,
            ExeStatus = true,
            OpIp = AppService?.RemoteIpAddress?.MapToIPv4()?.ToString(),
            OpBrowser = clientInfo?.UA?.Family + clientInfo?.UA?.Major,
            OpOs = clientInfo?.OS?.Family + clientInfo?.OS?.Major,
            OpTime = DateTime.Now,
            OpAccount = UserManager.UserAccount,
            ReqUrl = null,
            ReqMethod = "browser",
            ResultJson = resultJson,
            ClassName = methodBase.ReflectedType!.Name,
            MethodName = methodBase.Name,
            ParamJson = paramJson,
            VerificatId = UserManager.VerificatId,
        };
        return log;
    }

    /// <summary>
    /// 将日志消息写入队列中等待后台任务出队写入数据库
    /// </summary>
    /// <param name="logMsg">结构化日志消息</param>
    private void WriteToQueue(SysOperateLog logMsg)
    {
        _logMessageQueue.Enqueue(logMsg);
    }
}
