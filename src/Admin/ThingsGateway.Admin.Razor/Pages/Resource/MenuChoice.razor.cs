//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Razor;

public partial class MenuChoice
{
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }

    [EditorRequired]
    [Parameter]
    [NotNull]
    public string? DisplayText { get; set; }

    [Parameter]
    [NotNull]
    public IEnumerable<SelectedItem>? Items { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public long ModuleId { get; set; }

    public long TreeValue { get; set; }

    [Parameter]
    [NotNull]
    public long Value { get; set; }
    [Parameter]
    [NotNull]
    public EventCallback<long> ValueChanged { get; set; }

    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    private Task OnClearText() => OnValueChanged(0, true);

    private async Task OnSelect()
    {
        var option = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = AdminLocalizer["Choice"],
            ShowMaximizeButton = true,
            Class = "dialog-table",
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    await OnValueChanged(TreeValue, true);
                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<MenuChoiceDialog>(new Dictionary<string, object?>
            {
                [nameof(MenuChoiceDialog.ModuleId)] = ModuleId,
                [nameof(MenuChoiceDialog.Value)] = Value,

                [nameof(MenuChoiceDialog.ValueChanged)] = EventCallback.Factory.Create<long>(this, v => OnValueChanged(v))
            }).Render(),

        };
        await DialogService.Show(option);
    }

    private async Task OnValueChanged(long v, bool change = false)
    {
        if (TreeValue != v)
        {
            TreeValue = v;
        }
        if (change && ValueChanged.HasDelegate)
        {
            Value = TreeValue;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
