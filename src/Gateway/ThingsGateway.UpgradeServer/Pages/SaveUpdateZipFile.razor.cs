//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

using TouchSocket.Core;

namespace ThingsGateway.Upgrade;

public partial class SaveUpdateZipFile
{
    [Inject]
    private IStringLocalizer<SaveUpdateZipFile> Localizer { get; set; }
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Razor._Imports>? RazorLocalizer { get; set; }


    [Parameter]
    [NotNull]
    public Func<UpdateZipFileAddInput, Task>? OnSaveUpdateZipFile { get; set; }

    private UpdateZipFileAddInput Model { get; set; } = new();

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }
    [Inject]
    private ToastService ToastService { get; set; }
    private async Task OnSave(EditContext editContext)
    {
        try
        {
            if (OnSaveUpdateZipFile != null)
                await OnSaveUpdateZipFile.Invoke(Model).ConfigureAwait(false);
            if (OnCloseAsync != null)
                await OnCloseAsync().ConfigureAwait(false);
            await ToastService.Default().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message).ConfigureAwait(false);
        }
    }

    [Inject]
    private DownloadService DownloadService { get; set; }
    private async Task DownTemplate()
    {
        await DownloadService.DownloadFromFileAsync("UpdateZipFileTemplate.json", "UpdateZipFileTemplate.json").ConfigureAwait(false);
    }
}
