//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------


#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

using ThingsGateway.Management;

namespace ThingsGateway.Upgrade;

public partial class UpdateZipFilePage
{
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Management._Imports> ManagementLocalizer { get; set; }
    #region 查询
    [Inject]
    private ToastService ToastService { get; set; }
    [Inject]
    private DialogService DialogService { get; set; }
    [Inject]
    [NotNull]
    private IUpdateZipFileService? UpdateZipFileService { get; set; }

    private async Task<QueryData<UpdateZipFile>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            var data = await UpdateZipFileService.GetList();
            return new QueryData<UpdateZipFile>() { Items = data };
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
            return new QueryData<UpdateZipFile>() { };
        }

    }

    #endregion 查询

    private async Task ShowInfo(UpdateZipFile updateZipFile)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraExtraLarge,
            Title = ManagementLocalizer["Info"],
            ShowCloseButton = false,
            ShowMaximizeButton = true,
            ShowSaveButton = false,
            Class = "dialog-table",
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<UpdateZipFileInfo>(new Dictionary<string, object?>
            {
                [nameof(UpdateZipFileInfo.Logger)] = UpdateZipFileService.TextLogger,
                [nameof(UpdateZipFileInfo.LogPath)] = UpdateZipFileService.LogPath,
                [nameof(UpdateZipFileInfo.Model)] = updateZipFile,

            }).Render(),
        };

        await DialogService.Show(op);
    }


}
