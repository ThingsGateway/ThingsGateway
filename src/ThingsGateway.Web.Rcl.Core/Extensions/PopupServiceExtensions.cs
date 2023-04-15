
using Masa.Blazor;

namespace ThingsGateway.Web.Rcl.Core
{
    /// <summary>
    /// 扩展方法,部分代码来源为开源代码收集等
    /// </summary>
    public static class PopupServiceExtensions
    {

        public static async Task<bool> OpenConfirmDialogAsync(this IPopupService PopupService, string title, string content)
        {
            return await PopupService.ConfirmAsync(title, content, AlertTypes.Error);
        }

        public static async Task<bool> OpenConfirmDialogAsync(this IPopupService PopupService, string title, string content, AlertTypes type)
        {
            return await PopupService.ConfirmAsync(title, content, type);
        }

        public static async Task OpenInformationMessageAsync(this IPopupService PopupService, string message)
        {
            await PopupService.EnqueueSnackbarAsync(message, AlertTypes.Info);
        }


    }
}