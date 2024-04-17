
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Microsoft.AspNetCore.Components.Rendering;

namespace ThingsGateway.Razor;

public static class DisplayExtensions
{
    /// <summary>
    /// RenderTreeBuilder 扩展方法 通过 IEditorItem 与 model 创建 Display 组件
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="item"></param>
    /// <param name="model"></param>
    public static void CreateDisplayByFieldType(this RenderTreeBuilder builder, IEditorItem item, object model)
    {
        var fieldType = item.PropertyType;
        var fieldName = item.GetFieldName();
        var displayName = item.GetDisplayName() ?? BootstrapBlazor.Components.Utility.GetDisplayName(model, fieldName);
        var fieldValue = BootstrapBlazor.Components.Utility.GetPropertyValue(model, fieldName);
        var type = (Nullable.GetUnderlyingType(fieldType) ?? fieldType);
        if (type == typeof(bool) || fieldValue?.GetType() == typeof(bool))
        {
            builder.OpenComponent<Switch>(0);
            builder.AddAttribute(10, nameof(Switch.Value), fieldValue);
            builder.AddAttribute(20, nameof(Switch.IsDisabled), true);
            builder.AddAttribute(30, nameof(Switch.DisplayText), displayName);
            builder.AddAttribute(40, nameof(Switch.ShowLabelTooltip), item.ShowLabelTooltip);
            if (item is ITableColumn col)
            {
                builder.AddAttribute(50, "class", col.CssClass);
            }
            builder.AddMultipleAttributes(60, item.ComponentParameters);
            builder.CloseComponent();
        }
        else if (item.ComponentType == typeof(Textarea))
        {
            builder.OpenComponent(0, typeof(Textarea));
            builder.AddAttribute(10, nameof(Textarea.DisplayText), displayName);
            builder.AddAttribute(20, nameof(Textarea.Value), fieldValue);
            builder.AddAttribute(30, nameof(Textarea.ShowLabelTooltip), item.ShowLabelTooltip);
            builder.AddAttribute(40, "readonly", true);
            if (item.Rows > 0)
            {
                builder.AddAttribute(50, "rows", item.Rows);
            }
            if (item is ITableColumn col)
            {
                builder.AddAttribute(60, "class", col.CssClass);
            }
            builder.AddMultipleAttributes(70, item.ComponentParameters);
            builder.CloseComponent();
        }
        else
        {
            builder.OpenComponent(0, typeof(Display<>).MakeGenericType(fieldType));
            builder.AddAttribute(10, nameof(Display<string>.DisplayText), displayName);
            builder.AddAttribute(20, nameof(Display<string>.Value), fieldValue);
            builder.AddAttribute(30, nameof(Display<string>.LookupServiceKey), item.LookupServiceKey);
            builder.AddAttribute(40, nameof(Display<string>.LookupServiceData), item.LookupServiceData);
            builder.AddAttribute(50, nameof(Display<string>.Lookup), item.Lookup);
            builder.AddAttribute(60, nameof(Display<string>.ShowLabelTooltip), item.ShowLabelTooltip);
            builder.AddAttribute(70, nameof(Display<string>.ShowTooltip), true);

            builder.AddMultipleAttributes(100, item.ComponentParameters);
            builder.CloseComponent();
        }
    }
}