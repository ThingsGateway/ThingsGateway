using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Behaviors;

public class VirtualizationBehavior : Behavior
{
    public VirtualizationBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.ZoomChanged += CheckVisibility;
        Diagram.PanChanged += CheckVisibility;
        Diagram.ContainerChanged += CheckVisibility;
    }

    private void CheckVisibility()
    {
        if (!Diagram.Options.Virtualization.Enabled)
            return;

        if (Diagram.Container == null)
            return;

        if (Diagram.Options.Virtualization.OnNodes)
        {
            foreach (var node in Diagram.Nodes)
            {
                CheckVisibility(node);
            }
        }

        if (Diagram.Options.Virtualization.OnGroups)
        {
            foreach (var group in Diagram.Groups)
            {
                CheckVisibility(group);
            }
        }

        if (Diagram.Options.Virtualization.OnLinks)
        {
            foreach (var link in Diagram.Links)
            {
                CheckVisibility(link);
            }
        }
    }

    private void CheckVisibility(Model model)
    {
        if (model is not IHasBounds ihb)
            return;

        var bounds = ihb.GetBounds();
        if (bounds == null)
            return;

        var left = bounds.Left * Diagram.Zoom + Diagram.Pan.X;
        var top = bounds.Top * Diagram.Zoom + Diagram.Pan.Y;
        var right = left + bounds.Width * Diagram.Zoom;
        var bottom = top + bounds.Height * Diagram.Zoom;
        model.Visible = right > 0 && left < Diagram.Container!.Width && bottom > 0 && top < Diagram.Container.Height;
    }

    public override void Dispose()
    {
        Diagram.ZoomChanged -= CheckVisibility;
        Diagram.PanChanged -= CheckVisibility;
        Diagram.ContainerChanged -= CheckVisibility;
    }
}