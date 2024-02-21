//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Blazor;

public partial class Button
{
    private IAppDataTable _buttonsDatatable;

    [Parameter]
    public long ParentId { get; set; }

    private Task<SqlSugarPagedList<SysResource>> ButtonQueryCallAsync(ButtonPageInput input)
    {
        input.ParentId = ParentId;
        return _serviceScope.ServiceProvider.GetService<IButtonService>().PageAsync(input);
    }

    private Task ButtonAddCallAsync(ButtonAddInput input)
    {
        return _serviceScope.ServiceProvider.GetService<IButtonService>().AddAsync(input);
    }

    private Task ButtonDeleteCallAsync(IEnumerable<SysResource> input)
    {
        return _serviceScope.ServiceProvider.GetService<IButtonService>().DeleteAsync(input.Adapt<List<BaseIdInput>>());
    }

    private Task ButtonEditCallAsync(ButtonEditInput input)
    {
        return _serviceScope.ServiceProvider.GetService<IButtonService>().EditAsync(input);
    }

    private async Task BatchAddClickAsync()
    {
        await PopupService.OpenAsync(typeof(ButtonBatch), new Dictionary<string, object?>()
        {
            {nameof(ButtonBatch.ButtonAddInput),new ButtonAddInput(){ ParentId=ParentId } },
});
        await _buttonsDatatable.QueryClickAsync();
    }

    private void ButtonFilters(List<Filters> datas)
    {
        foreach (var item in datas)
        {
            switch (item.Key)
            {
                case nameof(SysResource.Code):
                    item.Value = true;
                    break;

                case nameof(SysResource.MenuType):
                case nameof(SysResource.Icon):
                case nameof(SysResource.Href):
                    item.Value = false;
                    break;
            }
        }
    }
}