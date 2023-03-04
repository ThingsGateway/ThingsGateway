namespace ThingsGateway.Web.Rcl
{
    public partial class Config
    {
        private List<DevConfig> _alarmConfig = new();
        private List<DevConfig> _hisConfig = new();
        protected override async Task OnInitializedAsync()
        {
            _alarmConfig = await ConfigService.GetListByCategory(ThingsGatewayConst.ThingGateway_AlarmConfig_Base);
            _hisConfig = await ConfigService.GetListByCategory(ThingsGatewayConst.ThingGateway_HisConfig_Base);
            await base.OnInitializedAsync();
        }


    }
}