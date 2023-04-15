namespace ThingsGateway.Web.Page
{
    public partial class RuntimeLogPage
    {
        private IAppDataTable _datatable;
        private RuntimeLogPageInput search = new();

        private void FilterHeaders(List<DataTableHeader<RuntimeLog>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(RuntimeLog.Id));
            foreach (DataTableHeader<RuntimeLog> item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {

                    case nameof(RuntimeLog.Exception):
                        item.CellClass += " table-text-truncate ";
                        break;

                    case nameof(RuntimeLog.LogMessage):
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


        private async Task<SqlSugarPagedList<RuntimeLog>> QueryCall(RuntimeLogPageInput input)
        {
            var data = await RuntimeLogService.PageAsync(input);
            return data;
        }

        private async Task ClearClick()
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("É¾³ý"), T("È·¶¨ ?"));
            if (confirm)
            {
                await RuntimeLogService.DeleteAsync();
                await _datatable?.QueryClickAsync();
            }
        }
    }
}