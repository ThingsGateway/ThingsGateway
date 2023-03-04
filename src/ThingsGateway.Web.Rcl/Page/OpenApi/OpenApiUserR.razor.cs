using BlazorComponent;

using SqlSugar;

using System;
using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class OpenApiUserR
    {
        private IAppDataTable _datatable;


        private OpenApiUserPageInput search = new();

        private async Task AddCall(OpenApiUserAddInput input)
        {
            await OpenApiUserService.Add(input);
        }

        private async Task DeleteCall(IEnumerable<OpenApiUser> users)
        {
            await OpenApiUserService.Delete(users.ToList().ConvertAll(it => it.Id.ToIdInput()));
        }

        private async Task EditCall(OpenApiUserEditInput users)
        {
            await OpenApiUserService.Edit(users);
        }

        private void FilterHeaders(List<DataTableHeader<OpenApiUser>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(OpenApiUser.Password));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiUser.PermissionCodeList));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiUser.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiUser.UpdateUserId));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiUser.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiUser.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(OpenApiUser.Id));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(OpenApiUser.Account):
                        item.Sortable = true;
                        break;

                    case nameof(OpenApiUser.LastLoginTime):
                        item.Sortable = true;
                        break;

                    case nameof(OpenApiUser.LatestLoginTime):
                        item.Sortable = true;
                        break;

                    case nameof(OpenApiUser.UserStatus):
                        item.Sortable = true;
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
                switch (item.Key)
                {
                    case nameof(OpenApiUser.Email):
                    case nameof(OpenApiUser.LastLoginDevice):
                    case nameof(OpenApiUser.LastLoginIp):
                    case nameof(OpenApiUser.LastLoginTime):
                    case nameof(OpenApiUser.SortCode):
                    case nameof(OpenApiUser.CreateTime):
                    case nameof(OpenApiUser.UpdateTime):
                    case nameof(OpenApiUser.CreateUser):
                    case nameof(OpenApiUser.UpdateUser):
                        item.Value = false;
                        break;

                }
            }
        }

        private async Task<SqlSugarPagedList<OpenApiUser>> QueryCall(OpenApiUserPageInput input)
        {
            var data = await OpenApiUserService.Page(input);
            return data;
        }




        private async Task UserStatusChange(OpenApiUser context, bool enable)
        {
            try
            {
                if (enable)
                    await OpenApiUserService.EnableUser(context.Id.ToIdInput());
                else
                    await OpenApiUserService.DisableUser(context.Id.ToIdInput());
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex, false);
            }
            finally
            {
                await _datatable?.QueryClick();
            }
        }
    }
}