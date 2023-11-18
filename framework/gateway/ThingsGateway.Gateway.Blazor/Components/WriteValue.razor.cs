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

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Gateway.Blazor
{
    public partial class WriteValue
    {
        [Parameter, EditorRequired]
        public string Content { get; set; }

        [Inject]
        IPopupService PopupService { get; set; }

        [Parameter]
        public EventCallback<string> OnSaveAsync { get; set; }

        private async Task ValueChanged()
        {
            try
            {
                await OnSaveAsync.InvokeAsync(Content);
                //await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync("�ɹ�", AlertTypes.Success));
                await ClosePopupAsync();
            }
            catch (Exception ex)
            {
                await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync(ex));
            }
        }
    }
}