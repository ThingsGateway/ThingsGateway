using BlazorComponent.I18n;

using NewLife;

namespace ThingsGateway.Web.Rcl.Core
{
    public class CultureComponentBase : BaseComponentBase
    {
        [CascadingParameter]
        public CultureInfo Culture { get; set; }

        [Inject]
        public I18n LanguageService { get; set; }

        public string ScopeT(string scope, string key, params object[] args)
        {
            return string.Format(LanguageService.T(scope, key, true), args);
        }

        public string T(string key, params object[] args)
        {
            if(key.IsNullOrEmpty())
            {
                return "";
            }
            return string.Format(LanguageService.T(key, false, key), args);
        }
    }
}