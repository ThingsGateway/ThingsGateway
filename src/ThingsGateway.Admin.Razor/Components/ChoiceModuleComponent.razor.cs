
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class ChoiceModuleComponent
{
    [Parameter]
    [EditorRequired]
    [NotNull]
    public long Value { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<SysResource> ModuleList { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Func<long, Task> OnSave { get; set; }

    private async Task Save()
    {
        if (OnSave != null)
            await OnSave.Invoke(Value);
    }

    private IEnumerable<long> SelectedItems { get; set; }

    [Inject]
    private ISysResourceService SysResourceService { get; set; }

    protected override void OnParametersSet()
    {
        SelectedItems = ModuleList.Select(a => a.Id);
        base.OnParametersSet();
    }
}