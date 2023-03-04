using BlazorComponent.I18n;

using System.Globalization;

namespace ThingsGateway.Web.Rcl
{
    public partial class MainLayout
    {
        private bool _drawerOpen = false;

        [Inject]
        public I18n I18n { get; set; }

        [Inject]
        private UserResoures UserResoures { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await UserResoures.InitAllAsync();
            Navs = UserResoures.Menus.Parse();
            await base.OnInitializedAsync();
        }
        public async Task MenuChange()
        {
            await UserResoures.InitMenuAsync();
            Navs = UserResoures.Menus.Parse();
            StateHasChanged();
        }
        public async Task UserChange()
        {
            await UserResoures.InitUserAsync();
            StateHasChanged();
        }
        private void LanguageChange(string name)
        {
            I18n.SetCulture(new CultureInfo(name));
        }
    }
}