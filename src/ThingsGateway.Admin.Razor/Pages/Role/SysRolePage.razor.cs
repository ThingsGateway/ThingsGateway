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

public partial class SysRolePage
{
    private SysRole? SearchModel { get; set; } = new();

    [Inject]
    [NotNull]
    private ISysRoleService? SysRoleService { get; set; }

    #region 查询

    private async Task<QueryData<SysRole>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SysRoleService.PageAsync(options);
        return data;
    }

    #endregion 查询

    #region 修改

    private async Task<bool> Delete(IEnumerable<SysRole> sysRoles)
    {
        try
        {
            return await SysRoleService.DeleteRoleAsync(sysRoles.Select(a => a.Id));
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> Save(SysRole sysRole, ItemChangedType itemChangedType)
    {
        try
        {
            return await SysRoleService.SaveRoleAsync(sysRole, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    #endregion 修改

    #region 授权

    private async Task GrantApi(long id)
    {
        var hasResources = (await SysRoleService.ApiOwnPermissionAsync(id))?.GrantInfoList;
        var ids = new List<string>();
        ids.AddRange(hasResources.Select(a => a.ApiUrl));
        var op = new DialogOption()
        {
            IsScrolling = false,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["RoleGrantApiPermission"],
            ShowCloseButton = false,
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantApiComponent>(new Dictionary<string, object?>
            {
                [nameof(GrantApiComponent.Value)] = ids,
                [nameof(GrantApiComponent.Id)] = id,
                [nameof(GrantApiComponent.IsRole)] = true,
            }).Render(),
        };
        await DialogService.Show(op);
    }

    private async Task GrantResource(long id)
    {
        var ids = (await SysRoleService.OwnResourceAsync(id))?.GrantInfoList;
        var op = new DialogOption()
        {
            IsScrolling = false,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["RoleGrantResource"],
            ShowCloseButton = false,
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantResourceComponent>(new Dictionary<string, object?>
            {
                [nameof(GrantResourceComponent.Value)] = ids.ToList(),
                [nameof(GrantResourceComponent.Id)] = id,
                [nameof(GrantResourceComponent.IsRole)] = true,
            }).Render(),
        };
        await DialogService.Show(op);
    }

    private async Task GrantUser(long id)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            Title = OperDescLocalizer["RoleGrantUser"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<GrantUserComponent>(new Dictionary<string, object?>
        {
            [nameof(GrantUserComponent.RoleId)] = id,
        });
        await DialogService.Show(op);
    }

    #endregion 授权
}
