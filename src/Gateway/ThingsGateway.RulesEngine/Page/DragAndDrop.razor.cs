#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using Microsoft.AspNetCore.Components.Web;

using System.Text;

using ThingsGateway.Blazor.Diagrams.Options;
using ThingsGateway.NewLife.Json.Extension;


namespace ThingsGateway.RulesEngine;

public partial class DragAndDrop
{
    private readonly BlazorDiagram _blazorDiagram = new(new BlazorDiagramOptions
    {
        GridSize = 75,
        GridSnapToCenter = true,
    });
    private string? _draggedType;
    [Inject]
    IStringLocalizer<ThingsGateway.RulesEngine._Imports> Localizer { get; set; }



    protected override void OnInitialized()
    {
        base.OnInitialized();
        _blazorDiagram.Options.Links.EnableSnapping = true;
        _blazorDiagram.Options.Zoom.Enabled = false;

        foreach (var item in RuleHelpers.CategoryNodeDict)
        {
            _blazorDiagram.RegisterComponent(item.Key, item.Value.WidgetType);
        }
    }


    private Task OnDragStart(string key)
    {
        _draggedType = key;
        return Task.CompletedTask;
    }

    private void OnDrop(DragEventArgs e)
    {
        if (string.IsNullOrEmpty(_draggedType))
            return;

        Point? position = _blazorDiagram.GetRelativeMousePoint(e.ClientX, e.ClientY);
        NodeModel node = RuleHelpers.GetNodeModel(_draggedType, Guid.NewGuid().ToString(), position);
        _blazorDiagram.Nodes.Add(node);
        _draggedType = null;
    }
    internal async Task OnSave()
    {
        try
        {
            Value = RuleHelpers.Save(_blazorDiagram);
            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(Value);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    [Inject]
    ToastService ToastService { get; set; }
    [Parameter]
    public EventCallback OnCancel { get; set; }
    [Parameter]
    public EventCallback<RulesJson?> ValueChanged { get; set; }
    [Parameter]
    public RulesJson Value { get; set; }
    internal async Task Load(UploadFile upload)
    {
        try
        {
            var data = await upload.GetBytesAsync(1024 * 1024);
            var str = Encoding.UTF8.GetString(data);
            await Load(str.FromJsonNetString<RulesJson>());
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await Load(Value);
    }
    public async Task Load(RulesJson value)
    {
        try
        {
            System.Console.WriteLine("1");
            Value = value;
            RuleHelpers.Load(_blazorDiagram, Value);
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    [Inject]
    DownloadService DownloadService { get; set; }

    internal async Task Download()
    {
        try
        {

            var data = RuleHelpers.Save(_blazorDiagram);
            await DownloadService.DownloadFromStreamAsync("RulesJson.json", new MemoryStream(Encoding.UTF8.GetBytes(data.ToJsonNetString())));

        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

}
