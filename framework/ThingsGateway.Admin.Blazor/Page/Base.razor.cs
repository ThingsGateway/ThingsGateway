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