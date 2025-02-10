using ThingsGateway.Blazor.Diagrams.Core.Options;

namespace ThingsGateway.Blazor.Diagrams.Options;

public class BlazorDiagramOptions : DiagramOptions
{
    public int LinksLayerOrder { get; set; } = 0;
    public int NodesLayerOrder { get; set; } = 0;

    public override BlazorDiagramZoomOptions Zoom { get; } = new();
    public override BlazorDiagramLinkOptions Links { get; } = new();
    public override BlazorDiagramGroupOptions Groups { get; } = new();
    public override BlazorDiagramConstraintsOptions Constraints { get; } = new();
    public override BlazorDiagramVirtualizationOptions Virtualization { get; } = new();
}