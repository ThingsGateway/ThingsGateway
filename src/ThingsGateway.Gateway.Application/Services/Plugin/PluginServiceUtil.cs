﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using CSScripting;

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class PluginServiceUtil
{
    /// <summary>
    /// 属性赋值方法
    /// </summary>
    /// <param name="dest"></param>
    /// <param name="source"></param>
    public static void CopyValue(this IEditorItem dest, IEditorItem source)
    {
        if (source.ComponentType != null) dest.ComponentType = source.ComponentType;
        if (source.ComponentParameters != null) dest.ComponentParameters = source.ComponentParameters;
        if (source.Ignore.HasValue) dest.Ignore = source.Ignore;
        if (source.EditTemplate != null) dest.EditTemplate = source.EditTemplate;
        if (source.Items != null) dest.Items = source.Items;
        if (source.Lookup != null) dest.Lookup = source.Lookup;
        if (source.ShowSearchWhenSelect) dest.ShowSearchWhenSelect = source.ShowSearchWhenSelect;
        if (source.IsPopover) dest.IsPopover = source.IsPopover;
        if (source.LookupStringComparison != StringComparison.OrdinalIgnoreCase) dest.LookupStringComparison = source.LookupStringComparison;
        if (source.LookupServiceKey != null) dest.LookupServiceKey = source.LookupServiceKey;
        if (source.LookupServiceData != null) dest.LookupServiceData = source.LookupServiceData;
        if (source.Readonly.HasValue) dest.Readonly = source.Readonly;
        if (source.Rows > 0) dest.Rows = source.Rows;
        if (source.SkipValidate) dest.SkipValidate = source.SkipValidate;
        if (!string.IsNullOrEmpty(source.Text)) dest.Text = source.Text;
        if (source.ValidateRules != null) dest.ValidateRules = source.ValidateRules;
        if (source.ShowLabelTooltip != null) dest.ShowLabelTooltip = source.ShowLabelTooltip;
        if (!string.IsNullOrEmpty(source.GroupName)) dest.GroupName = source.GroupName;
        if (source.GroupOrder != 0) dest.GroupOrder = source.GroupOrder;
        if (!string.IsNullOrEmpty(source.PlaceHolder)) dest.PlaceHolder = source.PlaceHolder;
        if (!string.IsNullOrEmpty(source.Step)) dest.Step = source.Step;
        if (source.Order != 0) dest.Order = source.Order;
        if (source.Required.HasValue) dest.Required = source.Required;
        if (!string.IsNullOrEmpty(source.RequiredErrorMessage)) dest.RequiredErrorMessage = source.RequiredErrorMessage;

        if (source is ITableColumn source1 && dest is ITableColumn dest1)
        {
            CopyValue(dest1, source1);
        }
    }
    private static void CopyValue(this ITableColumn dest, ITableColumn source)
    {
        if (source.Align.HasValue) dest.Align = source.Align;
        if (source.TextWrap.HasValue) dest.TextWrap = source.TextWrap;
        if (!string.IsNullOrEmpty(source.CssClass)) dest.CssClass = source.CssClass;
        if (source.DefaultSort) dest.DefaultSort = source.DefaultSort;
        if (source.DefaultSortOrder != SortOrder.Unset) dest.DefaultSortOrder = source.DefaultSortOrder;
        if (source.Filter != null) dest.Filter = source.Filter;
        if (source.Filterable.HasValue) dest.Filterable = source.Filterable;
        if (source.FilterTemplate != null) dest.FilterTemplate = source.FilterTemplate;
        if (source.Fixed) dest.Fixed = source.Fixed;
        if (source.FormatString != null) dest.FormatString = source.FormatString;
        if (source.Formatter != null) dest.Formatter = source.Formatter;
        if (source.HeaderTemplate != null) dest.HeaderTemplate = source.HeaderTemplate;
        if (source.OnCellRender != null) dest.OnCellRender = source.OnCellRender;
        if (source.Searchable.HasValue) dest.Searchable = source.Searchable;
        if (source.SearchTemplate != null) dest.SearchTemplate = source.SearchTemplate;
        if (source.ShownWithBreakPoint != BreakPoint.None) dest.ShownWithBreakPoint = source.ShownWithBreakPoint;
        if (source.ShowTips.HasValue) dest.ShowTips = source.ShowTips = true;
        if (source.Sortable.HasValue) dest.Sortable = source.Sortable;
        if (source.Template != null) dest.Template = source.Template;
        if (source.TextEllipsis.HasValue) dest.TextEllipsis = source.TextEllipsis;
        if (!source.Visible.HasValue) dest.Visible = source.Visible;
        if (source.Width != null) dest.Width = source.Width;
        if (source.ShowCopyColumn.HasValue) dest.ShowCopyColumn = source.ShowCopyColumn;
        if (source.HeaderTextWrap) dest.HeaderTextWrap = source.HeaderTextWrap;
        if (!string.IsNullOrEmpty(source.HeaderTextTooltip)) dest.HeaderTextTooltip = source.HeaderTextTooltip;
        if (source.ShowHeaderTooltip) dest.ShowHeaderTooltip = source.ShowHeaderTooltip;
        if (source.HeaderTextEllipsis) dest.HeaderTextEllipsis = source.HeaderTextEllipsis;
        if (source.IsMarkupString) dest.IsMarkupString = source.IsMarkupString;
        if (source.Visible.HasValue) dest.Visible = source.Visible;
        if (source.IsVisibleWhenAdd.HasValue) dest.IsVisibleWhenAdd = source.IsVisibleWhenAdd;
        if (source.IsVisibleWhenEdit.HasValue) dest.IsVisibleWhenEdit = source.IsVisibleWhenEdit;
        if (source.IsReadonlyWhenAdd.HasValue) dest.IsReadonlyWhenAdd = source.IsReadonlyWhenAdd;
        if (source.IsReadonlyWhenEdit.HasValue) dest.IsReadonlyWhenEdit = source.IsReadonlyWhenEdit;
        if (source.GetTooltipTextCallback != null) dest.GetTooltipTextCallback = source.GetTooltipTextCallback;
        if (source.CustomSearch != null) dest.CustomSearch = source.CustomSearch;
        if (source.ToolboxTemplate != null) dest.ToolboxTemplate = source.ToolboxTemplate;
        if (source.IsRequiredWhenAdd.HasValue) dest.IsRequiredWhenAdd = source.IsRequiredWhenAdd;
        if (source.IsRequiredWhenEdit.HasValue) dest.IsRequiredWhenEdit = source.IsRequiredWhenEdit;
    }

    /// <summary>
    /// 通过特定类型模型获取模型属性集合
    /// </summary>
    /// <param name="type">绑定模型类型</param>
    /// <returns></returns>
    public static IEnumerable<IEditorItem> GetEditorItems(Type type)
    {
        var cols = new List<IEditorItem>(50);
        //获取属性
        var props = type.GetProperties().Where(p => !p.IsStatic());
        foreach (var prop in props)
        {
            //获取插件自定义属性
            var classAttribute = prop.GetCustomAttribute<DynamicPropertyAttribute>(false);
            var autoGenerateColumnAttribute = prop.GetCustomAttribute<AutoGenerateColumnAttribute>(true);
            if (classAttribute == null) continue;//删除不需要显示的属性

            IEditorItem? tc;
            var displayName = classAttribute.Description ?? BootstrapBlazor.Components.Utility.GetDisplayName(type, prop.Name);
            tc = new InternalTableColumn(prop.Name, prop.PropertyType, displayName);
            if (autoGenerateColumnAttribute != null)
                CopyValue(tc, autoGenerateColumnAttribute);
            tc.ComponentParameters ??= new Dictionary<string, object>();
            if (classAttribute.Remark != null)
            {
                var dict = new Dictionary<string, object>
                {
                    { "title", classAttribute.Remark }
                };
                tc.ComponentParameters.AddItem(
                   new("title", classAttribute.Remark)
               );
            }

            cols.Add(tc);
        }
        return cols;
    }

    /// <summary>
    /// 根据插件FullName获取插件主程序集名称和插件类型名称
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public static (string FileName, string TypeName) GetFileNameAndTypeName(string pluginName)
    {
        if (pluginName.IsNullOrWhiteSpace())
            return (string.Empty, string.Empty);
        // 查找最后一个 '.' 的索引
        int lastIndex = pluginName.LastIndexOf('.');

        // 如果找到了最后一个 '.'，并且它不是最后一个字符
        if (lastIndex != -1 && lastIndex < pluginName.Length - 1)
        {
            // 获取子串直到最后一个 '.'
            string part1 = pluginName.Substring(0, lastIndex);
            // 获取最后一个 '.' 后面的部分
            string part2 = pluginName.Substring(lastIndex + 1);
            return (part1, part2);
        }
        else
        {
            // 如果没有找到 '.'，或者 '.' 是最后一个字符，则返回默认的键和插件名称
            return (nameof(ThingsGateway), pluginName);
        }
    }

    /// <summary>
    /// 根据插件主程序集名称和插件类型名称获取插件FullName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetFullName(string fileName, string name)
    {
        return string.IsNullOrEmpty(fileName) ? name : $"{fileName}.{name}";
    }

    public static bool HasDynamicProperty(object model)
    {
        var type = model.GetType();
        return type.GetRuntimeProperties().Any(a => a.GetCustomAttribute<DynamicPropertyAttribute>(false) != null);
    }

    /// <summary>
    /// 插件是否支持平台
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSupported(Type type)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return !Attribute.IsDefined(type, typeof(OnlyWindowsSupportAttribute));

        return true;
    }

    /// <summary>
    /// 通过实体赋值到字典中
    /// </summary>
    public static ConcurrentDictionary<long, Dictionary<string, string>> SetDict(ConcurrentDictionary<long, ModelValueValidateForm>? models)
    {
        ConcurrentDictionary<long, Dictionary<string, string>> results = new();
        models ??= new();
        foreach (var model in models)
        {
            var data = SetDict(model.Value.Value);
            results.TryAdd(model.Key, data);
        }
        return results;
    }

    /// <summary>
    /// 通过实体赋值到字典中
    /// </summary>
    /// <param name="model"></param>
    public static Dictionary<string, string> SetDict(object model)
    {
        Type type = model.GetType();
        var properties = type.GetRuntimeProperties();
        Dictionary<string, string> dict = new();
        foreach (var property in properties)
        {
            string propertyName = property.Name;
            // 如果属性存在且可写
            if (property != null && property.CanWrite)
            {
                // 进行类型转换
                var value = ThingsGatewayStringConverter.Default.Serialize(null, property.GetValue(model));
                dict.Add(propertyName, value);
            }
        }
        return dict;
    }

    /// <summary>
    /// 通过字典赋值到实体中
    /// </summary>
    /// <param name="model"></param>
    /// <param name="dict"></param>
    public static void SetModel(object model, Dictionary<string, string> dict)
    {
        Type type = model.GetType();
        var properties = type.GetRuntimeProperties();

        foreach (var property in properties)
        {
            string propertyName = property.Name;

            // 如果属性存在且可写
            if (property != null && property.CanWrite)
            {
                if (dict.TryGetValue(propertyName, out var dictValue))
                {
                    // 进行类型转换
                    object value = ThingsGatewayStringConverter.Default.Deserialize(null, dictValue, property.PropertyType);
                    // 设置属性值
                    property.SetValue(model, value);
                }
            }
        }
    }

}
