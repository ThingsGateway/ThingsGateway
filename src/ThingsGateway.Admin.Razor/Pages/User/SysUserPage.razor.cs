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

public partial class SysUserPage
{
    private SysUser? SearchModel { get; set; } = new();

    [Inject]
    [NotNull]
    private ISysUserService? SysUserService { get; set; }

    #region 查询

    private async Task<QueryData<SysUser>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SysUserService.PageAsync(options);
        return data;
    }

    #endregion 查询

    #region 修改

    private async Task<bool> Delete(IEnumerable<SysUser> sysUsers)
    {
        try
        {
            return await SysUserService.DeleteUserAsync(sysUsers.Select(a => a.Id));
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    private async Task GrantApi(long id)
    {
        var hasResources = (await SysUserService.ApiOwnPermissionAsync(id))?.GrantInfoList;
        var ids = new List<string>();
        ids.AddRange(hasResources.Select(a => a.ApiUrl));
        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["UserGrantApiPermission"],
            ShowCloseButton = false,
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantApiComponent>(new Dictionary<string, object?>
            {
                [nameof(GrantApiComponent.Value)] = ids,
                [nameof(GrantResourceComponent.Id)] = id,
            }).Render(),
        };
        await DialogService.Show(op);
    }

    private async Task GrantResource(long id)
    {
        var ids = (await SysUserService.OwnResourceAsync(id))?.GrantInfoList;
        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["UserGrantResource"],
            ShowCloseButton = false,
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantResourceComponent>(new Dictionary<string, object?>
            {
                [nameof(GrantResourceComponent.Value)] = ids.ToList(),
                [nameof(GrantResourceComponent.Id)] = id,
            }).Render(),
        };
        await DialogService.Show(op);
    }

    private async Task GrantRole(long id)
    {
        var op = new DialogOption()
        {
            Title = OperDescLocalizer["UserGrantRole"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<GrantRoleComponent>(new Dictionary<string, object?>
        {
            [nameof(GrantRoleComponent.UserId)] = id,
        });
        await DialogService.Show(op);
    }

    private async Task ResetPassword(long id)
    {
        try
        {
            await SysUserService.ResetPasswordAsync(id);
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
        }
    }

    private async Task<bool> Save(SysUser sysUser, ItemChangedType itemChangedType)
    {
        try
        {
            return await SysUserService.SaveUserAsync(sysUser, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    #endregion 修改
}
