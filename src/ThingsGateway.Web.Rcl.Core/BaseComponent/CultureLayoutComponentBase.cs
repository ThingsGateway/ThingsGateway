using BlazorComponent.I18n;

using Masa.Blazor;

namespace ThingsGateway.Web.Rcl.Core
{
    public class CultureLayoutComponentBase : LayoutComponentBase
    {
        [CascadingParameter]
        public CultureInfo Culture { get; set; }

        [Inject]
        public I18n LanguageService { get; set; }

        [Inject]
        public IPopupService PopupService { get; set; }

        public string ScopeT(string scope, string key, params object[] args)
        {
            return string.Format(LanguageService.T(scope, key, true), args);
        }

        public string T(string key, params object[] args)
        {
            return LanguageService.T(key, false, true, args);
        }

    }
}