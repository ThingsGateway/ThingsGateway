//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 认证信息
/// </summary>
public class AuthorizationClaims
{
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; }
}

/// <summary>
/// 异常信息
/// </summary>
public class LogException
{
    /// <summary>
    /// 异常内容
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 堆栈信息
    /// </summary>
    public string StackTrace { get; set; }

    /// <summary>
    /// 异常类型
    /// </summary>
    public string Type { get; set; }
}

/// <summary>
/// 请求信息格式化
/// </summary>
public class LoggingMonitorJson
{
    /// <summary>
    /// 方法名称
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// 认证信息
    /// </summary>
    public List<AuthorizationClaims> AuthorizationClaims { get; set; }

    /// <summary>
    /// 控制器名
    /// </summary>
    public string ControllerName { get; set; }

    /// <summary>
    /// 类名称
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 环境
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public LogException Exception { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// 服务端
    /// </summary>
    public string LocalIPv4 { get; set; }

    /// <summary>
    /// 日志时间
    /// </summary>
    public DateTimeOffset LogDateTime { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    public string OsArchitecture { get; set; }

    /// <summary>
    /// 系统名称
    /// </summary>
    public string OsDescription { get; set; }

    /// <summary>
    /// 参数列表
    /// </summary>
    public List<Parameters> Parameters { get; set; }

    /// <summary>
    /// 客户端IPV4地址
    /// </summary>
    public string RemoteIPv4 { get; set; }

    /// <summary>
    /// 认证信息
    /// </summary>
    public string RequestHeaderAuthorization { get; set; }

    /// <summary>
    /// 认证信息
    /// </summary>
    public string RequestHeaderCookies { get; set; }

    /// <summary>
    /// 请求地址
    /// </summary>
    public string RequestUrl { get; set; }

    /// <summary>
    /// 返回信息
    /// </summary>
    public ReturnInformation ReturnInformation { get; set; }

    /// <summary>
    /// 浏览器标识
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// 验证错误信息
    /// </summary>
    public Validation Validation { get; set; }
}

/// <summary>
/// 请求参数
/// </summary>
public class Parameters
{
    /// <summary>
    /// 参数名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public object Value { get; set; }
}

/// <summary>
/// 返回信息
/// </summary>
public class ReturnInformation
{
    /// <summary>
    /// 返回值
    /// </summary>
    public object Value { get; set; }
}

/// <summary>
/// 验证失败信息
/// </summary>
public class Validation
{
    /// <summary>
    /// 错误详情
    /// </summary>
    public string Message { get; set; }
}
