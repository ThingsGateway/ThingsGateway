//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Masa.Blazor;

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 导入excel
/// </summary>
public partial class ImportExcel
{
    private IBrowserFile _importFile;

    private Dictionary<string, ImportPreviewOutputBase> _importPreviews = new();

    private bool _isImport;

    private bool _isSaveImport;

    /// <summary>
    /// 当前步数
    /// </summary>
    private int _step = 1;

    /// <summary>
    /// 导入
    /// </summary>
    [Parameter]
    public EventCallback<Dictionary<string, ImportPreviewOutputBase>> Import { get; set; }

    /// <summary>
    /// 预览
    /// </summary>
    [Parameter]
    public Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> Preview { get; set; }

    private async Task DeviceImport(IBrowserFile file)
    {
        try
        {
            _isImport = true;
            StateHasChanged();
            _importPreviews.Clear();
            _importPreviews = await Preview.Invoke(file);
            _step = 2;
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        finally
        {
            _isImport = false;
        }
    }

    private async Task SaveDeviceImport()
    {
        try
        {
            _isSaveImport = true;
            StateHasChanged();
            await Import.InvokeAsync(_importPreviews);
            _importFile = null;
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
            await ClosePopupAsync(true);
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        finally
        {
            _isSaveImport = false;
        }
    }
}