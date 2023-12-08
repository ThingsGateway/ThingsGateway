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
using Furion.DataValidation;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using Furion.UnifyResult;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 规范化RESTful风格返回值
/// </summary>
[SuppressSniffer, UnifyModel(typeof(UnifyResult<>))]
public class UnifyResultProvider : IUnifyResultProvider
{
    /// <summary>
    /// 异常返回
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public IActionResult OnException(ExceptionContext context, ExceptionMetadata metadata)
    {
        return new JsonResult(RESTfulResult(metadata.StatusCode, data: metadata.Data, errors: metadata.Errors));
    }

    /// <summary>
    /// 状态码响应拦截
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    /// <param name="unifyResultSettings"></param>
    /// <returns></returns>
    public async Task OnResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings = null)
    {
        // 设置响应状态码
        UnifyContext.SetResponseStatusCodes(context, statusCode, unifyResultSettings);

        switch (statusCode)
        {
            // 处理 401 状态码
            case StatusCodes.Status401Unauthorized:
                await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "登录已过期，请重新登录"),
                    App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
                break;
            // 处理 403 状态码
            case StatusCodes.Status403Forbidden:
                await context.Response.WriteAsJsonAsync(RESTfulResult(statusCode, errors: "禁止访问，没有权限"),
                    App.GetOptions<JsonOptions>()?.JsonSerializerOptions);
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
    public IActionResult OnSucceeded(ActionExecutedContext context, object data)
    {
        return new JsonResult(RESTfulResult(StatusCodes.Status200OK, true, data));
    }

    /// <summary>
    /// 验证失败返回
    /// </summary>
    /// <param name="context"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public IActionResult OnValidateFailed(ActionExecutingContext context, ValidationMetadata metadata)
    {
        return new JsonResult(RESTfulResult(metadata.StatusCode ?? StatusCodes.Status400BadRequest, data: metadata.Data, errors: metadata.FirstErrorMessage ?? metadata.Message));
    }

    /// <summary>
    /// 返回 RESTful 风格结果集
    /// </summary>
    /// <param name="statusCode">状态码</param>
    /// <param name="succeeded">是否成功</param>
    /// <param name="data">数据</param>
    /// <param name="errors">错误信息</param>
    /// <returns></returns>
    private static UnifyResult<object> RESTfulResult(int statusCode, bool succeeded = default, object data = default, object errors = default)
    {
        return new UnifyResult<object>
        {
            Code = statusCode,
            Msg = statusCode == StatusCodes.Status200OK ? "请求成功" : errors,
            Data = data,
            Extras = UnifyContext.Take(),
            Time = DateTimeExtensions.CurrentDateTime,
        };
    }
}