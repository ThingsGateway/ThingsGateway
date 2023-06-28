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

using Furion.Reflection;
using Furion.Reflection.Extensions;

using UAParser;

namespace ThingsGateway.Application
{
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
            //如果不带返回值
            if (method.ReturnType == typeof(void))
            {
                return method.Invoke(Target, args);//直接返回
            }
            else
            {
                var result = method.Invoke(Target, args);//如果没有缓存就执行方法返回数据
                return result;//返回结果
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
                Exception exception = null;
                try
                {
                    var task = method.Invoke(Target, args) as Task;
                    await task;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                var str = "";
                ex(method, args, desc, str, exception);

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
                T result = default(T);
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
                var str = desc.IsRecordPar ? result.ToJson().FormatJson() : "";
                ex(method, args, desc, str, exception);
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

        private static void ex(MethodInfo method, object[] args, OperDescAttribute desc, string result, Exception exception)
        {
            //写入操作日志
            var str = App.HttpContext?.Request?.Headers?.UserAgent;
            ClientInfo clientInfo = null;
            if (str.HasValue)
            {
                clientInfo = UserAgent.Parser.Parse(str);
            }

            StringBuilder stringBuilder = new();
            if (desc.IsRecordPar)
            {
                var paras = method.GetParameters().Select(it => it.Name);
                stringBuilder.Append("{");
                foreach (var par in paras)
                {
                    stringBuilder.Append("\"" + par + "\":");
                    foreach (var item in args)
                    {
                        stringBuilder.Append(item.ToJson());
                    }
                }
                stringBuilder.Append("}");
            }
            //操作日志表实体
            var devLogOperate = new DevLogOperate
            {
                Name = desc.Description,
                Category = desc.Catcategory,
                ExeStatus = DevLogConst.SUCCESS,
                OpIp = App.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                OpBrowser = clientInfo?.UA?.Family + clientInfo?.UA?.Major,
                OpOs = clientInfo?.OS?.Family + clientInfo?.OS?.Major,
                OpTime = DateTime.UtcNow,
                OpAccount = UserManager.UserAccount,
                ReqUrl = "",
                ReqMethod = CateGoryConst.Log_REQMETHOD,
                ResultJson = result,
                ClassName = method.ReflectedType.Name,
                MethodName = method.Name,
                ParamJson = stringBuilder.ToString(),
                VerificatId = UserManager.VerificatId.ToLong(),
            };
            //如果异常不为空
            if (exception != null)
            {
                devLogOperate.ExeStatus = DevLogConst.FAIL;//操作状态为失败
                devLogOperate.ExeMessage = exception.Source + ":" + exception.Message + "\n" + exception.StackTrace;
            }
            var _db = DbContext.Db;
            _db.InsertableWithAttr(devLogOperate).IgnoreColumns(true).ExecuteCommand();//入库
        }
    }
}