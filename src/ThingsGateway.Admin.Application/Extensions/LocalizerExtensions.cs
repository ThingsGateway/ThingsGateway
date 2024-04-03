//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Localization;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace ThingsGateway.Admin.Application;

public static class LocalizerExtensions
{
    public static PropertyInfo? GetPropertyByName(this Type type, string propertyName) => type.GetRuntimeProperties().FirstOrDefault(p => p.Name == propertyName);

    public static MethodInfo? GetMethodByName(this Type type, string methodName) => type.GetRuntimeMethods().FirstOrDefault(p => p.Name == methodName);

    public static FieldInfo? GetFieldByName(this Type type, string fieldName) => type.GetRuntimeFields().FirstOrDefault(p => p.Name == fieldName);

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