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

using Masa.Blazor;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using SqlSugar;

using System;
using System.IO;

namespace ThingsGateway.Web.Page
{
    public partial class UploadDevicePage
    {
        private IAppDataTable _datatable;
        List<string> _deviceGroups = new();
        string _searchName;
        List<DriverPluginCategory> DriverPlugins;
        ImportExcel ImportExcel;
        private UploadDevicePageInput search = new();
        StringNumber tab;
        [Inject] public JsInitVariables JsInitVariables { get; set; } = default!;
        [Inject]
        IJSRuntime JS { get; set; }
        [CascadingParameter]
        MainLayout MainLayout { get; set; }

        [Inject]
        ResourceService ResourceService { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            DriverPlugins = DriverPluginService.GetDriverPluginChildrenList(DriverEnum.Upload);
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
            await base.OnParametersSetAsync();
        }

        private async Task AddCall(UploadDeviceAddInput input)
        {
            await UploadDeviceService.AddAsync(input);
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        async Task CopyDevice(IEnumerable<UploadDevice> data)
        {
            if (!data.Any())
            {
                await PopupService.EnqueueSnackbarAsync(@T("需选择一项或多项"), AlertTypes.Warning);
                return;
            }

            await UploadDeviceService.CopyDevAsync(data);
            await datatableQuery();
            await PopupService.EnqueueSnackbarAsync("复制成功", AlertTypes.Success);
        }

        private async Task datatableQuery()
        {
            await _datatable?.QueryClickAsync();
        }

        private async Task DeleteCall(IEnumerable<UploadDevice> input)
        {
            await UploadDeviceService.DeleteAsync(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImport(IBrowserFile file)
        {
            return UploadDeviceService.PreviewAsync(file);
        }

        async Task DownDeviceExport(IEnumerable<UploadDevice> input = null)
        {
            try
            {
                using var memoryStream = await UploadDeviceService.ExportFileAsync(input?.ToList());
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"上传设备导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }

        }

        async Task DownDeviceExport(UploadDevicePageInput input)
        {
            try
            {
                using var memoryStream = await UploadDeviceService.ExportFileAsync(input);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"上传设备导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }

        }

        private async Task DriverValueChanged(UploadDeviceEditInput context, long pluginId)
        {
            if (pluginId > 0)
                context.PluginId = pluginId;
            else
                return;
            if (context.DevicePropertys == null || context.DevicePropertys?.Count == 0)
            {
                context.DevicePropertys = GetDriverProperties(context.PluginId, context.Id);
                await PopupService.EnqueueSnackbarAsync("插件附加属性已更新", AlertTypes.Success);
            }

        }
        private async Task EditCall(UploadDeviceEditInput input)
        {
            await UploadDeviceService.EditAsync(input);
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        private void FilterHeaders(List<DataTableHeader<UploadDevice>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.UpdateUserId));

            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.Id));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.DevicePropertys));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(UploadDevice.Name):
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
                    case nameof(UploadDevice.CreateTime):
                    case nameof(UploadDevice.UpdateTime):
                    case nameof(UploadDevice.CreateUser):
                    case nameof(UploadDevice.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        List<DependencyProperty> GetDriverProperties(long driverId, long devId)
        {
            return ServiceExtensions.GetBackgroundService<UploadDeviceWorker>().GetDevicePropertys(driverId, devId);
        }

        private async Task<SqlSugarPagedList<UploadDevice>> QueryCall(UploadDevicePageInput input)
        {
            var data = await UploadDeviceService.PageAsync(input);
            return data;
        }

        async Task SaveDeviceImport(Dictionary<string, ImportPreviewOutputBase> data)
        {
            await UploadDeviceService.ImportAsync(data);
            await datatableQuery();
            ImportExcel.IsShowImport = false;
        }
    }
}