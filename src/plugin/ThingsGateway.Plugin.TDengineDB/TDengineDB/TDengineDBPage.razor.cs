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

using Microsoft.AspNetCore.Components;

using SqlSugar;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core;

namespace ThingsGateway.Plugin.TDengineDB;

public partial class TDengineDBPage : IDriverUIBase
{
    private readonly TDengineDBPageInput _search = new();
    private IAppDataTable _datatable;

    [Parameter, EditorRequired]
    public object Driver { get; set; }

    public TDengineDBProducer TDengineDBProducer => (TDengineDBProducer)Driver;

    private async Task<SqlSugarPagedList<TDengineDBHistoryValue>> QueryCallAsync(TDengineDBPageInput input)
    {
        using var db = BusinessDatabaseUtil.GetDb(TDengineDBProducer._driverPropertys.DbType, TDengineDBProducer._driverPropertys.BigTextConnectStr);
        var query = db.Queryable<TDengineDBHistoryValue>()
                             .WhereIF(input.StartTime != null, a => a.CreateTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.CreateTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.VariableName), it => it.Name.Contains(input.VariableName))
                           ;

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }
}