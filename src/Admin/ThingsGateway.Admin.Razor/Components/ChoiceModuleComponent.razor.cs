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
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Admin.Razor;

public partial class ChoiceModuleComponent
{
    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<SysResource> ModuleList { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Func<long, Task> OnClick { get; set; }

    [Parameter]
    public long Value { get; set; }

    private List<RibbonTabItem> RibbonTabItems { get; set; }

    protected override void OnParametersSet()
    {
        RibbonTabItems = GenerateRibbonTabs();
        StateHasChanged();
        base.OnParametersSet();
    }

    private async Task OnMenuClickAsync(TabItem tabItem)
    {
        if (OnClick != null)
            await OnClick.Invoke(tabItem.Url.ToLong());
    }


    private List<RibbonTabItem> GenerateRibbonTabs()
    {
        var tabs = new List<RibbonTabItem>(ModuleList?.Count ?? 1);
        foreach (var item in ModuleList)
        {
            var tab = new RibbonTabItem() { IsActive = Value == item.Id, Id = item.Id.ToString(), Text = item.Title, Icon = item.Icon };
            tabs.Add(tab);
        }
        return tabs;
    }


}
