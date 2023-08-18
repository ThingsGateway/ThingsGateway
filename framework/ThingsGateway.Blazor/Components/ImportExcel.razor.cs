#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Application;

namespace ThingsGateway.Blazor;
/// <summary>
/// 导入excel
/// </summary>
public partial class ImportExcel
{
    IBrowserFile _importFile;

    Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

    bool isImport;

    bool isSaveImport;
    /// <summary>
    /// 导入
    /// </summary>
    [Parameter]
    public Func<Dictionary<string, ImportPreviewOutputBase>, Task> Import { get; set; }
    /// <summary>
    /// 是否显示
    /// </summary>
    public bool IsShowImport { get; set; }
    /// <summary>
    /// 预览
    /// </summary>
    [Parameter]
    public Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> Preview { get; set; }
    /// <summary>
    /// 当前步数
    /// </summary>
    public int Step { get; set; }

    async Task DeviceImport(IBrowserFile file)
    {
        try
        {
            isImport = true;
            StateHasChanged();
            ImportPreviews.Clear();
            ImportPreviews = await Preview.Invoke(file);
            Step = 2;
        }
        finally
        {
            isImport = false;
        }
    }

    async Task SaveDeviceImport()
    {
        try
        {
            isSaveImport = true;
            StateHasChanged();
            await Import.Invoke(ImportPreviews);
            _importFile = null;
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
        finally
        {
            isSaveImport = false;
        }
    }
}