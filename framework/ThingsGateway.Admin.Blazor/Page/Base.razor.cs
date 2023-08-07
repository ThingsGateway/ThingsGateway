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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor
{
    /// <summary>
    /// Base
    /// </summary>
    public partial class Base
    {

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    var data = await App.GetService<UserCenterService>().GetLoginDefaultRazorAsync(UserManager.UserId);
                    var sameLevelMenus = await App.GetService<IResourceService>().GetaMenuAndSpaListAsync();
                    if (NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath == "/Login" || NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath == "/")
                        NavigationManager.NavigateTo(sameLevelMenus.FirstOrDefault(a => a.Id == data)?.Component ?? "index");
                    else
                        NavigationManager.NavigateTo(NavigationManager.Uri);
                }
                catch
                {
                    NavigationManager.NavigateTo("index");
                }
            }

            await base.OnAfterRenderAsync(firstRender);

        }

    }
}