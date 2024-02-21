//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.Reflection;
using Furion.Reflection.Extensions;

using Microsoft.Extensions.Hosting;

using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

using ThingsGateway.Core.Extension.ConcurrentQueue;
using ThingsGateway.Core.Extension.Json;

using UAParser;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// AOP处理操作日志
/// </summary>
public class OperDispatchProxy : AspectDispatchProxy, IDispatchProxy
{
    /// <summary>
    /// 服务提供器，可以用来解析服务，如：Services.GetService()
    /// </summary>
    public IServiceProvider Services { get; set; }

    /// <summary>
    /// 当前服务实例
    /// </summary>
    public object Target { get; set; }

    /// <summary>
    /// 方法
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public override object Invoke(MethodInfo method, object[] args)
    {
        var desc = method.GetActualCustomAttribute<OperDescAttribute>(Target);
        if (desc == null)
        {
            return Invoke(method, args);
        }
        else
        {
            Exception exception = default;
            object result = default;
            try
            {
                result = Invoke(method, args);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            WriteOperLog(method, args, desc, result, exception);

            if (exception != null)
            {
                throw exception;
            }
            return result;//返回结果
        }

        object Invoke(MethodInfo method, object[] args)
        {
            //如果不带返回值
            if (method.ReturnType == typeof(void))
            {
                return method.Invoke(Target, args);//直接返回
            }
            else
            {
                var result = method.Invoke(Target, args);
                return result;//返回结果
            }
        }
    }

    /// <summary>
    /// 异步无返回值
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override async Task InvokeAsync(MethodInfo method, object[] args)
    {
        var desc = method.GetActualCustomAttribute<OperDescAttribute>(Target, true);
        if (desc == null)
        {
            var task = method.Invoke(Target, args) as Task;
            await task;
        }
        else
        {
            Exception exception = default;
            try
            {
                var task = method.Invoke(Target, args) as Task;
                await task;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            WriteOperLog(method, args, desc, null, exception);

            if (exception != null)
            {
                throw exception;
            }
        }
    }

    /// <summary>
    /// 异步带返回值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override async Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
    {
        var desc = method.GetActualCustomAttribute<OperDescAttribute>(Target, true);
        if (desc == null)
        {
            var taskT = method.Invoke(Target, args) as Task<T>;
            var result = await taskT;
            return result;//返回结果
        }
        else
        {
            T result = default;
            //写入操作日志
            Exception exception = null;
            try
            {
                var taskT = method.Invoke(Target, args) as Task<T>;
                result = await taskT;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            WriteOperLog(method, args, desc, result, exception);
            if (exception != null)
            {
                throw exception;
            }
            else
            {
                return result;//返回结果
            }
        }
    }

    private void WriteOperLog(MethodInfo method, object[] args, OperDescAttribute desc, object result, Exception exception)
    {
        //写入操作日志
        var str = App.HttpContext?.Request?.Headers?.UserAgent;
        ClientInfo clientInfo = null;
        if (str.HasValue)
        {
            clientInfo = Parser.GetDefault().Parse(str);
        }

        StringBuilder stringBuilder = new();
        if (desc.IsRecordPar)
        {
            var parameters = method.GetParameters();
            var jsonParameters = parameters.Select((p, i) => $"\"{p.Name}\": {args[i].ToJsonString()}");
            stringBuilder.Append('{');
            stringBuilder.Append(string.Join(", ", jsonParameters));
            stringBuilder.Append('}');
        }
        var paramJson = stringBuilder.ToString();
        var resultJson = desc.IsRecordPar ? result?.ToJsonString() : null;
        //操作日志表实体
        var log = new SysOperateLog
        {
            Name = desc.Description,
            Category = desc.Catcategory,
            ExeStatus = LogConst.SUCCESS,
            OpIp = App.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString(),
            OpBrowser = clientInfo?.UA?.Family + clientInfo?.UA?.Major,
            OpOs = clientInfo?.OS?.Family + clientInfo?.OS?.Major,
            OpTime = DateTime.Now,
            OpAccount = UserManager.UserAccount,
            ReqUrl = "",
            ReqMethod = LogConst.LOG_REQMETHOD,
            ResultJson = resultJson,
            ClassName = method.ReflectedType.Name,
            MethodName = method.Name,
            ParamJson = paramJson,
            VerificatId = UserManager.VerificatId,
        };
        //如果异常不为空
        if (exception != null)
        {
            log.Category = CateGoryConst.Log_EXCEPTION;//操作类型为异常
            log.ExeStatus = LogConst.FAIL;//操作状态为失败
            log.ExeMessage = exception.Source + ":" + exception.Message + Environment.NewLine + exception.StackTrace;
        }
        WriteToQueue(log);
    }

    /// <summary>
    /// 日志消息队列（线程安全）
    /// </summary>
    private static readonly ConcurrentQueue<SysOperateLog> _logMessageQueue = new();

    static OperDispatchProxy()
    {
        // 创建长时间运行的后台任务，并将日志消息队列中数据写入存储中
        Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 将日志消息写入队列中等待后台任务出队写入数据库
    /// </summary>
    /// <param name="logMsg">结构化日志消息</param>
    private void WriteToQueue(SysOperateLog logMsg)
    {
        _logMessageQueue.Enqueue(logMsg);
    }

    /// <summary>
    /// 将日志消息写入数据库中
    /// </summary>
    private static async Task ProcessQueue()
    {
        var db = DbContext.Db.CopyNew();
        var appLifetime = App.GetService<IHostApplicationLifetime>();
        while (!(appLifetime.ApplicationStopping.IsCancellationRequested || appLifetime.ApplicationStopped.IsCancellationRequested))
        {
            if (_logMessageQueue.Count > 0)
            {
                await db.InsertableWithAttr(_logMessageQueue.ToListWithDequeue()).ExecuteCommandAsync();//入库
            }
            await Task.Delay(3000);
        }
    }
}