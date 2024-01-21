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

using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor
{
    public partial class GrantResource
    {
        [Parameter]
        public long Id { get; set; }

        [Parameter]
        public bool IsRole { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await ResourceInitAsync();
            await base.OnInitializedAsync();
        }

        private ResTreeSelector _resTreeSelectors = new();
        private List<RelationRoleResource> _hasResources = new();

        private async Task ResourceInitAsync()
        {
            _resTreeSelectors = (await _serviceScope.ServiceProvider.GetService<IResourceService>().ResourceTreeSelectorAsync());
            if (IsRole)
            {
                _hasResources = (await _serviceScope.ServiceProvider.GetService<IRoleService>().OwnResourceAsync(Id.ToInput()))?.GrantInfoList;
            }
            else
            {
                _hasResources = (await _serviceScope.ServiceProvider.GetService<ISysUserService>().OwnResourceAsync(Id.ToInput()))?.GrantInfoList;
            }
        }

        private async Task OnRoleHasResourcesSaveAsync(ModalActionEventArgs args)
        {
            if (IsRole)
            {
                try
                {
                    GrantResourceInput userGrantRoleInput = new();
                    var data = new List<SysResource>();
                    userGrantRoleInput.Id = Id;
                    userGrantRoleInput.GrantInfoList = _hasResources;
                    await _serviceScope.ServiceProvider.GetService<IRoleService>().GrantResourceAsync(userGrantRoleInput);
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
                    UserGrantResourceInput userGrantRoleInput = new();
                    var data = new List<SysResource>();
                    userGrantRoleInput.Id = Id;
                    userGrantRoleInput.GrantInfoList = _hasResources;
                    await _serviceScope.ServiceProvider.GetService<ISysUserService>().GrantResourceAsync(userGrantRoleInput);
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