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

using Masa.Blazor;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;
/// <summary>
/// ��������
/// </summary>
public partial class UserCenter
{
    List<long> _menusChoice = new();
    readonly PasswordInfoInput _passwordInfoInput = new();
    readonly UpdateInfoInput _updateInfoInput = new();

    long DefaultMenuId { get; set; }
    [CascadingParameter]
    MainLayout MainLayout { get; set; }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Inject]
    IUserCenterService UserCenterService { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        DefaultMenuId = await App.GetService<IUserCenterService>().GetLoginDefaultRazorAsync(UserManager.UserId);
        _updateInfoInput.Email = UserResoures.CurrentUser.Email;
        _updateInfoInput.Phone = UserResoures.CurrentUser.Phone;
        _menusChoice = await App.GetService<IUserCenterService>().GetLoginWorkbenchAsync();
        await base.OnParametersSetAsync();
    }

    async Task OnDefaultRazorSaveAsync()
    {
        await UserCenterService.UpdateUserDefaultRazorAsync(UserManager.UserId, DefaultMenuId);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("�ɹ�", AlertTypes.Success);
    }

    async Task OnShortcutSaveAsync()
    {
        await UserCenterService.UpdateWorkbenchAsync(_menusChoice);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("�ɹ�", AlertTypes.Success);
    }
    async Task OnUpdatePasswordInfoAsync(FormContext context)
    {
        var success = context.Validate();
        if (success)
        {
            //��֤�ɹ�������ҵ��
            _passwordInfoInput.Id = UserResoures.CurrentUser.Id;
            await UserCenterService.EditPasswordAsync(_passwordInfoInput);
            await MainLayout.StateHasChangedAsync();
            await PopupService.EnqueueSnackbarAsync("�ɹ��������µ�¼", AlertTypes.Success);
            await Task.Delay(2000);
            NavigationManager.NavigateTo(NavigationManager.Uri);
        }
    }

    async Task OnUpdateUserInfoAsync()
    {
        await UserCenterService.UpdateUserInfoAsync(_updateInfoInput);
        await MainLayout.StateHasChangedAsync();
        await PopupService.EnqueueSnackbarAsync("�ɹ�", AlertTypes.Success);
    }
}