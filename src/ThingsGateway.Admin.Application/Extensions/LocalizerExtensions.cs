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
using Microsoft.Extensions.Localization;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.Admin.Application;

public static class LocalizerExtensions
{
    public static PropertyInfo? GetPropertyByName(this Type type, string propertyName) => type.GetRuntimeProperties().FirstOrDefault(p => p.Name == propertyName);

    public static MethodInfo? GetMethodByName(this Type type, string methodName) => type.GetRuntimeMethods().FirstOrDefault(p => p.Name == methodName);

    public static FieldInfo? GetFieldByName(this Type type, string fieldName) => type.GetRuntimeFields().FirstOrDefault(p => p.Name == fieldName);

    private static bool IsPublic(PropertyInfo p) => p.GetMethod != null && p.SetMethod != null && p.GetMethod.IsPublic && p.SetMethod.IsPublic;

    /// <summary>
    /// 验证整个模型时验证属性方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="results"></param>
    public static void ValidateProperty(this ValidationContext context, List<ValidationResult> results)
    {
        // 获得所有可写属性
        var properties = context.ObjectType.GetRuntimeProperties().Where(p => IsPublic(p) && p.CanWrite && p.GetIndexParameters().Length == 0);
        foreach (var pi in properties)
        {
            var fieldIdentifier = new FieldIdentifier(context.ObjectInstance, pi.Name);
            context.DisplayName = fieldIdentifier.GetDisplayName();
            context.MemberName = fieldIdentifier.FieldName;

            var propertyValue = BootstrapBlazor.Components.Utility.GetPropertyValue(context.ObjectInstance, context.MemberName);

            // 验证 DataAnnotations
            var messages = new List<ValidationResult>();
            // 组件进行验证
            ValidateDataAnnotations(propertyValue, context, messages, pi);
            if (messages.Count > 0)
                results.AddRange(messages);
        }
    }

    /// <summary>
    /// 通过属性设置的 DataAnnotation 进行数据验证
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <param name="results"></param>
    /// <param name="propertyInfo"></param>
    /// <param name="memberName"></param>
    private static void ValidateDataAnnotations(object? value, ValidationContext context, List<ValidationResult> results, PropertyInfo propertyInfo, string? memberName = null)
    {
        var rules = propertyInfo.GetCustomAttributes(true).OfType<ValidationAttribute>();
        var metadataType = context.ObjectType.GetCustomAttribute<MetadataTypeAttribute>(false);
        if (metadataType != null)
        {
            var p = metadataType.MetadataClassType.GetPropertyByName(propertyInfo.Name);
            if (p != null)
            {
                rules = rules.Concat(p.GetCustomAttributes(true).OfType<ValidationAttribute>());
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
                var find = false;
                var ruleNameSpan = rule.GetType().Name.AsSpan();
                var index = ruleNameSpan.IndexOf(attributeSpan, StringComparison.OrdinalIgnoreCase);
                var ruleName = ruleNameSpan[..index];
                //// 通过设置 ErrorMessage 检索
                //if (!context.ObjectType.Assembly.IsDynamic && !find
                //    && !string.IsNullOrEmpty(rule.ErrorMessage)
                //    && App.CreateLocalizerByType(context.ObjectType).TryGetLocalizerString(rule.ErrorMessage, out var msg))
                //{
                //    rule.ErrorMessage = msg;
                //    find = true;
                //}

                //// 通过 Attribute 检索
                //if (!rule.GetType().Assembly.IsDynamic && !find
                //    && App.CreateLocalizerByType(rule.GetType()).TryGetLocalizerString(nameof(rule.ErrorMessage), out msg))
                //{
                //    rule.ErrorMessage = msg;
                //    find = true;
                //}

                // 通过 字段.规则名称 检索
                if (!context.ObjectType.Assembly.IsDynamic && !find
                    && App.CreateLocalizerByType(context.ObjectType).TryGetLocalizerString($"{memberName}.{ruleName.ToString()}", out var msg))
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
    /// 获取指定 Type 的资源文件
    /// </summary>
    /// <param name="localizer"></param>
    /// <param name="key"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool TryGetLocalizerString(this IStringLocalizer localizer, string key, [MaybeNullWhen(false)] out string? text)
    {
        var ret = false;
        text = null;
        var l = localizer[key];
        if (l != null)
        {
            ret = !l.ResourceNotFound;
            if (ret)
            {
                text = l.Value;
            }
        }
        return ret;
    }

    /// <summary>
    /// 获得类型自身的描述信息
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static string GetTypeDisplayName(this Type modelType)
    {
        string fieldName = modelType.Name;
        var cacheKey = $"{nameof(GetTypeDisplayName)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}";
        var displayName = App.CacheService.GetOrCreate(cacheKey, entry =>
        {
            string? dn = null;
            // 显示名称为空时通过资源文件查找 FieldName 项
            var localizer = modelType.Assembly.IsDynamic ? null : App.CreateLocalizerByType(modelType);
            var stringLocalizer = localizer?[fieldName];
            if (stringLocalizer is { ResourceNotFound: false })
            {
                dn = stringLocalizer.Value;
            }
            else if (modelType.IsEnum)
            {
                var info = modelType.GetFieldByName(fieldName);
                if (info != null)
                {
                    dn = FindDisplayAttribute(info);
                }
            }
            else if (TryGetProperty(modelType, fieldName, out var propertyInfo))
            {
                dn = FindDisplayAttribute(propertyInfo);
            }

            return dn;
        }, 300);

        return displayName ?? fieldName;

        string? FindDisplayAttribute(MemberInfo memberInfo)
        {
            // 回退查找 Display 标签
            var dn = memberInfo.GetCustomAttribute<DisplayAttribute>(true)?.Name
                ?? memberInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName
                ?? memberInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description;

            return dn;
        }
    }

    /// <summary>
    /// 获取 DisplayName属性名称
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string Description<T>(this T item, Expression<Func<T, object>> accessor)
    {
        if (accessor.Body == null)
        {
            throw new ArgumentNullException(nameof(accessor));
        }

        var expression = accessor.Body;
        if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert && unaryExpression.Type == typeof(object))
        {
            expression = unaryExpression.Operand;
        }

        if (expression is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Can only access properties");
        }

        return typeof(T).GetPropertyDisplayName(memberExpression.Member.Name) ?? memberExpression.Member.Name;
    }

    /// <summary>
    /// 获得类型属性的描述信息
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static string GetPropertyDisplayName(this Type modelType, string fieldName)
    {
        var cacheKey = $"{nameof(GetPropertyDisplayName)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}-{fieldName}";
        var displayName = App.CacheService.GetOrCreate(cacheKey, entry =>
        {
            string? dn = null;
            // 显示名称为空时通过资源文件查找 FieldName 项
            var localizer = modelType.Assembly.IsDynamic ? null : App.CreateLocalizerByType(modelType);
            var stringLocalizer = localizer?[fieldName];
            if (stringLocalizer is { ResourceNotFound: false })
            {
                dn = stringLocalizer.Value;
            }
            else if (modelType.IsEnum)
            {
                var info = modelType.GetFieldByName(fieldName);
                if (info != null)
                {
                    dn = FindDisplayAttribute(info);
                }
            }
            else if (TryGetProperty(modelType, fieldName, out var propertyInfo))
            {
                dn = FindDisplayAttribute(propertyInfo);
            }

            return dn;
        }, 300);

        return displayName ?? fieldName;

        string? FindDisplayAttribute(MemberInfo memberInfo)
        {
            // 回退查找 Display 标签
            var dn = memberInfo.GetCustomAttribute<DisplayAttribute>(true)?.Name
                ?? memberInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName
                ?? memberInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description;

            return dn;
        }
    }

    /// <summary>
    /// 获得方法的描述信息
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static string GetMethodDisplayName(this Type modelType, string methodName)
    {
        var cacheKey = $"{nameof(GetMethodDisplayName)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}-{methodName}";
        var displayName = App.CacheService.GetOrCreate(cacheKey, entry =>
        {
            string? dn = null;
            // 显示名称为空时通过资源文件查找 methodName 项
            var localizer = modelType.Assembly.IsDynamic ? null : App.CreateLocalizerByType(modelType);
            var stringLocalizer = localizer?[methodName];
            if (stringLocalizer is { ResourceNotFound: false })
            {
                dn = stringLocalizer.Value;
            }
            else
            {
                var info = modelType.GetMethodByName(methodName);
                if (info != null)
                {
                    dn = FindDisplayAttribute(info);
                }
            }

            return dn;
        }, 300);

        return displayName ?? methodName;

        string? FindDisplayAttribute(MemberInfo memberInfo)
        {
            // 回退查找 Display 标签
            var dn = memberInfo.GetCustomAttribute<DisplayAttribute>(true)?.Name
                ?? memberInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName
                ?? memberInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description;

            return dn;
        }
    }

    private static bool TryGetProperty(Type modelType, string fieldName, [NotNullWhen(true)] out PropertyInfo? propertyInfo)
    {
        var cacheKey = $"{nameof(TryGetProperty)}-{modelType.FullName}-{modelType.TypeHandle.Value}-{fieldName}";
        propertyInfo = App.CacheService.GetOrCreate(cacheKey, entry =>
        {
            IEnumerable<PropertyInfo>? props;

            // 支持 MetadataType
            var metadataType = modelType.GetCustomAttribute<MetadataTypeAttribute>(false);
            if (metadataType != null)
            {
                props = modelType.GetRuntimeProperties().AsEnumerable().Concat(metadataType.MetadataClassType.GetRuntimeProperties());
            }
            else
            {
                props = modelType.GetRuntimeProperties().AsEnumerable();
            }

            var pi = props.FirstOrDefault(p => p.Name == fieldName);

            return pi;
        }, 300);
        return propertyInfo != null;
    }
}
