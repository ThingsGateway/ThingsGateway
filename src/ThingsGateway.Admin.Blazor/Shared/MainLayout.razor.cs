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

namespace ThingsGateway.Admin.Blazor
{
    public partial class MainLayout : IMainLayout
    {
        private async Task ShowAbout()
        {
            await PopupService.OpenAsync(typeof(About), new Dictionary<string, object?>()
            {
            });
        }

        private bool Changed { get; set; }

        private bool? _drawerOpen = true;

        private PageTabs _pageTabs;

        private bool? _showSettings;

        [Inject] private BlazorAppService AppService { get; set; }
        [Inject] private IPopupService PopupService { get; set; }

        /// <summary>
        /// IsMobile
        /// </summary>
        [CascadingParameter(Name = "IsMobile")]
        public bool IsMobile { get; set; }

        private void HandleSettingsClick()
        {
            _showSettings = !_showSettings;
        }

        protected override async Task OnInitializedAsync()
        {
            await StateHasChangedAsync();
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// 页面刷新
        /// </summary>
        /// <returns></returns>
        public async Task StateHasChangedAsync()
        {
            await UserResoures.InitUserAsync();
            await UserResoures.InitMenuAsync();
            Changed = !Changed;
            await InvokeAsync(StateHasChanged);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }
    }
}