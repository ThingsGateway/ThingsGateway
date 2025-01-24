//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;

namespace ThingsGateway.Razor;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public class LocalizerUtil
{
    #region 是否启用

    public static List<SelectedItem> GetBoolItems(Type modelType, string fieldName, bool canNull = false)
    {
        var cacheKey = $"{nameof(GetBoolItems)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}-{fieldName}";
        return App.CacheService.GetOrAdd(cacheKey, entry =>
        {
            var items = new List<SelectedItem>();
            var localizer = modelType.Assembly.IsDynamic ? null : App.CreateLocalizerByType(modelType);
            IStringLocalizer? localizerAttr = null;
            if (canNull)
            {
                items.Add(new SelectedItem("", FindDisplayText(nameof(NullableBoolItemsAttribute.NullValueDisplayText), attr => attr.FalseValueDisplayText)));
            }

            items.Add(new SelectedItem("True", FindDisplayText(nameof(NullableBoolItemsAttribute.TrueValueDisplayText), attr => attr.TrueValueDisplayText)));
            items.Add(new SelectedItem("False", FindDisplayText(nameof(NullableBoolItemsAttribute.FalseValueDisplayText), attr => attr.FalseValueDisplayText)));

            return items;

            string FindDisplayText(string itemName, Func<NullableBoolItemsAttribute, string?> callback)
            {
                string? dn = null;

                // 优先读取资源文件配置
                var stringLocalizer = localizer?[$"{fieldName}-{itemName}"];
                if (stringLocalizer is { ResourceNotFound: false })
                {
                    dn = stringLocalizer.Value;
                }
                else if (Utility.TryGetProperty(modelType, fieldName, out var propertyInfo))
                {
                    // 类资源文件未找到 回落查找标签
                    var attr = propertyInfo.GetCustomAttribute<NullableBoolItemsAttribute>(true);
                    if (attr != null && !modelType.Assembly.IsDynamic)
                    {
                        dn = callback(attr);
                    }
                }

                // 回落读取 NullableBoolItemsAttribute 资源文件配置
                return dn ?? FindDisplayTextByItemName(itemName);
            }

            string FindDisplayTextByItemName(string itemName)
            {
                localizerAttr ??= App.CreateLocalizerByType(typeof(NullableBoolItemsAttribute));
                var stringLocalizer = localizerAttr![itemName];
                return stringLocalizer.Value;
            }
        });
    }

    #endregion

}
