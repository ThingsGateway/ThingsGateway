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

using BlazorComponent;

using Mapster;

namespace ThingsGateway.Web.Rcl
{
    public partial class Config
    {
        private IAppDataTable _datatable;
        private ConfigPageInput search = new();
        private List<DevConfig> _sysConfig = new();
        protected override async Task OnInitializedAsync()
        {
            _sysConfig = await ConfigService.GetListByCategory(CateGoryConst.Config_SYS_BASE);
            await base.OnInitializedAsync();
        }

        private async Task AddCall(ConfigAddInput input)
        {
            await ConfigService.Add(input);
        }

        private async Task DeleteCall(IEnumerable<DevConfig> sysConfigs)
        {
            await ConfigService.Delete(sysConfigs.Adapt<ConfigDeleteInput[]>());
        }

        private async Task EditCall(ConfigEditInput sysConfigs)
        {
            await ConfigService.Edit(sysConfigs);
        }

        private void FilterHeaders(List<DataTableHeader<DevConfig>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(DevConfig.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(DevConfig.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(DevConfig.Id));
            datas.RemoveWhere(it => it.Value == nameof(DevConfig.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DevConfig.UpdateUserId));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(DevConfig.Category):
                        item.Sortable = true;
                        break;

                    case nameof(DevConfig.ConfigKey):
                        item.Sortable = true;
                        break;

                    case nameof(DevConfig.ConfigValue):
                        item.CellClass += " table-text-truncate ";
                        item.Sortable = true;
                        break;

                    case nameof(DevConfig.Remark):
                        item.Sortable = true;
                        break;

                    case BlazorConst.TB_Actions:
                        item.CellClass = "";
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<DevConfig>> QueryCall(ConfigPageInput input)
        {
            var data = await ConfigService.Page(input);
            return data;
        }
    }
}