namespace ThingsGateway.Blazor.Diagrams.Core.Options;

public class DiagramOptions
{
    public int? GridSize { get; set; }
    public bool GridSnapToCenter { get; set; }
    public bool AllowMultiSelection { get; set; } = true;
    public bool AllowPanning { get; set; } = true;

    public virtual DiagramZoomOptions Zoom { get; } = new();
    public virtual DiagramLinkOptions Links { get; } = new();
    public virtual DiagramGroupOptions Groups { get; } = new();
    public virtual DiagramConstraintsOptions Constraints { get; } = new();
    public virtual DiagramVirtualizationOptions Virtualization { get; } = new();
}