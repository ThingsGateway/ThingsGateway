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

using Microsoft.JSInterop;

using System;
using System.IO;

namespace ThingsGateway.Web.Page
{
    public partial class RpcLogPage
    {
        private IAppDataTable _datatable;
        private RpcLogPageInput search = new();
        [Inject]
        public JsInitVariables JsInitVariables { get; set; } = default!;

        [Inject]
        IJSRuntime JS { get; set; }

        private async Task ClearClick()
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("删除"), T("确定 ?"));
            if (confirm)
            {
                await RpcLogService.DeleteAsync();
                await _datatable?.QueryClickAsync();
            }
        }
        async Task DownDeviceExport(IEnumerable<RpcLog> input = null)
        {
            try
            {
                using var memoryStream = await RpcLogService.ExportFileAsync(input?.ToList());
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"后台日志导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }
        }
        async Task DownDeviceExport(RpcLogPageInput input)
        {
            try
            {
                using var memoryStream = await RpcLogService.ExportFileAsync(input);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"后台日志导出{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("MM-dd-HH-mm-ss")}.xlsx", streamRef);
            }
            finally
            {
            }
        }
        private void FilterHeaders(List<DataTableHeader<RpcLog>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(RpcLog.Id));
            foreach (DataTableHeader<RpcLog> item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {

                    case nameof(RpcLog.ParamJson):
                        item.CellClass += " table-text-truncate ";
                        break;

                    case nameof(RpcLog.ResultJson):
                        item.CellClass += " table-text-truncate ";
                        break;


                    case BlazorConst.TB_Actions:
                        item.CellClass = "";
                        break;
                }
            }
        }

        private void Filters(List<Filters> datas)
        {
            foreach (var item in datas)
            {

            }
        }


        private async Task<SqlSugarPagedList<RpcLog>> QueryCall(RpcLogPageInput input)
        {
            var data = await RpcLogService.PageAsync(input);
            return data;
        }
    }
}