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

using Furion.Logging;


using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Core;

using UAParser;

namespace ThingsGateway.Web.Core;

/// <summary>
/// 数据库写入器
/// </summary>
public class DatabaseLoggingWriter : IDatabaseLoggingWriter
{
    /// <summary>
    /// 此方法只会写入经由MVCFilter捕捉的方法日志，对于BlazorServer的内部操作，由<see cref="OperDispatchProxy"/>执行
    /// </summary>
    /// <param name="logMsg"></param>
    /// <param name="flush"></param>
    public void Write(LogMessage logMsg, bool flush)
    {
        //获取请求json字符串
        var jsonString = logMsg.Context.Get("loggingMonitor").ToString();
        //转成实体
        var loggingMonitor = jsonString.ToJsonWithT<LoggingMonitorJson>();
        //日志时间赋值
        loggingMonitor.LogDateTime = logMsg.LogDateTime;
        {
            var operation = logMsg.Context.Get(LoggingConst.Operation).ToString();//获取操作名称
            var client = (ClientInfo)logMsg.Context.Get(LoggingConst.Client);//获取客户端信息
            var path = logMsg.Context.Get(LoggingConst.Path).ToString();//获取操作名称
            var method = logMsg.Context.Get(LoggingConst.Method).ToString();//获取方法
                                                                            //表示访问日志
            if (operation == EventSubscriberConst.Login || operation == EventSubscriberConst.Logout ||
                operation == EventSubscriberConst.LogoutOpenApi || operation == EventSubscriberConst.LoginOpenApi
                )
            {
                //如果没有异常信息
                if (loggingMonitor.Exception == null && loggingMonitor.Validation == null)
                {
                    CreateVisitLog(operation, loggingMonitor, client);//添加到访问日志
                }
                else
                {
                    //添加到异常日志
                    CreateOperationLog(operation, path, loggingMonitor, client);
                }
            }
            else
            {
                //只有定义了Title的POST方法才记录日志
                if (!operation.Contains("/") && method == "POST")
                {
                    //添加到操作日志
                    CreateOperationLog(operation, path, loggingMonitor, client);
                }
            }
        }
    }

    /// <summary>
    /// 创建操作日志
    /// </summary>
    /// <param name="operation">操作名称</param>
    /// <param name="path">请求地址</param>
    /// <param name="loggingMonitor">loggingMonitor</param>
    /// <param name="clientInfo">客户端信息</param>
    /// <returns></returns>
    private void CreateOperationLog(string operation, string path, LoggingMonitorJson loggingMonitor, ClientInfo clientInfo)
    {
        //用户名称
        var name = loggingMonitor.AuthorizationClaims?.Where(it => it.Type == ClaimConst.Name).Select(it => it.Value).FirstOrDefault();
        //账号
        var opAccount = loggingMonitor.AuthorizationClaims?.Where(it => it.Type == ClaimConst.Account).Select(it => it.Value).FirstOrDefault();

        //获取参数json字符串，
        var paramJson = loggingMonitor.Parameters == null || loggingMonitor.Parameters?.Count == 0 ? null : loggingMonitor.Parameters[0].Value.ToJsonString();
        //获取结果json字符串
        var resultJson = string.Empty;
        if (loggingMonitor.ReturnInformation != null)
            resultJson = loggingMonitor.ReturnInformation.Value == null ? null : loggingMonitor.ReturnInformation.Value.ToJsonString();

        //操作日志表实体
        var sysLogOperate = new SysOperateLog
        {
            Name = operation,
            Category = LogConst.LOG_OPENAPIOPERATE,
            ExeStatus = LogConst.LOG_SUCCESS,
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
            VerificatId = UserManager.VerificatId.ToLong(),
        };
        //如果异常不为空
        if (loggingMonitor.Exception != null || loggingMonitor.Validation != null)
        {
            sysLogOperate.ExeStatus = LogConst.LOG_FAIL;//操作状态为失败
            sysLogOperate.ExeMessage = loggingMonitor.Exception != null ?
                loggingMonitor.Exception.Type + ":" + loggingMonitor.Exception.Message + Environment.NewLine +
                loggingMonitor.Exception.StackTrace : loggingMonitor.Validation.Message;
        }
        DbContext.Db.InsertableWithAttr(sysLogOperate).IgnoreColumns(true).ExecuteCommand();//入库
    }

    /// <summary>
    /// 创建访问日志
    /// </summary>
    /// <param name="operation">访问类型</param>
    /// <param name="loggingMonitor">loggingMonitor</param>
    /// <param name="clientInfo">客户端信息</param>
    private void CreateVisitLog(string operation, LoggingMonitorJson loggingMonitor, ClientInfo clientInfo)
    {
        var opAccount = "";//用户账号
        long verificatId = 0;//验证Id
        if (operation == EventSubscriberConst.Login || operation == EventSubscriberConst.LoginOpenApi)
        {
            //如果是登录，用户信息就从返回值里拿
            var result = loggingMonitor.ReturnInformation.Value.ToJsonString();//返回值转json
            var userInfo = result.ToJsonWithT<UnifyResult<BaseLoginOutput>>();//格式化成user表
            opAccount = userInfo.Data.Account;//赋值账号

            verificatId = userInfo.Data.VerificatId;
        }
        else
        {
            //如果是登录出，用户信息就从AuthorizationClaims里拿
            opAccount = loggingMonitor.AuthorizationClaims.Where(it => it.Type == ClaimConst.Account).Select(it => it.Value).FirstOrDefault();
            verificatId = loggingMonitor.AuthorizationClaims.Where(it => it.Type == ClaimConst.VerificatId).Select(it => it.Value).FirstOrDefault().ToLong();
        }
        string category = "";
        switch (operation)
        {
            case EventSubscriberConst.Login:
                category = LogConst.LOG_LOGIN;
                break;

            case EventSubscriberConst.Logout:
                category = LogConst.LOG_LOGOUT;
                break;

            case EventSubscriberConst.LoginOpenApi:
                category = LogConst.LOG_OPENAPILOGIN;
                break;

            case EventSubscriberConst.LogoutOpenApi:
                category = LogConst.LOG_OPENAPILOGOUT;
                break;

            default:
                break;
        }
        //日志表实体
        var sysLogVisit = new SysVisitLog
        {
            Name = operation,
            Category = category,
            ExeStatus = LogConst.LOG_SUCCESS,
            OpIp = loggingMonitor.RemoteIPv4,
            OpBrowser = clientInfo.UA.Family + clientInfo.UA.Major,
            OpOs = clientInfo.OS.Family + clientInfo.OS.Major,
            OpTime = loggingMonitor.LogDateTime.LocalDateTime,
            OpAccount = opAccount,
            VerificatId = verificatId,
        };
        DbContext.Db.InsertableWithAttr(sysLogVisit).IgnoreColumns(true).ExecuteCommand();//入库
    }
}