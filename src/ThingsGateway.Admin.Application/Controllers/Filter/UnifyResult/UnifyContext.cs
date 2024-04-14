
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Reflection;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 规范化结果上下文
/// </summary>
public static class UnifyContext
{
    /// <summary>
    /// 是否启用规范化结果
    /// </summary>
    internal static bool EnabledUnifyHandler = true;

    /// <summary>
    /// 检查请求成功是否进行规范化处理
    /// </summary>
    /// <param name="method"></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    internal static bool CheckSucceededNonUnify(MethodInfo method)
    {
        // 判断是否跳过规范化处理
        var isSkip = !EnabledUnifyHandler
              || method.CustomAttributes.Any(x => typeof(NonUnifyAttribute).IsAssignableFrom(x.AttributeType) || typeof(ProducesResponseTypeAttribute).IsAssignableFrom(x.AttributeType) || typeof(IApiResponseMetadataProvider).IsAssignableFrom(x.AttributeType))
              || method.ReflectedType!.IsDefined(typeof(NonUnifyAttribute), true)
              || method.DeclaringType!.Assembly.GetName().Name!.StartsWith("Microsoft.AspNetCore.OData");

        return isSkip;
    }

    /// <summary>
    /// 检查短路状态码（>=400）是否进行规范化处理
    /// </summary>
    /// <param name="context"></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    internal static bool CheckStatusCodeNonUnify(HttpContext context)
    {
        // 获取终点路由特性
        var endpointFeature = context.Features.Get<IEndpointFeature>();
        if (endpointFeature == null) return false;

        // 判断是否跳过规范化处理
        var isSkip = !EnabledUnifyHandler
              || context.Request.Headers["accept"].ToString().Contains("odata.metadata=", StringComparison.OrdinalIgnoreCase)
                || context.Request.Headers["accept"].ToString().Contains("odata.streaming=", StringComparison.OrdinalIgnoreCase)
                || ResponseContentTypesOfNonUnify.Any(u => context.Response.Headers["content-type"].ToString().Contains(u, StringComparison.OrdinalIgnoreCase)
                || context.GetMetadata<NonUnifyAttribute>() != null
                || endpointFeature?.Endpoint?.Metadata?.GetMetadata<NonUnifyAttribute>() != null
              );

        return isSkip;
    }

    /// <summary>
    /// 跳过规范化处理的 Response Content-Type
    /// </summary>
    internal static string[] ResponseContentTypesOfNonUnify = new[]
    {
        "text/event-stream",
        "application/pdf",
        "application/octet-stream",
        "image/"
    };

    /// <summary>
    /// 检查 HttpContext 是否进行规范化处理
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    internal static bool CheckHttpContextNonUnify(HttpContext httpContext)
    {
        var contentType = httpContext.Response.Headers["content-type"].ToString();
        if (ResponseContentTypesOfNonUnify.Any(u => contentType.Contains(u, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查是否是有效的结果（可进行规范化的结果）
    /// </summary>
    /// <param name="result"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    internal static bool CheckVaildResult(IActionResult result, out object? data)
    {
        data = default;

        // 排除以下结果，跳过规范化处理
        var isDataResult = result switch
        {
            ViewResult => false,
            PartialViewResult => false,
            FileResult => false,
            ChallengeResult => false,
            SignInResult => false,
            SignOutResult => false,
            RedirectToPageResult => false,
            RedirectToRouteResult => false,
            RedirectResult => false,
            RedirectToActionResult => false,
            LocalRedirectResult => false,
            ForbidResult => false,
            ViewComponentResult => false,
            PageResult => false,
            NotFoundResult => false,
            NotFoundObjectResult => false,
            _ => true,
        };

        // 目前支持返回值 ActionResult
        if (isDataResult) data = result switch
        {
            // 处理内容结果
            ContentResult content => content.Content,
            // 处理对象结果
            ObjectResult obj => obj.Value,
            // 处理 JSON 对象
            JsonResult json => json.Value,
            _ => null,
        };

        return isDataResult;
    }
}