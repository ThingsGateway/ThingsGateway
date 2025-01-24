// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// 授权处理上下文拓展类
/// </summary>
[SuppressSniffer]
public static class AuthorizationHandlerContextExtensions
{
    internal const string FAIL_STATUSCODE_KEY = $"{nameof(AuthorizationHandlerContext)}_FAIL_STATUSCODE";

    /// <summary>
    /// 获取当前 HttpContext 上下文
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static DefaultHttpContext GetCurrentHttpContext(this AuthorizationHandlerContext context)
    {
        DefaultHttpContext httpContext;

        // 获取 httpContext 对象
        if (context.Resource is AuthorizationFilterContext filterContext) httpContext = (DefaultHttpContext)filterContext.HttpContext;
        else if (context.Resource is DefaultHttpContext defaultHttpContext) httpContext = defaultHttpContext;
        else httpContext = null;

        return httpContext;
    }

    /// <summary>
    /// 设置授权状态码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    public static void StatusCode(this AuthorizationHandlerContext context, int statusCode)
    {
        var httpContext = context.GetCurrentHttpContext();
        if (httpContext != null)
        {
            httpContext.Items[FAIL_STATUSCODE_KEY] = statusCode;
        }
    }

    /// <summary>
    /// 标记授权失败并设置状态码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    public static void Fail(this AuthorizationHandlerContext context, int statusCode)
    {
        context.Fail();
        context.StatusCode(statusCode);
    }
}