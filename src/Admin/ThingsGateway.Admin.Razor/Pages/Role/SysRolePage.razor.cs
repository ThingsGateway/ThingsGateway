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

public partial class SysRolePage
{
    private SysRole? SearchModel { get; set; } = new();
    private long OrgId { get; set; }

    [Inject]
    [NotNull]
    private ISysRoleService? SysRoleService { get; set; }

    [Inject]
    [NotNull]
    private ISysResourceService? SysResourceService { get; set; }

    [Inject]
    [NotNull]
    private ISysOrgService? SysOrgService { get; set; }


    #region 查询

    private async Task<QueryData<SysRole>> OnQueryAsync(QueryPageOptions options)
    {
        var orgIds = await SysOrgService.GetOrgChildIdsAsync(OrgId);//获取下级机构
        var data = await SysRoleService.PageAsync(options, a => a.WhereIF(OrgId != 0, b => orgIds.Contains(b.OrgId)));
        return data;
    }
    private Task TreeChangedAsync(long id)
    {
        OrgId = id;
        return table.QueryAsync();
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
            await ToastService.Warn(ex);
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
            await ToastService.Warn(ex);
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
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["RoleGrantApiPermission"],
            ShowCloseButton = false,
            ShowMaximizeButton = true,
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    GrantPermissionData data = new();
                    data.Id = id;
                    data.GrantInfoList = ids.Select(a => new RelationPermission() { ApiUrl = a });
                    await SysRoleService.GrantApiPermissionAsync(data);
                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            Class = "dialog-table",
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantApiDialog>(new Dictionary<string, object?>
            {
                [nameof(GrantApiDialog.Value)] = ids,
                [nameof(GrantApiDialog.ValueChanged)] = (List<string> v) => { ids = v; return Task.CompletedTask; },
            }).Render(),
        };


        await DialogService.Show(op);
    }

    private async Task GrantResource(long id)
    {
        var grantInfoList = (await SysRoleService.OwnResourceAsync(id))?.GrantInfoList.ToList();

        var menuData = grantInfoList.Select(a => a.MenuId);
        var buttonData = grantInfoList.SelectMany(a => a.ButtonIds);
        var value = menuData.Concat(buttonData).ToList();

        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            Title = OperDescLocalizer["RoleGrantResource"],
            ShowCloseButton = false,
            ShowMaximizeButton = true,
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    GrantResourceData data = new();

                    var allResource = await SysResourceService.GetAllAsync();
                    var resources = allResource.Where(a => value.Contains(a.Id));
                    var pResources = SysResourceService.GetMyParentResources(allResource, resources);
                    var grantInfoList = new List<RelationResourcePermission>();
                    foreach (var item in pResources.Concat(resources).Distinct().Where(a => a.Category == ResourceCategoryEnum.Menu && !a.Href.IsNullOrEmpty()))
                    {
                        var relationResourcePermission = new RelationResourcePermission();
                        relationResourcePermission.MenuId = item.Id;
                        relationResourcePermission.ButtonIds = SysResourceService.GetResourceChilden(allResource, item.Id).Where(a => value.Contains(a.Id)).Select(a => a.Id).ToHashSet();
                        grantInfoList.Add(relationResourcePermission);
                    }

                    var buttons = resources.Where(a => a.Category == ResourceCategoryEnum.Button && a.ParentId == 0);
                    grantInfoList.Add(new RelationResourcePermission()
                    {
                        MenuId = 0,
                        ButtonIds = buttons.Select(a => a.Id).ToHashSet()
                    });

                    data.GrantInfoList = grantInfoList;
                    data.Id = id;
                    await SysRoleService.GrantResourceAsync(data);

                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }

            },
            Class = "dialog-table",
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<GrantResourceDialog>(new Dictionary<string, object?>
            {
                [nameof(GrantResourceDialog.Value)] = value,
                [nameof(GrantResourceDialog.ValueChanged)] = (List<long> v) => { value = v; return Task.CompletedTask; },
            }).Render(),
        };
        await DialogService.Show(op);
    }


    private async Task GrantUser(long id)
    {
        var data = (await SysRoleService.OwnUserAsync(id)).ToHashSet();
        GrantUserChoiceValues = data;
        var option = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraExtraLarge,
            Title = OperDescLocalizer["RoleGrantUser"],
            ShowMaximizeButton = true,
            Class = "dialog-table",
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    await OnGrantUserValueChanged(GrantUserChoiceValues, id, true);
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
                [nameof(UserChoiceDialog.Values)] = data,
                [nameof(UserChoiceDialog.ValuesChanged)] = (HashSet<long> v) => OnGrantUserValueChanged(v, id)
            }).Render(),

        };
        await DialogService.Show(option);

    }
    private HashSet<long> GrantUserChoiceValues = new();
    private async Task OnGrantUserValueChanged(HashSet<long> values, long roleId, bool change = false)
    {
        GrantUserChoiceValues = values;
        if (change)
        {
            GrantUserOrRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = roleId;
            userGrantRoleInput.GrantInfoList = values;
            await SysRoleService.GrantUserAsync(userGrantRoleInput);
        }
    }

    #endregion 授权
}
