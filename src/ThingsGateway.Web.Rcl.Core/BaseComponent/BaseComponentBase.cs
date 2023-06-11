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

namespace ThingsGateway.Web.Rcl.Core
{
    public class BaseComponentBase : ComponentBase, IAsyncDisposable
    {
        private bool IsDisposed;

        [Inject]
        public IPopupService PopupService
        {
            get;
            set;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
        }

        protected virtual Task DisposeAsync(bool disposing)
        {
            if (IsDisposed) return Task.CompletedTask;
            IsDisposed = true;
            return Task.CompletedTask;
        }

        protected virtual Task InvokeStateHasChangedAsync()
        {
            if (!IsDisposed)
            {
                return InvokeAsync(StateHasChanged);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}