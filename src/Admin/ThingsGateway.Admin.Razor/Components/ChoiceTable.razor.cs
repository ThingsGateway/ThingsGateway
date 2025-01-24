//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Razor;

public partial class ChoiceTable<TItem> where TItem : class, new()
{
    [Parameter]
    [EditorRequired]
    [NotNull]
    public Func<HashSet<TItem>, Task>? OnChangedAsync { get; set; }
    [Parameter]
    [EditorRequired]
    public HashSet<TItem> SelectedRows { get; set; }
    [Parameter]
    [EditorRequired]
    public Func<QueryPageOptions, Task<QueryData<TItem>>> OnQueryAsync { get; set; }

    private List<TItem> SelectedAddRows { get; set; } = new();
    private List<TItem> SelectedDeleteRows { get; set; } = new();

    [Parameter]
    public int MaxCount { get; set; } = 0;

    public async Task OnAddAsync(IEnumerable<TItem> selectorOutputs)
    {
        if (MaxCount > 0 && selectorOutputs.Count() + SelectedRows.Count > MaxCount)
        {
            await ToastService.Warning(AdminLocalizer["MaxCount"]);
            return;
        }
        foreach (var item in selectorOutputs)
        {
            SelectedRows.Add(item);
            await table2.QueryAsync();
            await OnChangedAsync(SelectedRows);
        }
    }
    public async Task OnAddAsync(TItem item)
    {
        if (MaxCount > 0 && 1 + SelectedRows.Count > MaxCount)
        {
            await ToastService.Warning(AdminLocalizer["MaxCount"]);
            return;
        }
        SelectedRows.Add(item);
        await table2.QueryAsync();
        await OnChangedAsync(SelectedRows);
    }
    public async Task QueryAsync()
    {
        await table1.QueryAsync();
    }




}
