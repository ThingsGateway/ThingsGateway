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

using Microsoft.AspNetCore.Mvc.ModelBinding;

using ThingsGateway.Extensions;

namespace ThingsGateway.AspNetCore;

/// <summary>
/// 时间戳转 DateTime 类型模型绑定
/// </summary>
[SuppressSniffer]
public sealed class TimestampToDateTimeModelBinder : IModelBinder
{
    /// <summary>
    /// 绑定模型处理
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        // 获取模型名称（参数/属性/类名）
        var modelName = bindingContext.ModelName;

        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        // 获取值
        var value = valueProviderResult.FirstValue;

        var modelType = bindingContext.ModelType;
        var actType = modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? modelType.GenericTypeArguments[0]
            : modelType;

        DateTime dateTime;

        try
        {
            // 处理时间戳
            if (long.TryParse(value, out var timestamp))
            {
                dateTime = timestamp.ConvertToDateTime();
            }
            else
            {
                dateTime = Convert.ToDateTime(value);
            }

            if (actType == typeof(DateTime))
            {
                bindingContext.Result = ModelBindingResult.Success(dateTime);
            }
            else if (actType == typeof(DateTimeOffset))
            {
                bindingContext.Result = ModelBindingResult.Success(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
            }
        }
        catch
        {
            bindingContext.ModelState.TryAddModelError(modelName, $"The value '{value}' is not valid for {modelName}.");
        }

        return Task.CompletedTask;
    }
}