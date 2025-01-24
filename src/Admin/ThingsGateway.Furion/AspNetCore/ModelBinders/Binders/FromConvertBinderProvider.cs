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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

using System.Collections.Concurrent;

using ThingsGateway.AspNetCore;

namespace ThingsGateway.SensitiveDetection;

/// <summary>
/// [FromConvert] 模型绑定提供器
/// </summary>
[SuppressSniffer]
public class FromConvertBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// 定义模型绑定转换器集合
    /// </summary>
    private readonly ConcurrentDictionary<Type, Type> _modelBinderConverts;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="modelBinderConverts">定义模型绑定转换器集合</param>
    public FromConvertBinderProvider(ConcurrentDictionary<Type, Type> modelBinderConverts)
    {
        _modelBinderConverts = modelBinderConverts;
    }

    /// <summary>
    /// 返回自定义绑定器
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // 判断是否定义 [FromConvert] 特性
        if (context.Metadata is DefaultModelMetadata actMetadata
            && actMetadata.Attributes.ParameterAttributes != null
            && actMetadata.Attributes.ParameterAttributes.Count > 0
            && actMetadata.Attributes.ParameterAttributes.Any(u => u.GetType() == typeof(FromConvertAttribute)))
        {
            return new FromConvertBinder(_modelBinderConverts);
        }

        return null;
    }
}