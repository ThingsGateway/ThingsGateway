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
using Furion.Reflection;
using Furion.Reflection.Extensions;

using System.Reflection;
using System.Text;

using ThingsGateway.Admin.Core;
using ThingsGateway.Admin.Core.JsonExtensions;

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
    /// <exception cref="NotImplementedException"></exception>
    public override object Invoke(MethodInfo method, object[] args)
    {
        var desc = Target.GetCustomAttribute<OperDescAttribute>(method.ToString(), true);
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
        var desc = method.GetActualCustomAttribute<OperDescAttribute>(Target);
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
        var desc = method.GetActualCustomAttribute<OperDescAttribute>(Target);
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


    private static void WriteOperLog(MethodInfo method, object[] args, OperDescAttribute desc, object result, Exception exception)
    {

        //写入操作日志
        var str = App.HttpContext?.Request?.Headers?.UserAgent;
        ClientInfo clientInfo = null;
        if (str.HasValue)
        {
            clientInfo = StaticParser.Parser.Parse(str);
        }

        StringBuilder stringBuilder = new();
        if (desc.IsRecordPar)
        {
            var parameters = method.GetParameters();
            var jsonParameters = parameters.Select((p, i) => $"\"{p.Name}\": {args[i].ToJsonString()}");
            stringBuilder.Append("{");
            stringBuilder.Append(string.Join(", ", jsonParameters));
            stringBuilder.Append("}");
        }
        var paramJson = stringBuilder.ToString();
        var resultJson = desc.IsRecordPar ? result?.ToJsonString() : null;
        //操作日志表实体
        var log = new SysOperateLog
        {
            Name = desc.Description,
            Category = desc.Catcategory,
            ExeStatus = LogConst.LOG_SUCCESS,
            OpIp = App.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString(),
            OpBrowser = clientInfo?.UA?.Family + clientInfo?.UA?.Major,
            OpOs = clientInfo?.OS?.Family + clientInfo?.OS?.Major,
            OpTime = SysDateTimeExtensions.CurrentDateTime,
            OpAccount = UserManager.UserAccount,
            ReqUrl = "",
            ReqMethod = LogConst.LOG_REQMETHOD,
            ResultJson = resultJson,
            ClassName = method.ReflectedType.Name,
            MethodName = method.Name,
            ParamJson = paramJson,
            VerificatId = UserManager.VerificatId.ToLong(),
        };
        //如果异常不为空
        if (exception != null)
        {
            log.ExeStatus = LogConst.LOG_FAIL;//操作状态为失败
            log.ExeMessage = exception.Source + ":" + exception.Message + Environment.NewLine + exception.StackTrace;
        }


        DbContext.Db.InsertableWithAttr(log).ExecuteCommand();//入库
    }
}