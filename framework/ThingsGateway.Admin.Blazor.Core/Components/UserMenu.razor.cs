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

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// UserMenu
/// </summary>
public partial class UserMenu
{
    [Inject]
    NavigationManager NavigationManager { get; set; }
    [Inject]
    private UserResoures UserResoures { get; set; }

    [Inject]
    private AjaxService AjaxService { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    private async Task LogoutAsync()
    {
        var ajaxOption = new AjaxOption
        {
            Url = "/auth/b/logout",
        };
        var str = await AjaxService.GetMessageAsync(ajaxOption);
        var ret = str?.ToJsonWithT<UnifyResult<string>>();
        if (ret?.Code != 200)
        {
            await PopupService.EnqueueSnackbarAsync("ע��ʧ��", AlertTypes.Error);
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync("ע���ɹ�", AlertTypes.Success);
            await Task.Delay(500);
            NavigationManager.NavigateTo(NavigationManager.Uri);
        }
    }
}