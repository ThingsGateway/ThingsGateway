using ThingsGateway.Blazor.Diagrams.Core.Behaviors;
using ThingsGateway.Blazor.Diagrams.Core.Events;
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
using ThingsGateway.Blazor.Diagrams.Core.Positions;

namespace ThingsGateway.Blazor.Diagrams.Core.Controls.Default;

public class ArrowHeadControl : ExecutableControl
{
    public ArrowHeadControl(bool source, LinkMarker? marker = null)
    {
        Source = source;
        Marker = marker ?? LinkMarker.NewArrow(20, 20);
    }

    public bool Source { get; }
    public LinkMarker Marker { get; }
    public double Angle { get; private set; }

    public override Point? GetPosition(Model model)
    {
        if (model is not BaseLinkModel link)
            throw new DiagramsException("ArrowHeadControl only works for models of type BaseLinkModel");

        var dist = Source ? Marker.Width - (link.SourceMarker?.Width ?? 0) : (link.TargetMarker?.Width ?? 0) - Marker.Width;
        var pp = new LinkPathPositionProvider(dist);
        var p1 = pp.GetPosition(link);
        if (p1 is not null)
        {
            var p2 = Source ? link.Source.GetPosition(link) : link.Target.GetPosition(link);
            if (p2 is not null)
            {
                Angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X) * 180 / Math.PI;
            }
        }

        return p1;
    }

    public override ValueTask OnPointerDown(Diagram diagram, Model model, PointerEventArgs e)
    {
        if (model is not BaseLinkModel link)
            throw new DiagramsException("ArrowHeadControl only works for models of type BaseLinkModel");

        var dnlb = diagram.GetBehavior<DragNewLinkBehavior>()!;
        if (Source)
        {
            link.SetSource(link.Target);
        }

        dnlb.StartFrom(link, e.ClientX, e.ClientY);
        return ValueTask.CompletedTask;
    }
}
