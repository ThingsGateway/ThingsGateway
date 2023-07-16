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

using Mapster;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using SqlSugar;

using System;
using System.IO;

namespace ThingsGateway.Web.Page
{
    public partial class MemoryVariablePage
    {
        private IAppDataTable _datatable;

        long choiceUploadDeviceId;

        ImportExcel ImportExcel;

        private MemoryVariablePageInput search = new();

        StringNumber tab;

        List<UploadDevice> UploadDevices = new();

        [Inject]
        public JsInitVariables JsInitVariables { get; set; } = default!;

        [Inject]
        IJSRuntime JS { get; set; }

        [CascadingParameter]
        MainLayout MainLayout { get; set; }


        [Inject]
        ResourceService ResourceService { get; set; }

        [Inject]
        IUploadDeviceService UploadDeviceService { get; set; }
        protected override async Task OnParametersSetAsync()
        {
            UploadDevices = UploadDeviceService.GetCacheList();

            await base.OnParametersSetAsync();
        }
        private async Task AddCall(MemoryVariableAddInput input)
        {
            await VariableService.AddAsync(input);
        }

        private async Task Clear()
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("确认"), $"清空?");
            if (confirm)
            {
                await VariableService.ClearMemoryVariableAsync();
            }
            await datatableQuery();

        }

        private async Task datatableQuery()
        {
            await _datatable.QueryClickAsync();
        }

        private async Task DeleteCall(IEnumerable<DeviceVariable> input)
        {
            await VariableService.DeleteAsync(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
        }

        Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImport(IBrowserFile file)
        {
            return VariableService.PreviewAsync(file);
        }

        async Task DownDeviceExport(IEnumerable<DeviceVariable> input = null)
        {
            try
            {
                using var memoryStream = await VariableService.MemoryVariableExportFileAsync(input?.ToList()?.Adapt<List<MemoryVariable>>());
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"变量导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }
        }

        async Task DownDeviceExport(MemoryVariablePageInput input)
        {
            try
            {

                using var memoryStream = await VariableService.ExportFileAsync(input);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"变量导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }
        }


        private async Task EditCall(MemoryVariableAddInput input)
        {
            await VariableService.EditAsync(input);
        }

        private void FilterHeaders(List<DataTableHeader<DeviceVariable>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.UpdateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.Id));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.VariablePropertys));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.IsMemoryVariable));

            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.DeviceId));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.WriteExpressions));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.IntervalTime));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.OtherMethod));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.VariableAddress));
            datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.RpcWriteEnable));

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
                    case nameof(DeviceVariable.Name):
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
                    case nameof(DeviceVariable.CreateTime):
                    case nameof(DeviceVariable.UpdateTime):
                    case nameof(DeviceVariable.CreateUser):
                    case nameof(DeviceVariable.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        List<DependencyProperty> GetDriverProperties(long driverId, List<DependencyProperty> dependencyProperties)
        {
            return ServiceExtensions.GetBackgroundService<UploadDeviceWorker>().GetVariablePropertys(driverId, dependencyProperties);
        }

        private async Task<SqlSugarPagedList<DeviceVariable>> QueryCall(MemoryVariablePageInput input)
        {
            var data = await VariableService.PageAsync(input);
            return data;
        }

        async Task SaveDeviceImport(Dictionary<string, ImportPreviewOutputBase> data)
        {
            await VariableService.ImportAsync(data);
            await datatableQuery();
            ImportExcel.IsShowImport = false;
        }
    }
}