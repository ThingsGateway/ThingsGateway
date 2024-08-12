//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using System.Security.Claims;

namespace ThingsGateway;

/// <summary>
/// App静态类
/// </summary>
public class App : NetCoreApp
{
    /// <summary>
    /// 获取请求上下文
    /// </summary>
    public static HttpContext? HttpContext => RootServices?.GetService<IHttpContextAccessor>()?.HttpContext;


    /// <summary>
    /// 获取请求上下文用户
    /// </summary>
    public static ClaimsPrincipal? User => HttpContext?.User;

    /// <summary>
    /// 获取当前请求 TraceId
    /// </summary>
    /// <returns></returns>
    public static string GetTraceId()
    {
        return Activity.Current?.Id ?? (RootServices == null ? default : HttpContext?.TraceIdentifier);
    }


}


