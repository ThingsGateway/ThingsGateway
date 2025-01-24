//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class UserChoice
{
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }

    [Inject]
    [NotNull]
    public ISysUserService SysUserService { get; set; }

    [EditorRequired]
    [Parameter]
    [NotNull]
    public string? DisplayText { get; set; }

    public HashSet<long> ChoiceValues { get; set; }

    [Parameter]
    [NotNull]
    public HashSet<long> Values { get; set; } = new();

    private long SelectedArrayValue { get; set; }

    [Parameter]
    [NotNull]
    public EventCallback<HashSet<long>> ValuesChanged { get; set; }

    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    public IEnumerable<SelectedItem> Items { get; set; }
    private Task OnClearText() => OnValueChanged(new(), true);

    private async Task OnSelect()
    {
        var option = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraExtraLarge,
            Title = AdminLocalizer["Choice"],
            ShowMaximizeButton = true,
            Class = "dialog-table",
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    await OnValueChanged(ChoiceValues, true);
                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<UserChoiceDialog>(new Dictionary<string, object?>
            {
                [nameof(UserChoiceDialog.MaxCount)] = 1,
                [nameof(UserChoiceDialog.Values)] = Values,
                [nameof(UserChoiceDialog.ValuesChanged)] = (HashSet<long> v) => OnValueChanged(v)
            }).Render(),

        };
        await DialogService.Show(option);
    }

    private async Task OnValueChanged(HashSet<long> values, bool change = false)
    {
        if (ChoiceValues != values)
        {
            ChoiceValues = values;
        }
        if (change && ValuesChanged.HasDelegate)
        {
            Values = ChoiceValues;
            Items = UserUtil.BuildUserSelectList(await SysUserService.GetUserListByIdListAsync(Values));
            SelectedArrayValue = Values.FirstOrDefault();
            await ValuesChanged.InvokeAsync(Values);
        }
    }
}
