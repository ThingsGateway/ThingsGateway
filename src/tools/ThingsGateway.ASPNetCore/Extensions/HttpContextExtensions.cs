//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.AspNetCore.Mvc.Controllers;

using System.Text;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Http 拓展类
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 获取 控制器/Action 描述器
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static ControllerActionDescriptor GetControllerActionDescriptor(this HttpContext httpContext)
    {
        return httpContext.GetEndpoint()?.Metadata?.FirstOrDefault(u => u is ControllerActionDescriptor) as ControllerActionDescriptor;
    }

    /// <summary>
    /// 获取本机 IPv4地址
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetLocalIpAddressToIPv4(this HttpContext context)
    {
        return context.Connection.LocalIpAddress?.MapToIPv4()?.ToString();
    }

    /// <summary>
    /// 获取本机 IPv6地址
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetLocalIpAddressToIPv6(this HttpContext context)
    {
        return context.Connection.LocalIpAddress?.MapToIPv6()?.ToString();
    }

    /// <summary>
    /// 获取 Action 特性
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static TAttribute GetMetadata<TAttribute>(this HttpContext httpContext)
        where TAttribute : class
    {
        return httpContext.GetEndpoint()?.Metadata?.GetMetadata<TAttribute>();
    }

    /// <summary>
    /// 获取来源地址
    /// </summary>
    /// <param name="request"></param>
    /// <param name="refererHeaderKey"></param>
    /// <returns></returns>
    public static string GetRefererUrlAddress(this HttpRequest request, string refererHeaderKey = "Referer")
    {
        return request.Headers[refererHeaderKey].ToString();
    }

    /// <summary>
    /// 获取远程 IPv4地址
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRemoteIpAddressToIPv4(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();
    }

    /// <summary>
    /// 获取远程 IPv6地址
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRemoteIpAddressToIPv6(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.MapToIPv6()?.ToString();
    }

    /// <summary>
    /// 获取完整请求地址
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetRequestUrlAddress(this HttpRequest request)
    {
        return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
    }

    /// <summary>
    /// 判断是否是 WebSocket 请求
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool IsWebSocketRequest(this HttpContext context)
    {
        return context.WebSockets.IsWebSocketRequest || context.Request.Path == "/ws";
    }

    /// <summary>
    /// 读取 Body 内容
    /// </summary>
    /// <param name="httpContext"></param>
    /// <remarks>需先在 Startup 的 Configure 中注册 app.EnableBuffering()</remarks>
    /// <returns></returns>
    public static async Task<string> ReadBodyContentAsync(this HttpContext httpContext)
    {
        if (httpContext == null) return default;
        return await httpContext.Request.ReadBodyContentAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 读取 Body 内容
    /// </summary>
    /// <param name="request"></param>
    /// <remarks>需先在 Startup 的 Configure 中注册 app.EnableBuffering()</remarks>
    /// <returns></returns>
    public static async Task<string> ReadBodyContentAsync(this HttpRequest request)
    {
        request.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);

        request.Body.Seek(0, SeekOrigin.Begin);
        return body;
    }

    /// <summary>
    /// 设置响应头 Tokens
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="accessToken"></param>
    /// <param name="refreshToken"></param>
    public static void SetTokensOfResponseHeaders(this HttpContext httpContext, string accessToken, string? refreshToken = null)
    {
        httpContext.Response.Headers["access-token"] = accessToken;
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            httpContext.Response.Headers["x-access-token"] = refreshToken;
        }
    }

    /// <summary>
    /// 设置规范化文档自动登录
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="accessToken"></param>
    public static void SigninToSwagger(this HttpContext httpContext, string accessToken)
    {
        // 设置 Swagger 刷新自动授权
        httpContext.Response.Headers["access-token"] = accessToken;
    }

    /// <summary>
    /// 设置规范化文档退出登录
    /// </summary>
    /// <param name="httpContext"></param>
    public static void SignoutToSwagger(this HttpContext httpContext)
    {
        httpContext.Response.Headers["access-token"] = "invalid_token";
    }
}
