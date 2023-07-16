#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/dotnetchina/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
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
                await PopupService.EnqueueSnackbarAsync(T("�ɹ�"), AlertTypes.Success);
            }
            finally
            {
                isSaveImport = false;
            }
        }
    }
}