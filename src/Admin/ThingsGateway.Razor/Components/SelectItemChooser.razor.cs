//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Razor;

/// <inheritdoc/>
public partial class SelectItemChooser
{
    private long _selectedItem;

    /// <inheritdoc/>
    [Parameter]
    [NotNull]
    [EditorRequired]
    public IEnumerable<long> Items { get; set; }

    /// <inheritdoc/>
    [Parameter]
    [EditorRequired]
    [NotNull]
    public RenderFragment<long>? ItemTemplate { get; set; }

    /// <inheritdoc/>
    [Parameter]
    [NotNull]
    public long Value { get; set; }

    /// <inheritdoc/>
    [Parameter]
    [NotNull]
    public EventCallback<long> ValueChanged { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        _selectedItem = Value;
        await base.OnParametersSetAsync();
    }

    private string? GetItemClass(long item) => CssBuilder.Default("btn m-2")
                .AddClass("btn-primary", _selectedItem == item)
    .Build();

    private async Task OnClickItem(long item)
    {
        if (_selectedItem != item && ValueChanged.HasDelegate)
        {
            Value = item;
            await ValueChanged.InvokeAsync(Value);
        }
        _selectedItem = item;
        Value = item;
    }
}
