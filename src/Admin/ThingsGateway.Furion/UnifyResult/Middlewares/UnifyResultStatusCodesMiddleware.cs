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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ThingsGateway.UnifyResult;

/// <summary>
/// 状态码中间件
/// </summary>
[SuppressSniffer]
public class UnifyResultStatusCodesMiddleware
{
    /// <summary>
    /// 请求委托
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// 授权头
    /// </summary>
    private readonly string[] _authorizedHeaders;

    /// <summary>
    /// 是否携带授权头判断
    /// </summary>
    private readonly bool _withAuthorizationHeaderCheck;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next"></param>
    /// <param name="authorizedHeaders"></param>
    /// <param name="withAuthorizationHeaderCheck"></param>
    public UnifyResultStatusCodesMiddleware(RequestDelegate next
        , string[] authorizedHeaders
        , bool withAuthorizationHeaderCheck)
    {
        _next = next;
        _authorizedHeaders = authorizedHeaders;
        _withAuthorizationHeaderCheck = withAuthorizationHeaderCheck;
    }

    /// <summary>
    /// 中间件执行方法
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context).ConfigureAwait(false);

        // 只有请求错误（短路状态码）和非 WebSocket 才支持规范化处理
        if (context.IsWebSocketRequest()
            || context.Response.StatusCode < 400
            || context.Response.StatusCode == 404) return;

        // 仅针对特定的头进行处理
        if (_withAuthorizationHeaderCheck
            && context.Response.StatusCode == StatusCodes.Status401Unauthorized
            && !context.Response.Headers.Any(h => _authorizedHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase)))
        {
            return;
        }

        // 处理规范化结果
        if (!UnifyContext.CheckExceptionHttpContextNonUnify(context, out var unifyResult))
        {
            // 解决刷新 Token 时间和 Token 时间相近问题
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized
                && context.Response.Headers.ContainsKey("access-token")
                && context.Response.Headers.ContainsKey("x-access-token"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }

            // 如果 Response 已经完成输出，则禁止写入
            if (context.Response.HasStarted) return;

            var statusCode = context.Response.StatusCode;

            // 获取授权失败设置的状态码
            var authorizationFailStatusCode = context.Items[AuthorizationHandlerContextExtensions.FAIL_STATUSCODE_KEY];
            if (authorizationFailStatusCode != null)
            {
                statusCode = Convert.ToInt32(authorizationFailStatusCode);
            }

            await unifyResult.OnResponseStatusCodes(context, statusCode, context.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value).ConfigureAwait(false);
        }
    }
}