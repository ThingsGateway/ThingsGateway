//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Web;

using ThingsGateway.NewLife.X.Extension;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Razor;

public partial class GrantRoleComponent
{
    [Parameter]
    public List<SysRole> Items { get; set; } = new();

    [Parameter]
    public long UserId { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    private SysRole? SearchModel { get; set; } = new();
    private List<SysRole> SelectedAddRows { get; set; } = new();
    private List<SysRole> SelectedDeleteRows { get; set; } = new();
    private HashSet<SysRole> SelectedRows { get; set; } = new();

    [Inject]
    [NotNull]
    private ISysRoleService? SysRoleService { get; set; }

    [Inject]
    [NotNull]
    private ISysUserService? SysUserService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Items = await SysRoleService.GetAllAsync();
        var data = await SysUserService.OwnRoleAsync(UserId);
        var roles = await SysRoleService.GetRoleListByIdListAsync(data);
        SelectedRows = roles.Where(a => data.Contains(a.Id)).ToHashSet();
        await base.OnInitializedAsync();
    }

    private async Task<QueryData<SysRole>> OnQueryAsync(QueryPageOptions options)
    {
        await Task.Delay(100);
        var items = Items.WhereIF(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText)).GetQueryData(options);
        return items;
    }

    private async Task OnSave(MouseEventArgs args)
    {
        try
        {
            GrantUserOrRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = UserId;
            userGrantRoleInput.GrantInfoList = SelectedRows.Select(it => it.Id);
            await SysUserService.GrantRoleAsync(userGrantRoleInput);
            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }
}
