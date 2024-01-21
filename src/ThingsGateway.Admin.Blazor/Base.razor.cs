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

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor
{
    /// <summary>
    /// Base
    /// </summary>
    public partial class Base
    {
        protected override void OnInitialized()
        {
            _serviceScope = _serviceScopeFactory.CreateScope();
        }

        [Inject]
        private IServiceScopeFactory _serviceScopeFactory { get; set; }

        protected IServiceScope _serviceScope { get; set; }

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
                    var data = await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginWorkbenchAsync(UserManager.UserId);
                    var sameLevelMenus = await _serviceScope.ServiceProvider.GetService<IResourceService>().GetMenuAndSpaListAsync();
                    if (NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath == "/Login" || NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath == "/")
                        NavigationManager.NavigateTo(sameLevelMenus.FirstOrDefault(a => a.Id == data.DefaultRazpor)?.Href ?? "index", true);
                    else
                        NavigationManager.NavigateTo(NavigationManager.Uri, true);
                }
                catch
                {
                    NavigationManager.NavigateTo("index", true);
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}