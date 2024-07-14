//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

using System.Collections.Concurrent;

using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

public partial class VariableEditComponent
{
    public long ChoiceBusinessDeviceId;

    [Parameter]
    public bool BatchEditEnable { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Dictionary<long, Device> BusinessDeviceDict { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public IEnumerable<SelectedItem> BusinessDevices { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Dictionary<long, Device> CollectDeviceDict { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public IEnumerable<SelectedItem> CollectDevices { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Variable? Model { get; set; }

    [Parameter]
    public Func<Task> OnValidSubmit { get; set; }

    public IEnumerable<SelectedItem> OtherMethods { get; set; }

    [Parameter]
    public bool ValidateEnable { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    [Inject]
    [NotNull]
    private IPluginService PluginService { get; set; }

    private ConcurrentDictionary<long, IEnumerable<IEditorItem>>? VariablePropertyEditors { get; set; } = new();

    public async Task ValidSubmit(EditContext editContext)
    {
        try
        {
            if (OnValidSubmit != null)
                await OnValidSubmit.Invoke();
            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Model.VariablePropertys ??= new();
        foreach (var item in Model.VariablePropertys)
        {
            await RefreshBusinessPropertyClickAsync(item.Key);
        }
    }

    private Task OnDeviceSelectedItemChanged(SelectedItem selectedItem)
    {
        try
        {
            if (CollectDeviceDict.TryGetValue(selectedItem.Value.ToLong(), out var device))
            {
                var data = PluginService.GetDriverMethodInfos(device.PluginName);
                OtherMethods = data.Select(a => new SelectedItem(a.Name, a.Description)).Append(new SelectedItem(string.Empty, "none"));
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
        }
        return Task.CompletedTask;
    }

    private async Task RefreshBusinessPropertyClickAsync(long id)
    {
        if (id > 0)
        {
            if (BusinessDeviceDict.TryGetValue(id, out var device))
            {
                var data = PluginService.GetVariablePropertyTypes(device.PluginName);
                Model.VariablePropertyModels ??= new();
                Model.VariablePropertyModels.AddOrUpdate(id, (a) => new ModelValueValidateForm() { Value = data.Model }, (a, b) => new ModelValueValidateForm() { Value = data.Model });
                VariablePropertyEditors.TryAdd(id, data.EditorItems);
                if (Model.VariablePropertys.TryGetValue(id, out var dict))
                {
                    PluginServiceUtil.SetModel(data.Model, dict);
                }
            }
        }
        else
        {
            await ToastService.Warning(null, Localizer["RefreshBusinessPropertyError"]);
        }
    }
}
