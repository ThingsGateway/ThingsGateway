//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor
{
    public partial class GrantApi
    {
        [Parameter]
        public long Id { get; set; }

        [Parameter]
        public bool IsRole { get; set; }

        private string _searchName { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await ApiInitAsync();
            await base.OnInitializedAsync();
        }

        private List<OpenApiPermissionTreeSelector> _apiTreeSelectors = new();
        private List<OpenApiPermissionTreeSelector> _hasApis = new();

        private async Task ApiInitAsync()
        {
            _apiTreeSelectors = (_serviceScope.ServiceProvider.GetService<IResourceService>().ApiPermissionTreeSelector());
            if (IsRole)
            {
                var data = (await _serviceScope.ServiceProvider.GetService<IRoleService>().ApiOwnPermissionAsync(Id.ToInput()))?.GrantInfoList;
                _hasApis = _apiTreeSelectors.SelectMany(a => a.Children).Where(a => data.Select(b => b.ApiUrl).Contains(a.ApiRoute)).ToList();
            }
            else
            {
                var data = (await _serviceScope.ServiceProvider.GetService<ISysUserService>().ApiOwnPermissionAsync(Id.ToInput()))?.GrantInfoList;
                _hasApis = _apiTreeSelectors.SelectMany(a => a.Children).Where(a => data.Select(b => b.ApiUrl).Contains(a.ApiRoute)).ToList();
            }
        }

        private async Task OnRoleHasApisSaveAsync(ModalActionEventArgs args)
        {
            if (IsRole)
            {
                try
                {
                    GrantPermissionInput userGrantPermissionInput = new();
                    var data = new List<SysResource>();
                    userGrantPermissionInput.Id = Id;
                    userGrantPermissionInput.GrantInfoList = _hasApis.Select(it => new RelationRolePermission() { ApiUrl = it.ApiRoute }).ToList();
                    await _serviceScope.ServiceProvider.GetService<IRoleService>().ApiGrantPermissionAsync(userGrantPermissionInput);
                    await ClosePopupAsync(true);
                }
                catch (Exception ex)
                {
                    args.Cancel();
                    await PopupService.EnqueueSnackbarAsync(ex, false);
                }
            }
            else
            {
                try
                {
                    GrantPermissionInput userGrantPermissionInput = new();
                    var data = new List<SysResource>();
                    userGrantPermissionInput.Id = Id;
                    userGrantPermissionInput.GrantInfoList = _hasApis.Select(it => new RelationRolePermission() { ApiUrl = it.ApiRoute }).ToList();
                    await _serviceScope.ServiceProvider.GetService<ISysUserService>().ApiGrantPermissionAsync(userGrantPermissionInput);
                    await ClosePopupAsync(true);
                }
                catch (Exception ex)
                {
                    args.Cancel();
                    await PopupService.EnqueueSnackbarAsync(ex, false);
                }
            }
        }
    }
}