//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Extension;

using SqlSugar;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Logging;

using UAParser;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 数据库写入器
/// </summary>
public class DatabaseLoggingWriter : IDatabaseLoggingWriter
{
    /// <summary>
    /// 此方法只会写入经由MVCFilter捕捉的方法日志，对于BlazorServer的内部操作，由<see cref="OperDescAttribute"/>执行
    /// </summary>
    /// <param name="logMsg"></param>
    /// <param name="flush"></param>
    public async Task WriteAsync(LogMessage logMsg, bool flush)
    {
        //获取请求json字符串
        var jsonString = logMsg.Context.Get("loggingMonitor").ToString();
        //转成实体
        var loggingMonitor = jsonString.FromSystemTextJsonString<LoggingMonitorJson>();
        //日志时间赋值
        loggingMonitor.LogDateTime = logMsg.LogDateTime;
        // loggingMonitor.ReturnInformation.Value
        //验证失败不记录日志
        bool save = false;
        if (loggingMonitor.Validation == null)
        {
            var operation = logMsg.Context.Get(LoggingConst.Operation).ToString();//获取操作名称
            var client = (ClientInfo)logMsg.Context.Get(LoggingConst.Client);//获取客户端信息
            var path = logMsg.Context.Get(LoggingConst.Path).ToString();//获取操作名称
            var method = logMsg.Context.Get(LoggingConst.Method).ToString();//获取方法
            //表示访问日志
            if (path == "/api/auth/login" || path == "/api/auth/logout")
            {
                //如果没有异常信息
                if (loggingMonitor.Exception == null)
                {
                    save = await CreateVisitLog(operation, path, loggingMonitor, client, flush).ConfigureAwait(false);//添加到访问日志
                }
                else
                {
                    //添加到异常日志
                    save = await CreateOperationLog(operation, path, loggingMonitor, client, flush).ConfigureAwait(false);
                }
            }
            else
            {
                //只有定义了Title的POST方法才记录日志
                if (!operation.IsNullOrWhiteSpace() && method == "POST")
                {
                    //添加到操作日志
                    save = await CreateOperationLog(operation, path, loggingMonitor, client, flush).ConfigureAwait(false);
                }
            }
        }
        if (save)
        {
            await Task.Delay(1000).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 创建操作日志
    /// </summary>
    /// <param name="operation">操作名称</param>
    /// <param name="path">请求地址</param>
    /// <param name="loggingMonitor">loggingMonitor</param>
    /// <param name="clientInfo">客户端信息</param>
    /// <param name="flush"></param>
    /// <returns></returns>
    private async Task<bool> CreateOperationLog(string operation, string path, LoggingMonitorJson loggingMonitor, ClientInfo clientInfo, bool flush)
    {
        //账号
        var opAccount = loggingMonitor.AuthorizationClaims?.Where(it => it.Type == ClaimConst.Account).Select(it => it.Value).FirstOrDefault();

        //获取参数json字符串，
        var paramJson = loggingMonitor.Parameters == null || loggingMonitor.Parameters.Count == 0 ? null : loggingMonitor.Parameters[0].Value.ToSystemTextJsonString();

        //获取结果json字符串
        var resultJson = string.Empty;
        if (loggingMonitor.ReturnInformation != null)//如果有返回值
        {
            if (loggingMonitor.ReturnInformation.Value != null)//如果返回值不为空
            {
                resultJson = loggingMonitor.ReturnInformation.Value.ToSystemTextJsonString();
            }
        }

        //操作日志表实体
        var sysLogOperate = new SysOperateLog
        {
            Name = operation,
            Category = LogCateGoryEnum.Operate,
            ExeStatus = true,
            OpIp = loggingMonitor.RemoteIPv4,
            OpBrowser = clientInfo.UA.Family + clientInfo.UA.Major,
            OpOs = clientInfo.OS.Family + clientInfo.OS.Major,
            OpTime = loggingMonitor.LogDateTime.LocalDateTime,
            OpAccount = opAccount,
            ReqMethod = loggingMonitor.HttpMethod,
            ReqUrl = path,
            ResultJson = resultJson,
            ClassName = loggingMonitor.DisplayName,
            MethodName = loggingMonitor.ActionName,
            ParamJson = paramJson,
            VerificatId = UserManager.VerificatId,
        };
        //如果异常不为空
        if (loggingMonitor.Exception != null)
        {
            sysLogOperate.Category = LogCateGoryEnum.Exception;//操作类型为异常
            sysLogOperate.ExeStatus = false;//操作状态为失败

            if (loggingMonitor.Exception.Type == typeof(UserFriendlyException).ToString())
                sysLogOperate.ExeMessage = loggingMonitor?.Exception.Message;
            else
                sysLogOperate.ExeMessage = $"{loggingMonitor.Exception.Type}:{loggingMonitor.Exception.Message}{Environment.NewLine}{loggingMonitor.Exception.StackTrace}";
        }

        _operateLogMessageQueue.Enqueue(sysLogOperate);

        if (flush)
        {
            SqlSugarClient ??= DbContext.Db.GetConnectionScopeWithAttr<SysOperateLog>().CopyNew();
            await SqlSugarClient.InsertableWithAttr(_operateLogMessageQueue.ToListWithDequeue()).ExecuteCommandAsync();//入库
            return true;
        }
        return false;
    }

    private SqlSugarClient SqlSugarClient;

    /// <summary>
    /// 日志消息队列（线程安全）
    /// </summary>
    private readonly ConcurrentQueue<SysOperateLog> _operateLogMessageQueue = new();

    /// <summary>
    /// 创建访问日志
    /// </summary>
    /// <param name="operation">访问类型</param>
    /// <param name="path"></param>
    /// <param name="loggingMonitor">loggingMonitor</param>
    /// <param name="clientInfo">客户端信息</param>
    /// <param name="flush"></param>
    private async Task<bool> CreateVisitLog(string operation, string path, LoggingMonitorJson loggingMonitor, ClientInfo clientInfo, bool flush)
    {
        long verificatId = 0;//验证Id
        var opAccount = "";//用户账号
        if (path == "/api/auth/login")
        {
            //如果是登录，用户信息就从返回值里拿
            var result = loggingMonitor.ReturnInformation?.Value?.ToSystemTextJsonString();//返回值转json
            var userInfo = result.FromSystemTextJsonString<UnifyResult<LoginOutput>>();//格式化成user表
            opAccount = userInfo.Data.Account;//赋值账号
            verificatId = userInfo.Data.VerificatId;
        }
        else
        {
            //如果是登录出，用户信息就从AuthorizationClaims里拿
            opAccount = loggingMonitor.AuthorizationClaims.Where(it => it.Type == ClaimConst.Account).Select(it => it.Value).FirstOrDefault();
            verificatId = loggingMonitor.AuthorizationClaims.Where(it => it.Type == ClaimConst.VerificatId).Select(it => it.Value).FirstOrDefault().ToLong();
        }
        //日志表实体
        var sysLogVisit = new SysOperateLog
        {
            Name = operation,
            Category = path == "/api/auth/login" ? LogCateGoryEnum.Login : LogCateGoryEnum.Logout,
            ExeStatus = true,
            OpIp = loggingMonitor.RemoteIPv4,
            OpBrowser = clientInfo.UA.Family + clientInfo.UA.Major,
            OpOs = clientInfo.OS.Family + clientInfo.OS.Major,
            OpTime = loggingMonitor.LogDateTime.LocalDateTime,
            VerificatId = verificatId,
            OpAccount = opAccount,

            ReqMethod = loggingMonitor.HttpMethod,
            ReqUrl = path,
            ResultJson = loggingMonitor.ReturnInformation?.Value?.ToSystemTextJsonString(),
            ClassName = loggingMonitor.DisplayName,
            MethodName = loggingMonitor.ActionName,
            ParamJson = loggingMonitor.Parameters?.ToSystemTextJsonString(),
        };
        _operateLogMessageQueue.Enqueue(sysLogVisit);

        if (flush)
        {
            SqlSugarClient ??= DbContext.Db.GetConnectionScopeWithAttr<SysOperateLog>().CopyNew();
            await SqlSugarClient.InsertableWithAttr(_operateLogMessageQueue.ToListWithDequeue()).ExecuteCommandAsync();//入库
            return true;
        }
        return false;
    }
}
