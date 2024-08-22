//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

using ThingsGateway.Core.Json.Extension;

namespace ThingsGateway.ASPNetCore;

/// <summary>
/// 规范化RESTful风格返回值
/// </summary>
public class ResultFilter : IAsyncActionFilter
{
    public const string ValidationFailedKey = $"{nameof(ResultFilter)}Validate";

    /// <summary>
    /// 获取异常元数据
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetExceptionMetadata(ActionContext context)
    {
        // 判断是否是 ExceptionContext 或者 ActionExecutedContext
        var exception = context is ExceptionContext exContext
            ? exContext.Exception
            : (
                context is ActionExecutedContext edContext
                ? edContext.Exception
                : default
            );

        string? errors = exception?.InnerException?.Message ?? exception?.Message;
        return errors;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 排除 WebSocket 请求处理
        if (context.HttpContext.IsWebSocketRequest())
        {
            await next().ConfigureAwait(false);
            return;
        }
        var httpContext = context.HttpContext;
        var unifyResult = httpContext.RequestServices.GetRequiredService<IUnifyResultProvider>();

        #region 验证

        // 解析验证消息
        {
            if (!context.ModelState.IsValid)
            {
                var allValidationResults = new List<ValidationResult>();
                int errorCount = 0;
                //重新获取错误信息
                foreach (var item in context.ActionArguments)
                {
                    if (errorCount == context.ModelState.ErrorCount) break;
                    foreach (var parameter in context.ModelState)
                    {
                        if (errorCount == context.ModelState.ErrorCount) break;
                        var validationResults = new List<ValidationResult>();
                        var validationContext = new ValidationContext(item.Value!);
                        ValidateProperty(validationContext, validationResults, parameter.Key);
                        allValidationResults.AddRange(validationResults);
                        errorCount += validationResults.Count;
                    }
                }

                //var validationMetadata = GetValidationMetadata(context.ModelState!);
                if (allValidationResults.Count > 0)
                {
                    string? errorMessage;
                    if (allValidationResults.Count == 1)
                    {
                        errorMessage = allValidationResults.FirstOrDefault()!.ErrorMessage;
                    }
                    else
                    {
                        errorMessage = allValidationResults!.ToDictionary(a => a.MemberNames.FirstOrDefault()!, a => a.ErrorMessage).ToSystemTextJsonString();
                    }
                    var result = unifyResult.OnValidateFailed(context, errorMessage);
                    if (result != null)
                    {
                        context.Result = result;

                        // 存储验证执行结果
                        context.HttpContext.Items[ValidationFailedKey] = errorMessage;

                        return;
                    }
                }
            }
        }

        #endregion 验证

        // 执行 Action 并获取结果
        ActionExecutedContext? actionExecutedContext = await next().ConfigureAwait(false);

        #region 异常

        // 如果出现异常
        if (actionExecutedContext.Exception != null)
        {
            // 判断是否支持 MVC 规范化处理
            if (UnifyContext.CheckHttpContextNonUnify(httpContext)) return;
            // 执行规范化异常处理
            actionExecutedContext.Result = unifyResult.OnException(actionExecutedContext);
            actionExecutedContext.ExceptionHandled = true;

            return;
        }

        #endregion 异常

        #region 状态码

        // 处理已经含有状态码结果的 Result
        if (actionExecutedContext.Result is IStatusCodeActionResult statusCodeResult && statusCodeResult.StatusCode != null)
        {
            // 小于 200 或者 大于 299 都不是成功值，直接跳过
            if (statusCodeResult.StatusCode.Value < 200 || statusCodeResult.StatusCode.Value > 299)
            {
                // 处理规范化结果
                if (!UnifyContext.CheckStatusCodeNonUnify(httpContext))
                {
                    var statusCode = statusCodeResult.StatusCode.Value;

                    // 解决刷新 Token 时间和 Token 时间相近问题
                    if (statusCodeResult.StatusCode.Value == StatusCodes.Status401Unauthorized
                        && httpContext.Response.Headers.ContainsKey("access-token")
                        && httpContext.Response.Headers.ContainsKey("x-access-token"))
                    {
                        httpContext.Response.StatusCode = statusCode = StatusCodes.Status403Forbidden;
                    }

                    // 如果 Response 已经完成输出，则禁止写入
                    if (httpContext.Response.HasStarted) return;

                    await unifyResult.OnResponseStatusCodes(httpContext, statusCode).ConfigureAwait(false);
                }

                return;
            }
        }

        #endregion 状态码

        #region 成功

        // 获取控制器信息
        var actionDescriptor = actionExecutedContext.ActionDescriptor as ControllerActionDescriptor;

        // 判断是否支持 MVC 规范化处理或特定检查
        if (UnifyContext.CheckHttpContextNonUnify(httpContext)) return;

        // 判断是否跳过规范化处理
        if (UnifyContext.CheckSucceededNonUnify(actionDescriptor!.MethodInfo)) return;

        // 处理 BadRequestObjectResult 类型规范化处理
        if (actionExecutedContext.Result is BadRequestObjectResult badRequestObjectResult)
        {
            // 解析验证消息
            var validationMetadata = GetValidationMetadata(badRequestObjectResult.Value!);

            var result = unifyResult.OnValidateFailed(context, validationMetadata);
            if (result != null) actionExecutedContext.Result = result;
        }
        else
        {
            IActionResult? result = default;

            // 检查是否是有效的结果（可进行规范化的结果）
            if (UnifyContext.CheckVaildResult(actionExecutedContext.Result!, out var data))
            {
                result = unifyResult.OnSucceeded(actionExecutedContext, data);
            }

            // 如果是不能规范化的结果类型，则跳过
            if (result == null) return;

            actionExecutedContext.Result = result;
        }

        #endregion 成功
    }

    /// <summary>
    /// 获取验证错误信息
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    internal static string GetValidationMetadata(object errors)
    {
        object? validationResults = null;
        string? message = default;

        // 判断是否是集合类型
        if (errors is IEnumerable && errors is not string)
        {
            // 如果是模型验证字典类型
            if (errors is ModelStateDictionary modelState)
            {
                // 将验证错误信息转换成字典并序列化成 Json
                validationResults = modelState.Where(u => modelState[u.Key]!.ValidationState == ModelValidationState.Invalid)
                        .ToDictionary(u => u.Key, u => modelState[u.Key]!.Errors.Select(c => c.ErrorMessage).ToArray());
            }
            // 如果是 ValidationProblemDetails 特殊类型
            else if (errors is ValidationProblemDetails validation)
            {
                validationResults = validation.Errors
                    .ToDictionary(u => u.Key, u => u.Value.ToArray());
            }
            // 如果是字典类型
            else if (errors is IDictionary<string, string[]> dicResults)
            {
                validationResults = dicResults;
            }

            message = JsonSerializer.Serialize(validationResults, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
        }

        return message;
    }

    /// <summary>
    /// 通过属性设置的 DataAnnotation 进行数据验证
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <param name="results"></param>
    /// <param name="propertyInfo"></param>
    /// <param name="memberName"></param>
    private void ValidateDataAnnotations(object? value, ValidationContext context, List<ValidationResult> results, PropertyInfo propertyInfo, string? memberName = null)
    {
        var rules = propertyInfo.GetCustomAttributes(true).OfType<ValidationAttribute>().ToList();
        var metadataType = context.ObjectType.GetCustomAttribute<MetadataTypeAttribute>(false);
        if (metadataType != null)
        {
            var p = metadataType.MetadataClassType.GetPropertyByName(propertyInfo.Name);
            if (p != null)
            {
                rules.AddRange(p.GetCustomAttributes(true).OfType<ValidationAttribute>());
            }
        }
        var displayName = context.DisplayName;
        memberName ??= propertyInfo.Name;
        var attributeSpan = nameof(Attribute).AsSpan();
        foreach (var rule in rules)
        {
            var result = rule.GetValidationResult(value, context);
            if (result != null && result != ValidationResult.Success)
            {
                // 查找 resx 资源文件中的 ErrorMessage
                var ruleNameSpan = rule.GetType().Name.AsSpan();
                var index = ruleNameSpan.IndexOf(attributeSpan, StringComparison.OrdinalIgnoreCase);
                var ruleName = ruleNameSpan[..index];
                var find = false;

                // 通过设置 ErrorMessage 检索
                if (!context.ObjectType.Assembly.IsDynamic && !find
                    && !string.IsNullOrEmpty(rule.ErrorMessage)
                    && NetCoreApp.CreateLocalizerByType(context.ObjectType)!.TryGetLocalizerString(rule.ErrorMessage, out var msg))
                {
                    rule.ErrorMessage = msg;
                    find = true;
                }

                // 通过 Attribute 检索
                if (!rule.GetType().Assembly.IsDynamic && !find
                    && NetCoreApp.CreateLocalizerByType(rule.GetType())!.TryGetLocalizerString(nameof(rule.ErrorMessage), out msg))
                {
                    rule.ErrorMessage = msg;
                    find = true;
                }
                // 通过 字段.规则名称 检索
                if (!context.ObjectType.Assembly.IsDynamic && !find
                    && NetCoreApp.CreateLocalizerByType(context.ObjectType)!.TryGetLocalizerString($"{memberName}.{ruleName.ToString()}", out msg))
                {
                    rule.ErrorMessage = msg;
                    find = true;
                }

                if (!find)
                {
                    rule.ErrorMessage = result.ErrorMessage;
                }
                var errorMessage = !string.IsNullOrEmpty(rule.ErrorMessage) && rule.ErrorMessage.Contains("{0}")
                    ? rule.FormatErrorMessage(displayName)
                    : rule.ErrorMessage;
                results.Add(new ValidationResult(errorMessage, new string[] { memberName }));
            }
        }
    }

    /// <summary>
    /// 验证整个模型时验证属性方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="results"></param>
    /// <param name="pName"></param>
    private void ValidateProperty(ValidationContext context, List<ValidationResult> results, string pName)
    {
        // 获得所有可写属性
        var pi = context.ObjectType.GetPropertyByName(pName);
        if (pi != null)
        {
            // 设置其关联属性字段
            var propertyValue = pi.GetValue(context.ObjectInstance);
            var fieldIdentifier = new FieldIdentifier(context.ObjectInstance, pi.Name);
            context.DisplayName = fieldIdentifier.GetDisplayName();
            context.MemberName = fieldIdentifier.FieldName;

            // 组件进行验证
            ValidateDataAnnotations(propertyValue, context, results, pi);
        }
    }
}
