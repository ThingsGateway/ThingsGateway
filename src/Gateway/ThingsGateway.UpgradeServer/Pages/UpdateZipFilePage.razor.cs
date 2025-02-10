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

namespace ThingsGateway.Upgrade;

public partial class UpdateZipFilePage
{
    #region 查询

    [Inject]
    [NotNull]
    private IUpdateZipFileService? UpdateZipFileService { get; set; }

    private async Task<QueryData<UpdateZipFile>> OnQueryAsync(QueryPageOptions options)
    {
        return await Task.Run(async () =>
        {
            var data = await UpdateZipFileService.Page(options);
            return data;
        });
    }

    #endregion 查询

    #region 添加

    [Inject]
    private IStringLocalizer<SaveUpdateZipFile> Loaclozer { get; set; }

    private async Task OnAdd(IEnumerable<UpdateZipFile> pluginOutputs)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = Loaclozer["SaveUpdateZipFile"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () =>
            {
                await InvokeAsync(table.QueryAsync);
            },
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<SaveUpdateZipFile>(new Dictionary<string, object?>
        {
            [nameof(SaveUpdateZipFile.OnSaveUpdateZipFile)] = new Func<UpdateZipFileAddInput, Task>(UpdateZipFileService.SaveUpdateZipFile),
        });
        await DialogService.Show(op);
    }

    #endregion 添加
    [Inject]
    private DialogService DialogService { get; set; }
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Razor._Imports>? RazorLocalizer { get; set; }

    private Task<bool> OnDeleteAsync(IEnumerable<UpdateZipFile> updateZipFiles)
    {
        return UpdateZipFileService.DeleteAsync(updateZipFiles);
    }
}
