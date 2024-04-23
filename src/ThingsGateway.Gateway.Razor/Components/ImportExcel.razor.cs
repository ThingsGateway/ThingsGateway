
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Microsoft.AspNetCore.Components.Forms;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public partial class ImportExcel
{
    [Required]
    private IBrowserFile _importFile { get; set; }

    /// <summary>
    /// 导入
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Func<Dictionary<string, ImportPreviewOutputBase>, Task> Import { get; set; }

    private Dictionary<string, ImportPreviewOutputBase> _importPreviews = new();

    /// <summary>
    /// 预览
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> Preview { get; set; }

    private async Task DeviceImport(IBrowserFile file)
    {
        try
        {
            _importPreviews.Clear();
            _importPreviews = await Preview.Invoke(file);
            await step.Next();
        }
        catch (Exception ex)
        {
            await ToastService.Error(null, ex.Message);
        }
    }

    private async Task SaveDeviceImport()
    {
        try
        {
            await Import.Invoke(_importPreviews);
            _importFile = null;
            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Error(null, ex.Message);
        }
    }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }
}
