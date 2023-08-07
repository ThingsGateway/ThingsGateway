#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Application;

namespace ThingsGateway.Blazor;
/// <summary>
/// ����excel
/// </summary>
public partial class ImportExcel
{
    IBrowserFile _importFile;

    Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

    bool isImport;

    bool isSaveImport;
    /// <summary>
    /// ����
    /// </summary>
    [Parameter]
    public Func<Dictionary<string, ImportPreviewOutputBase>, Task> Import { get; set; }
    /// <summary>
    /// �Ƿ���ʾ
    /// </summary>
    public bool IsShowImport { get; set; }
    /// <summary>
    /// Ԥ��
    /// </summary>
    [Parameter]
    public Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> Preview { get; set; }
    /// <summary>
    /// ��ǰ����
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
            await PopupService.EnqueueSnackbarAsync("�ɹ�", AlertTypes.Success);
        }
        finally
        {
            isSaveImport = false;
        }
    }
}