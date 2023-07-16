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

using Furion;

using Mapster;

using Masa.Blazor;

using SqlSugar;

using TouchSocket.Core;

namespace ThingsGateway.Web.Page
{
    public partial class DeviceVariableRunTimePage
    {
        private IAppDataTable _datatable;
        List<DeviceTree> _deviceGroups = new();
        string _searchName;
        private System.Timers.Timer DelayTimer;

        [Parameter]
        [SupplyParameterFromQuery]
        public string DeviceName { get; set; }
        [Parameter]
        [SupplyParameterFromQuery]
        public string UploadDeviceName { get; set; }
        VariablePageInput _searchModel { get; set; } = new();
        [Inject]
        IUploadDeviceService _uploadDeviceService { get; set; }

        CollectDeviceWorker CollectDeviceHostService { get; set; }
        [Inject]
        RpcSingletonService rpcCore { get; set; }

        protected override async Task DisposeAsync(bool disposing)
        {
            await base.DisposeAsync(disposing);
            DelayTimer?.SafeDispose();
        }

        protected override async Task OnInitializedAsync()
        {
            DelayTimer = new System.Timers.Timer(1000);
            DelayTimer.Elapsed += timer_Elapsed;
            DelayTimer.AutoReset = true;
            DelayTimer.Start();
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_searchModel.DeviceName != DeviceName && !DeviceName.IsNullOrEmpty())
            {
                _searchModel.DeviceName = DeviceName;
                await datatableQuery();
            }
            if (_searchModel.UploadDeviceName != UploadDeviceName && !UploadDeviceName.IsNullOrEmpty())
            {
                _searchModel.UploadDeviceName = UploadDeviceName;
                await datatableQuery();
            }

            CollectDeviceHostService = ServiceExtensions.GetBackgroundService<CollectDeviceWorker>();
            _deviceGroups = _globalDeviceData.CollectDevices.Adapt<List<CollectDevice>>().GetTree();
            await base.OnParametersSetAsync();
        }
        private async Task datatableQuery()
        {
            if (_datatable != null)
                await _datatable?.QueryClickAsync();
        }

        private void FilterHeaders(List<DataTableHeader<DeviceVariableRunTime>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.UpdateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.Id));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.VariablePropertys));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.DeviceId));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.VariableValueChange));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.ThingsGatewayBitConverter));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.DataType));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.EventTime));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.EventTypeEnum));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.CollectDeviceRunTime));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.LastSetValue));
            datas.RemoveWhere(it => it.Value.Contains("His"));
            datas.RemoveWhere(it => it.Value.Contains("Alarm"));
            datas.RemoveWhere(it => it.Value.Contains("RestrainExpressions"));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(DeviceVariableRunTime.Name):
                        item.Sortable = true;
                        break;
                    case nameof(DeviceVariableRunTime.DeviceName):
                        item.Sortable = true;
                        break;
                    case nameof(DeviceVariableRunTime.VariableAddress):
                        item.Sortable = true;
                        break;
                    case nameof(DeviceVariableRunTime.OtherMethod):
                        item.Sortable = true;
                        break;
                    case nameof(DeviceVariableRunTime.ChangeTime):
                        item.Sortable = true;
                        break;
                    case nameof(DeviceVariableRunTime.IsOnline):
                        item.Sortable = true;
                        break;
                    case nameof(DeviceVariableRunTime.ReadExpressions):
                        item.Sortable = true;
                        break;
                }
            }
        }

        private void Filters(List<Filters> datas)
        {
            foreach (var item in datas)
            {
                switch (item.Key)
                {
                    case nameof(DeviceVariableRunTime.CreateTime):
                    case nameof(DeviceVariableRunTime.UpdateTime):
                    case nameof(DeviceVariableRunTime.CreateUser):
                    case nameof(DeviceVariableRunTime.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }
        [Inject]
        GlobalDeviceData _globalDeviceData { get; set; }
        private async Task<SqlSugarPagedList<DeviceVariableRunTime>> QueryCall(VariablePageInput input)
        {
            var uploadDevId = _uploadDeviceService.GetIdByName(input.UploadDeviceName);
            var data = await _globalDeviceData.AllVariables
                .WhereIf(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName == input.DeviceName)
                .WhereIf(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name))
                .WhereIf(!input.VariableAddress.IsNullOrEmpty(), a => a.VariableAddress.Contains(input.VariableAddress))
                .WhereIf(!input.UploadDeviceName.IsNullOrEmpty(), a => a.VariablePropertys.ContainsKey(uploadDevId ?? 0))
                .ToList().ToPagedListAsync(input);
            return data;
        }

        async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }
        }
        private async Task Write(DeviceVariableRunTime collectVariableRunTime)
        {
            var confirm = await PopupService.PromptAsync(T("写入"), $"输入变量{collectVariableRunTime.Name}的写入值");
            if (confirm != null)
            {
                var data = await rpcCore?.InvokeDeviceMethodAsync($"BLAZOR-{UserResoures.CurrentUser.Account}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()}", new KeyValuePair<string, string>(collectVariableRunTime.Name, confirm));
                if (data.IsSuccess)
                {
                    await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync(data.Message, AlertTypes.Success));
                }
                else
                {
                    await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync(data.Message, AlertTypes.Warning));
                }
            }
        }
    }
}