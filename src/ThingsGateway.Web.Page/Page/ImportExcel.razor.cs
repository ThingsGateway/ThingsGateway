#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Masa.Blazor;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using System;

namespace ThingsGateway.Web.Page
{
    public partial class ImportExcel
    {
        IBrowserFile _importFile;

        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

        bool isImport;

        bool isSaveImport;

        [Parameter]
        public Func<Dictionary<string, ImportPreviewOutputBase>, Task> Import { get; set; }

        public bool IsShowImport { get; set; }

        [Parameter]
        public Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> Preview { get; set; }

        [Parameter]
        public int Step { get; set; }

        [Inject]
        IJSRuntime JS { get; set; }
        async Task DeviceImport(IBrowserFile file)
        {
            try
            {
                isImport = true;
                StateHasChanged();
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
                await PopupService.EnqueueSnackbarAsync(T("成功"), AlertTypes.Success);
            }
            finally
            {
                isSaveImport = false;
            }
        }
    }
}