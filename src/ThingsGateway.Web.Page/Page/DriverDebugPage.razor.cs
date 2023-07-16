#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Masa.Blazor;

namespace ThingsGateway.Web.Page
{
    public partial class DriverDebugPage
    {
        List<DeviceTree> _deviceGroups = new();
        private BootstrapDynamicComponent _importComponent;
        private DriverDebugUIBase _importRef;
        private RenderFragment _importRender;
        string _searchName;
        List<DriverPluginCategory> DriverPlugins;
        bool IsShowTreeView = true;
        PluginDebugUIInput _searchModel { get; set; } = new();
        [Inject]
        IDriverPluginService DriverPluginService { get; set; }

        [CascadingParameter]
        MainLayout MainLayout { get; set; }

        [Inject]
        ResourceService ResourceService { get; set; }

        [Inject]
        IUploadDeviceService UploadDeviceService { get; set; }

        protected override void OnInitialized()
        {
            DriverPlugins = DriverPluginService.GetDriverPluginChildrenList();
            base.OnInitialized();
        }

        async Task ImportVaiable(long driverId)
        {
            var driver = ServiceExtensions.GetBackgroundService<CollectDeviceWorker>().GetDebugUI(driverId);
            if (driver == null)
            {
                await PopupService.EnqueueSnackbarAsync("插件未实现调试页面", AlertTypes.Warning);
                return;
            }

            _importComponent = new BootstrapDynamicComponent(driver);
            _importRender = _importComponent.Render(a => _importRef = (DriverDebugUIBase)a);
        }
        class PluginDebugUIInput
        {
            public long PluginId { get; set; }
            public string PluginName { get; set; }
        }
    }
}