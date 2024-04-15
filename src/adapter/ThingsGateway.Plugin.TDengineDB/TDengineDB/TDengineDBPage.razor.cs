//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

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
        var query = TDengineDBProducer.Query(input);
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }
}