//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class SysResourceEdit
{
    [Parameter]
    [NotNull]
    public SysResource? Model { get; set; }

    [Parameter]
    [NotNull]
    public IEnumerable<SelectedItem>? ParementMenus { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public long ModuleId { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<MenuIconList> Localizer { get; set; }

    [Inject]
    [NotNull]
    private DialogService DialogService { get; set; }

    private Task OnToggleIconDialog() => DialogService.Show(new DialogOption()
    {
        Title = Localizer["ChoiceIcon"],
        ShowFooter = false,
        Component = BootstrapDynamicComponent.CreateComponent<MenuIconList>(new Dictionary<string, object?>()
        {
            [nameof(MenuIconList.Value)] = Model.Icon,
            [nameof(MenuIconList.ValueChanged)] = EventCallback.Factory.Create<string?>(this, v => Model.Icon = v!)
        })
    });
}
