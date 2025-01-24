﻿// ------------------------------------------------------------------------
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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;
using System.Reflection;

using ThingsGateway.Extensions;
using ThingsGateway.FriendlyException;

namespace ThingsGateway.UnifyResult;

/// <summary>
/// 规范化结果上下文
/// </summary>
[SuppressSniffer]
public static class UnifyContext
{
    /// <summary>
    /// 是否启用规范化结果
    /// </summary>
    internal static bool EnabledUnifyHandler = false;

    /// <summary>
    /// 是否启用状态码拦截中间件
    /// </summary>
    internal static bool EnabledStatusCodesMiddleware = false;

    /// <summary>
    /// 规范化结果额外数据键
    /// </summary>
    internal static string UnifyResultExtrasKey = "UNIFY_RESULT_EXTRAS";

    /// <summary>
    /// 规范化结果提供器
    /// </summary>
    internal static ConcurrentDictionary<string, UnifyMetadata> UnifyProviders = new();

    /// <summary>
    /// 规范化序列化配置
    /// </summary>
    internal static ConcurrentDictionary<string, object> UnifySerializerSettings = new();

    /// <summary>
    /// 获取异常元数据
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static ExceptionMetadata GetExceptionMetadata(ActionContext context)
    {
        object errorCode = default;
        object originErrorCode = default;
        object errors = default;
        object data = default;
        var statusCode = StatusCodes.Status500InternalServerError;
        var isValidationException = false; // 判断是否是验证异常

        // 判断是否是 ExceptionContext 或者 ActionExecutedContext
        var exception = context is ExceptionContext exContext
            ? exContext.Exception
            : (
                context is ActionExecutedContext edContext
                ? edContext.Exception
                : default
            );

        // 判断是否是友好异常
        if (exception is AppFriendlyException friendlyException)
        {
            errorCode = friendlyException.ErrorCode;
            originErrorCode = friendlyException.OriginErrorCode;
            statusCode = friendlyException.StatusCode;
            isValidationException = friendlyException.ValidationException;
            errors = friendlyException.ErrorMessage;
            data = friendlyException.Data;
        }

        return new ExceptionMetadata
        {
            StatusCode = statusCode,
            ErrorCode = errorCode,
            OriginErrorCode = originErrorCode,
            Errors = errors,
            Data = data,
            Exception = exception
        };
    }

    /// <summary>
    /// 填充附加信息
    /// </summary>
    /// <param name="extras"></param>
    public static void Fill(object extras)
    {
        var items = App.HttpContext?.Items;
        if (items == null)
        {
            return;
        }

        if (items.ContainsKey(UnifyResultExtrasKey)) items.Remove(UnifyResultExtrasKey);
        items.Add(UnifyResultExtrasKey, extras);
    }

    /// <summary>
    /// 读取附加信息
    /// </summary>
    public static object Take()
    {
        object extras = null;
        App.HttpContext?.Items?.TryGetValue(UnifyResultExtrasKey, out extras);
        return extras;
    }

    /// <summary>
    /// 设置响应状态码
    /// </summary>
    /// <param name="context"></param>
    /// <param name="statusCode"></param>
    /// <param name="unifyResultSettings"></param>
    public static void SetResponseStatusCodes(HttpContext context, int statusCode, UnifyResultSettingsOptions unifyResultSettings)
    {
        if (unifyResultSettings == null) return;

        // 篡改响应状态码
        if (unifyResultSettings.AdaptStatusCodes != null && unifyResultSettings.AdaptStatusCodes.Length > 0)
        {
            var adaptStatusCode = unifyResultSettings.AdaptStatusCodes.FirstOrDefault(u => u[0] == statusCode);
            if (adaptStatusCode != null && adaptStatusCode.Length > 0 && adaptStatusCode[0] > 0)
            {
                context.Response.StatusCode = adaptStatusCode[1];
                return;
            }
        }

        // 如果为 null，则所有请求错误的状态码设置为 200
        if (unifyResultSettings.Return200StatusCodes == null) context.Response.StatusCode = 200;
        // 否则只有里面的才设置为 200
        else if (unifyResultSettings.Return200StatusCodes.Contains(statusCode)) context.Response.StatusCode = 200;
        else { }
    }

    /// <summary>
    /// 获取序列化配置
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static object GetSerializerSettings(FilterContext context)
    {
        // 获取控制器信息
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor) return null;

        // 获取序列化配置
        var unifySerializerSettingAttribute = actionDescriptor.MethodInfo.GetFoundAttribute<UnifySerializerSettingAttribute>(true);
        if (unifySerializerSettingAttribute == null || string.IsNullOrWhiteSpace(unifySerializerSettingAttribute.Name)) return null;

        // 解析全局配置
        var succeed = UnifySerializerSettings.TryGetValue(unifySerializerSettingAttribute.Name, out var serializerSettings);
        return succeed ? serializerSettings : null;
    }

    /// <summary>
    /// 获取序列化配置
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static object GetSerializerSettings(string name)
    {
        // 解析全局配置
        var succeed = UnifySerializerSettings.TryGetValue(name, out var serializerSettings);
        return succeed ? serializerSettings : null;
    }

    /// <summary>
    /// 获取序列化配置
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static object GetSerializerSettings(DefaultHttpContext context)
    {
        // 获取终点路由特性
        var endpointFeature = context?.Features?.Get<IEndpointFeature>();
        if (endpointFeature == null) return null;

        // 获取序列化配置
        var unifySerializerSettingAttribute = context.GetMetadata<UnifySerializerSettingAttribute>() ?? endpointFeature?.Endpoint?.Metadata?.GetMetadata<UnifySerializerSettingAttribute>();
        if (unifySerializerSettingAttribute == null || string.IsNullOrWhiteSpace(unifySerializerSettingAttribute.Name)) return null;

        // 解析全局配置
        var succeed = UnifySerializerSettings.TryGetValue(unifySerializerSettingAttribute.Name, out var serializerSettings);
        return succeed ? serializerSettings : null;
    }

    /// <summary>
    /// 检查请求成功是否进行规范化处理
    /// </summary>
    /// <param name="method"></param>
    /// <param name="unifyResult"></param>
    /// <param name="isWebRequest"></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    internal static bool CheckSucceededNonUnify(MethodInfo method, out IUnifyResultProvider unifyResult, bool isWebRequest = true)
    {
        // 解析规范化元数据
        var unityMetadata = GetMethodUnityMetadata(method);

        // 判断是否跳过规范化处理
        var isSkip = !EnabledUnifyHandler
              || method.GetRealReturnType().HasImplementedRawGeneric(unityMetadata.ResultType)
              || method.CustomAttributes.Any(x => typeof(NonUnifyAttribute).IsAssignableFrom(x.AttributeType) || typeof(ProducesResponseTypeAttribute).IsAssignableFrom(x.AttributeType) || typeof(IApiResponseMetadataProvider).IsAssignableFrom(x.AttributeType))
              || method.ReflectedType.IsDefined(typeof(NonUnifyAttribute), true)
              || method.DeclaringType.Assembly.GetName().Name.StartsWith("Microsoft.AspNetCore.OData");

        if (!isWebRequest)
        {
            unifyResult = null;
            return isSkip;
        }

        unifyResult = isSkip ? null : App.RootServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
        return unifyResult == null || isSkip;
    }

    /// <summary>
    /// 检查请求失败（验证失败、抛异常）是否进行规范化处理
    /// </summary>
    /// <param name="method"></param>
    /// <param name="unifyResult"></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    internal static bool CheckFailedNonUnify(MethodInfo method, out IUnifyResultProvider unifyResult)
    {
        // 解析规范化元数据
        var unityMetadata = GetMethodUnityMetadata(method);

        // 判断是否跳过规范化处理
        var isSkip = !EnabledUnifyHandler
                || method.CustomAttributes.Any(x => typeof(NonUnifyAttribute).IsAssignableFrom(x.AttributeType))
                || (
                        !method.CustomAttributes.Any(x => typeof(ProducesResponseTypeAttribute).IsAssignableFrom(x.AttributeType) || typeof(IApiResponseMetadataProvider).IsAssignableFrom(x.AttributeType))
                        && method.ReflectedType.IsDefined(typeof(NonUnifyAttribute), true)
                    )
                || method.DeclaringType.Assembly.GetName().Name.StartsWith("Microsoft.AspNetCore.OData");

        unifyResult = isSkip ? null : App.RootServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
        return unifyResult == null || isSkip;
    }

    /// <summary>
    /// 检查短路状态码（>=400）是否进行规范化处理
    /// </summary>
    /// <param name="context"></param>
    /// <param name="unifyResult"></param>
    /// <returns>返回 true 跳过处理，否则进行规范化处理</returns>
    internal static bool CheckExceptionHttpContextNonUnify(HttpContext context, out IUnifyResultProvider unifyResult)
    {
        // 获取终点路由特性
        var endpointFeature = context.Features?.Get<IEndpointFeature>();
        if (endpointFeature == null) return (unifyResult = null) == null;

        // 判断是否跳过规范化处理
        var isSkip = !EnabledUnifyHandler
                || context.GetMetadata<NonUnifyAttribute>() != null
                || endpointFeature?.Endpoint?.Metadata?.GetMetadata<NonUnifyAttribute>() != null
                || context.Request.Headers["accept"].ToString().Contains("odata.metadata=", StringComparison.OrdinalIgnoreCase)
                || context.Request.Headers["accept"].ToString().Contains("odata.streaming=", StringComparison.OrdinalIgnoreCase)
                || ResponseContentTypesOfNonUnify.Any(u => context.Response.Headers["content-type"].ToString().Contains(u, StringComparison.OrdinalIgnoreCase));

        if (isSkip == true) unifyResult = null;
        else
        {
            // 解析规范化元数据
            var unifyProviderAttribute = endpointFeature?.Endpoint?.Metadata?.GetMetadata<UnifyProviderAttribute>();
            UnifyProviders.TryGetValue(unifyProviderAttribute?.Name ?? string.Empty, out var unityMetadata);

            unifyResult = unityMetadata?.ProviderType == null
                ? null
                : context.RequestServices.GetService(unityMetadata.ProviderType) as IUnifyResultProvider;
        }

        return unifyResult == null || isSkip;
    }

    /// <summary>
    /// 判断是否支持 Mvc 控制器规范化处理
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="actionDescriptor"></param>
    /// <param name="unifyResultSettings"></param>
    /// <returns></returns>
    internal static bool CheckSupportMvcController(HttpContext httpContext, ControllerActionDescriptor actionDescriptor, out UnifyResultSettingsOptions unifyResultSettings)
    {
        // 获取规范化配置选项
        unifyResultSettings = httpContext.RequestServices.GetService<IOptions<UnifyResultSettingsOptions>>()?.Value;

        // 如果未启用 MVC 规范化处理，则跳过
        if (unifyResultSettings?.SupportMvcController == false && typeof(Controller).IsAssignableFrom(actionDescriptor.ControllerTypeInfo)) return false;

        return true;
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
    internal static bool CheckVaildResult(IActionResult result, out object data)
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

    /// <summary>
    /// 获取方法规范化元数据
    /// </summary>
    /// <remarks>如果追求性能，这里理应缓存起来，避免每次请求去检测</remarks>
    /// <param name="method"></param>
    /// <returns></returns>
    internal static UnifyMetadata GetMethodUnityMetadata(MethodInfo method)
    {
        if (method == default) return default;

        var unityProviderAttribute = method.GetFoundAttribute<UnifyProviderAttribute>(true);

        // 获取元数据
        var isExists = UnifyProviders.TryGetValue(unityProviderAttribute?.Name ?? string.Empty, out var metadata);
        if (!isExists)
        {
            // 不存在则将默认的返回
            UnifyProviders.TryGetValue(string.Empty, out metadata);
        }

        return metadata;
    }
}