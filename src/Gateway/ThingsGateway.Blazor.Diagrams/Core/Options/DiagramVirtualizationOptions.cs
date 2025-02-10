namespace ThingsGateway.Blazor.Diagrams.Core.Options;

public class DiagramVirtualizationOptions
{
    public bool Enabled { get; set; }
    public bool OnNodes { get; set; } = true;
    public bool OnGroups { get; set; }
    public bool OnLinks { get; set; }
}