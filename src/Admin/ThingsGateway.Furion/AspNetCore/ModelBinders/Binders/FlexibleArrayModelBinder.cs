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
/// 数组 URL 地址参数模型绑定
/// </summary>
internal sealed class FlexibleArrayModelBinder<T> : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // 空检查
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        // 获取模型名和类型
        var modelName = bindingContext.ModelName;
        var modelType = bindingContext.ModelType;

        // 获取 URL 参数集合
        var queryCollection = bindingContext.HttpContext.Request.Query;

        // 尝试从 status[] 获取值
        if (queryCollection.ContainsKey(modelName + "[]"))
        {
            var values = ConvertValues(queryCollection[modelName + "[]"], modelType);
            bindingContext.Result = ModelBindingResult.Success(values);

            return Task.CompletedTask;
        }

        // 尝试从 status 获取逗号分隔的值
        var commaSeparatedValue = queryCollection[modelName];
        if (!string.IsNullOrEmpty(commaSeparatedValue))
        {
            var values = ConvertValues(commaSeparatedValue.ToString().Split(',').Where(s => !string.IsNullOrWhiteSpace(s)), modelType);
            bindingContext.Result = ModelBindingResult.Success(values);

            return Task.CompletedTask;
        }

        // 如果以上两种情况都不满足，尝试将多个 status 参数合并
        var individualValues = queryCollection[modelName];
        if (individualValues.Count > 0)
        {
            var values = ConvertValues(individualValues, modelType);
            bindingContext.Result = ModelBindingResult.Success(values);

            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 转换集合类型值为模型类型值
    /// </summary>
    /// <param name="values"></param>
    /// <param name="modelType"></param>
    /// <returns></returns>
    private static object ConvertValues(IEnumerable<string> values, Type modelType)
    {
        // 处理数组类型
        if (modelType.IsArray)
        {
            return values.Select(u => u.ChangeType<T>()).ToArray();
        }
        // 处理 List 类型
        if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return values.Select(u => u.ChangeType<T>()).ToList();
        }

        // IEnumerable<T> 类型
        return values.Select(u => u.ChangeType<T>());
    }
}