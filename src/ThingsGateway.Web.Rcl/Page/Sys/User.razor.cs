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

using BlazorComponent;

using SqlSugar;

using System;
using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class User
    {
        private IAppDataTable _datatable;
        private UserPageInput search = new();

        private async Task AddCall(UserAddInput input)
        {
            await SysUserService.Add(input);
        }

        private async Task DeleteCall(IEnumerable<SysUser> users)
        {
            await SysUserService.Delete(users.ToList().ConvertAll(it => it.Id.ToIdInput()));
        }

        private async Task EditCall(UserEditInput users)
        {
            await SysUserService.Edit(users);
        }

        private void FilterHeaders(List<DataTableHeader<SysUser>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(SysUser.Password));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.ButtonCodeList));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.PermissionCodeList));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.RoleCodeList));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.RoleIdList));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.UpdateUserId));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(SysUser.Id));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(SysUser.Account):
                        item.Sortable = true;
                        break;

                    case nameof(SysUser.LastLoginTime):
                        item.Sortable = true;
                        break;

                    case nameof(SysUser.LatestLoginTime):
                        item.Sortable = true;
                        break;

                    case nameof(SysUser.UserStatus):
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
                    case nameof(SysUser.Email):
                    case nameof(SysUser.LastLoginDevice):
                    case nameof(SysUser.LastLoginIp):
                    case nameof(SysUser.LastLoginTime):
                    case nameof(SysUser.SortCode):
                    case nameof(SysUser.CreateTime):
                    case nameof(SysUser.UpdateTime):
                    case nameof(SysUser.CreateUser):
                    case nameof(SysUser.UpdateUser):
                        item.Value = false;
                        break;

                }
            }
        }

        private async Task<SqlSugarPagedList<SysUser>> QueryCall(UserPageInput input)
        {
            var data = await SysUserService.Page(input);
            return data;
        }

        private async Task ResetPassword(SysUser sysUser)
        {
            await SysUserService.ResetPassword(sysUser.Id.ToIdInput());
            await PopupService.EnqueueSnackbarAsync(new(T("成功"), AlertTypes.Success));
        }

        private async Task UserStatusChange(SysUser context, bool enable)
        {
            try
            {
                if (enable)
                    await SysUserService.EnableUser(context.Id.ToIdInput());
                else
                    await SysUserService.DisableUser(context.Id.ToIdInput());
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(new(ex.Message,AlertTypes.Error));
            }
            finally
            {
                await _datatable?.QueryClickAsync();
            }
        }
    }
}