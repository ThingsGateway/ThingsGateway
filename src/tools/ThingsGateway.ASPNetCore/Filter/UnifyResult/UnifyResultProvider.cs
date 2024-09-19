//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Localization;

using ThingsGateway;
using ThingsGateway.NewLife.X;

namespace ThingsGateway.ASPNetCore;

/// <summary>
/// 规范化RESTful风格返回值
/// </summary>
public class UnifyResultProvider : IUnifyResultProvider
{
    private static IStringLocalizer Localizer = NetCoreApp.CreateLocalizerByType(typeof(UnifyResultProvider))!;

    /// <summary>
    /// 异常返回
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public IActionResult OnException(ActionExecutedContext context)
    {
        return new JsonResult(RESTfulResult(context.Result is IStatusCodeActionResult statusCodeResult ? statusCodeResult.StatusCode ?? 500 : 500, false, null, context.Exception?.GetTrue()?.Message));
    }

    /// <summary>
    /// 状态码响应拦截
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public async Task OnResponseStatusCodes(HttpContext context, int statusCode)
    {
        switch (statusCode)
        {
            // 处理 401 状态码
            case StatusCodes.Status401Unauthorized:
                await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, false, Localizer["TokenOver"])).ConfigureAwait(false);
                break;
            // 处理 403 状态码
            case StatusCodes.Status403Forbidden:
                await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, false, default, "NoPermission")).ConfigureAwait(false);
                break;

            default: break;
        }
    }

    /// <summary>
    /// 成功返回
    /// </summary>
    /// <param name="context"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public IActionResult OnSucceeded(ActionExecutedContext context, object? data)
    {
        return new JsonResult(RESTfulResult(StatusCodes.Status200OK, true, data));
    }

    /// <summary>
    /// 验证失败返回
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    public IActionResult OnValidateFailed(ActionExecutingContext context, string? errors)
    {
        return new JsonResult(RESTfulResult(StatusCodes.Status400BadRequest, false, null, errors));
    }

    /// <summary>
    /// 返回 RESTful 风格结果集
    /// </summary>
    /// <param name="statusCode">状态码</param>
    /// <param name="succeeded">是否成功</param>
    /// <param name="data">数据</param>
    /// <param name="errors">错误信息</param>
    /// <returns></returns>
    private static UnifyResult<object> RESTfulResult(int statusCode, bool succeeded = default, object? data = default, object? errors = default)
    {
        return new UnifyResult<object>
        {
            Code = statusCode,
            Msg = statusCode == StatusCodes.Status200OK ? "Success" : errors,
            Data = data,
            Time = DateTime.Now,
        };
    }
}
