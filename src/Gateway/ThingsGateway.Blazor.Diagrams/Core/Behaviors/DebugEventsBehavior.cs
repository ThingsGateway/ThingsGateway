using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Behaviors;

public class DebugEventsBehavior : Behavior
{
    public DebugEventsBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.Changed += Diagram_Changed;
        Diagram.ContainerChanged += Diagram_ContainerChanged;
        Diagram.PanChanged += Diagram_PanChanged;
        Diagram.Nodes.Added += Nodes_Added;
        Diagram.Nodes.Removed += Nodes_Removed;
        Diagram.Links.Added += Links_Added;
        Diagram.Links.Removed += Links_Removed;
        Diagram.Groups.Added += Diagram_GroupAdded;
        Diagram.Groups.Removed += Diagram_GroupRemoved;
        Diagram.SelectionChanged += Diagram_SelectionChanged;
        Diagram.ZoomChanged += Diagram_ZoomChanged;
    }

    private void Diagram_ZoomChanged()
    {
        Console.WriteLine($"ZoomChanged, Zoom={Diagram.Zoom}");
    }

    private void Diagram_SelectionChanged(SelectableModel obj)
    {
        Console.WriteLine($"SelectionChanged, Model={obj.GetType().Name}, Selected={obj.Selected}");
    }

    private void Links_Removed(BaseLinkModel obj)
    {
        Console.WriteLine($"Links.Removed, Links=[{obj}]");
    }

    private void Links_Added(BaseLinkModel obj)
    {
        Console.WriteLine($"Links.Added, Links=[{obj}]");
    }

    private void Nodes_Removed(NodeModel obj)
    {
        Console.WriteLine($"Nodes.Removed, Nodes=[{obj}]");
    }

    private void Nodes_Added(NodeModel obj)
    {
        Console.WriteLine($"Nodes.Added, Nodes=[{obj}]");
    }

    private void Diagram_PanChanged()
    {
        Console.WriteLine($"PanChanged, Pan={Diagram.Pan}");
    }

    private void Diagram_GroupRemoved(GroupModel obj)
    {
        Console.WriteLine($"GroupRemoved, Id={obj.Id}");
    }

    private void Diagram_GroupAdded(GroupModel obj)
    {
        Console.WriteLine($"GroupAdded, Id={obj.Id}");
    }

    private void Diagram_ContainerChanged()
    {
        Console.WriteLine($"ContainerChanged, Container={Diagram.Container}");
    }

    private void Diagram_Changed()
    {
        Console.WriteLine("Changed");
    }

    public override void Dispose()
    {
        Diagram.Changed -= Diagram_Changed;
        Diagram.ContainerChanged -= Diagram_ContainerChanged;
        Diagram.PanChanged -= Diagram_PanChanged;
        Diagram.Nodes.Added -= Nodes_Added;
        Diagram.Nodes.Removed -= Nodes_Removed;
        Diagram.Links.Added -= Links_Added;
        Diagram.Links.Removed -= Links_Removed;
        Diagram.Groups.Added -= Diagram_GroupAdded;
        Diagram.Groups.Removed -= Diagram_GroupRemoved;
        Diagram.SelectionChanged -= Diagram_SelectionChanged;
        Diagram.ZoomChanged -= Diagram_ZoomChanged;
    }
}
