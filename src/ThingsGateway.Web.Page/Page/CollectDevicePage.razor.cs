#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Masa.Blazor;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using SqlSugar;

using System;
using System.IO;

namespace ThingsGateway.Web.Page
{
    public partial class CollectDevicePage
    {
        IAppDataTable _datatable;
        List<string> _deviceGroups = new();
        string _searchName;
        List<CollectDevice> CollectDevices = new();
        List<DriverPluginCategory> DriverPlugins;
        ImportExcel ImportExcel;
        CollectDevicePageInput search = new();
        StringNumber tab;
        [Inject] public JsInitVariables JsInitVariables { get; set; } = default!;

        [Inject]
        IJSRuntime JS { get; set; }

        [CascadingParameter]
        MainLayout MainLayout { get; set; }

        [Inject]
        ResourceService ResourceService { get; set; }
        protected override void OnAfterRender(bool firstRender)
        {
            CollectDevices = CollectDeviceService.GetCacheList();
            base.OnAfterRender(firstRender);
        }
        protected override async Task OnParametersSetAsync()
        {

            DriverPlugins = DriverPluginService.GetDriverPluginChildrenList(DriverEnum.Collect);
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
            await base.OnParametersSetAsync();
        }
        private async Task AddCall(CollectDeviceAddInput input)
        {
            await CollectDeviceService.AddAsync(input);
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        async Task CopyDevAndVar(IEnumerable<CollectDevice> data)
        {
            if (!data.Any())
            {
                await PopupService.EnqueueSnackbarAsync(@T("需选择一项或多项"), AlertTypes.Warning);
                return;
            }

            await CollectDeviceService.CopyDevAndVarAsync(data);
            await DatatableQuery();
            await PopupService.EnqueueSnackbarAsync("复制成功", AlertTypes.Success);
        }

        async Task CopyDevice(IEnumerable<CollectDevice> data)
        {
            if (!data.Any())
            {
                await PopupService.EnqueueSnackbarAsync(@T("需选择一项或多项"), AlertTypes.Warning);
                return;
            }

            await CollectDeviceService.CopyDevAsync(data);
            await DatatableQuery();
            await PopupService.EnqueueSnackbarAsync("复制成功", AlertTypes.Success);
        }

        private async Task DatatableQuery()
        {
            await _datatable?.QueryClickAsync();
        }

        private async Task DeleteCall(IEnumerable<CollectDevice> input)
        {
            await CollectDeviceService.DeleteAsync(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        void DeviceChanged(long devId)
        {
            if (devId > 0)
            {

            }
        }
        Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImport(IBrowserFile file)
        {
            return CollectDeviceService.PreviewAsync(file);
        }

        async Task DownDeviceExport(IEnumerable<CollectDevice> input = null)
        {
            try
            {
                using var memoryStream = await CollectDeviceService.ExportFileAsync(input?.ToList());
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"采集设备导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }

        }
        async Task DownDeviceExport(CollectDevicePageInput input)
        {
            try
            {
                using var memoryStream = await CollectDeviceService.ExportFileAsync(input);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"采集设备导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }

        }

        private async Task DriverValueChanged(CollectDeviceEditInput context, long pluginId)
        {
            bool a = false;
            if (context.PluginId != pluginId && pluginId > 0)
            {
                a = true;
            }
            if (pluginId > 0)
                context.PluginId = pluginId;
            else
                return;
            if (context.DevicePropertys == null || context.DevicePropertys?.Count == 0 || a)
            {
                context.DevicePropertys = GetDriverProperties(context.PluginId, context.Id);
                await PopupService.EnqueueSnackbarAsync("插件附加属性已更新", AlertTypes.Success);
            }
        }

        private async Task EditCall(CollectDeviceEditInput input)
        {
            await CollectDeviceService.EditAsync(input);
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();

        }

        private void FilterHeaders(List<DataTableHeader<CollectDevice>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.UpdateUserId));

            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.RedundantDeviceId));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.Id));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.DevicePropertys));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(CollectDevice.Name):
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
                    case nameof(CollectDevice.CreateTime):
                    case nameof(CollectDevice.UpdateTime):
                    case nameof(CollectDevice.CreateUser):
                    case nameof(CollectDevice.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        List<DependencyProperty> GetDriverProperties(long driverId, long devId)
        {
            return ServiceExtensions.GetBackgroundService<CollectDeviceWorker>().GetDevicePropertys(driverId, devId);
        }

        private async Task<SqlSugarPagedList<CollectDevice>> QueryCall(CollectDevicePageInput input)
        {
            var data = await CollectDeviceService.PageAsync(input);
            return data;
        }

        async Task SaveDeviceImport(Dictionary<string, ImportPreviewOutputBase> data)
        {
            await CollectDeviceService.ImportAsync(data);
            await DatatableQuery();
            ImportExcel.IsShowImport = false;
        }
    }
}