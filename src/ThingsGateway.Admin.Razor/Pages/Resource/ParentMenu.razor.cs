
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Microsoft.AspNetCore.Components.Web;

namespace ThingsGateway.Admin.Razor;

public partial class ParentMenu
{
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Razor._Imports>? DefaultLocalizer { get; set; }

    [Inject]
    [NotNull]
    public IStringLocalizer<ParentMenu>? Localizer { get; set; }

    [Parameter]
    [NotNull]
    public IEnumerable<SelectedItem>? Lookup { get; set; }

    [Parameter]
    [NotNull]
    public string? DisplayText { get; set; }

    [Parameter]
    [NotNull]
    public long Value { get; set; }

    public long TreeValue { get; set; }

    [Parameter]
    [NotNull]
    public EventCallback<long> ValueChanged { get; set; }

    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }

    private DialogOption? Option { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public long ModuleId { get; set; }

    private async Task OnSelectMenu()
    {
        Option = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = Localizer["ChoiceMenu"],
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<ParentMenuTree>(new Dictionary<string, object?>
            {
                [nameof(ParentMenuTree.ModuleId)] = ModuleId,
                [nameof(ParentMenuTree.Value)] = Value,
                [nameof(ParentMenuTree.ValueChanged)] = EventCallback.Factory.Create<long>(this, v => OnValueChanged(v))
            }).Render(),
            FooterTemplate =

            BootstrapDynamicComponent.CreateComponent<Button>(new Dictionary<string, object?>
            {
                [nameof(Button.Color)] = Color.Primary,
                [nameof(Button.Icon)] = "fa-solid fa-fw fa-check",
                [nameof(Button.Text)] = DefaultLocalizer["Save"].Value,
                [nameof(Button.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(this, async () =>
                {
                    await OnValueChanged(TreeValue, true);
                    if (Option != null)
                    {
                        await Option.CloseDialogAsync();
                    }
                }),
            }).Render()
        };
        await DialogService.Show(Option);
    }

    private Task OnClearText() => OnValueChanged(0, true);

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