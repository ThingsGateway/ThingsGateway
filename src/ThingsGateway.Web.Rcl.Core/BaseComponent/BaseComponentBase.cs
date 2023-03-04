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
            await Dispose(true);
        }

        protected virtual Task Dispose(bool disposing)
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