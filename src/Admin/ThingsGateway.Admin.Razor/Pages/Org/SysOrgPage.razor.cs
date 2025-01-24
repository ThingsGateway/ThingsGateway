//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class SysOrgPage
{
    private long? ParentId { get; set; }

    [Inject]
    [NotNull]
    private ISysOrgService? SysOrgService { get; set; }

    #region 查询

    private async Task<QueryData<SysOrg>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SysOrgService.PageAsync(options,
            a => a
            .WhereIF(ParentId != null && ParentId != 0, b => b.ParentId == ParentId || b.Id == ParentId || SqlFunc.JsonLike(b.ParentIdList, ParentId.ToString()))

            );
        return data;
    }

    private Task TreeChangedAsync(long parentId)
    {
        ParentId = parentId;
        return table.QueryAsync();
    }

    #endregion 查询

    #region 修改
    private List<SysOrg> SelectedRows { get; set; } = new();
    private SysOrgCopyInput SysOrgCopyInput = new();
    private async Task OnCopy()
    {
        SysOrgCopyInput = new();
        var option = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.Medium,
            Title = AdminLocalizer["Copy"],
            ShowMaximizeButton = true,
            Class = "dialog-select",
            ShowSaveButton = true,
            OnSaveAsync = async () =>
            {
                try
                {
                    await SysOrgService.CopyAsync(SysOrgCopyInput);
                    await table.QueryAsync();
                    await ToastService.Default();
                    return true;
                }
                catch (Exception ex)
                {
                    await ToastService.Warn(ex);
                    return false;
                }
            },
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<SysOrgCopy>(new Dictionary<string, object?>
            {
                [nameof(SysOrgCopy.SysOrgCopyInput)] = SysOrgCopyInput,
            }).Render(),

        };
        SysOrgCopyInput.Ids = SelectedRows.Select(a => a.Id).ToList();
        await DialogService.Show(option);

    }
    private async Task<bool> Delete(IEnumerable<SysOrg> sysOrgs)
    {
        try
        {
            return await SysOrgService.DeleteOrgAsync(sysOrgs.Select(a => a.Id));
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    private async Task<bool> Save(SysOrg sysOrg, ItemChangedType itemChangedType)
    {
        try
        {
            return await SysOrgService.SaveOrgAsync(sysOrg, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion 修改

}
