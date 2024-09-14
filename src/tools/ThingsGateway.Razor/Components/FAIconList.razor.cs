// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.JSInterop;

namespace ThingsGateway.Razor;

/// <summary>
/// FAIconList 组件
/// </summary>
[JSModuleAutoLoader("Components/FAIconList.razor.js", JSObjectReference = true)]
public partial class FAIconList : IAsyncDisposable
{
    private string? ClassString => CssBuilder.Default("icon-list")
        .AddClass("is-catalog", ShowCatalog)
        .AddClass("is-dialog", ShowCopyDialog)
        .AddClassFromAttributes(AdditionalAttributes)
        .Build();

    /// <summary>
    /// 获得/设置 点击时是否显示高级拷贝弹窗 默认 false 直接拷贝到粘贴板
    /// </summary>
    [Parameter]
    public bool ShowCopyDialog { get; set; }

    /// <summary>
    /// 获得/设置 是否显示目录 默认 false
    /// </summary>
    [Parameter]
    public bool ShowCatalog { get; set; }

    /// <summary>
    /// 获得/设置 高级弹窗 Header 显示文字
    /// </summary>
    [Parameter]
    [NotNull]
    public string? DialogHeaderText { get; set; }

    /// <summary>
    /// 获得/设置 当前选择图标
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// 获得/设置 当前选择图标回调方法
    /// </summary>
    [Parameter]
    public EventCallback<string?> IconChanged { get; set; }

    /// <summary>
    /// 获得/设置 拷贝成功提示文字
    /// </summary>
    [Parameter]
    public string? CopiedTooltipText { get; set; }

    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<IconDialog>? Localizer { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        DialogHeaderText ??= Localizer[nameof(DialogHeaderText)];
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override Task InvokeInitAsync() => InvokeVoidAsync("init", Id, Interop, nameof(UpdateIcon), nameof(ShowDialog));

    /// <summary>
    /// UpdateIcon 方法由 JS Invoke 调用
    /// </summary>
    /// <param name="icon"></param>
    [JSInvokable]
    public async Task UpdateIcon(string icon)
    {
        Icon = icon;
        if (IconChanged.HasDelegate)
        {
            await IconChanged.InvokeAsync(Icon);
        }
        else
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// ShowDialog 方法由 JS Invoke 调用
    /// </summary>
    /// <returns></returns>
    [JSInvokable]
    public Task ShowDialog(string text) => DialogService.ShowCloseDialog<IconDialog>(DialogHeaderText, parameters =>
    {
        parameters.Add(nameof(IconDialog.IconName), text);
    });
}
