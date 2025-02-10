using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Anchors;

public sealed class ShapeIntersectionAnchor : Anchor
{
    public ShapeIntersectionAnchor(NodeModel model) : base(model)
    {
        Node = model;
    }

    public NodeModel Node { get; }

    public override Point? GetPosition(BaseLinkModel link, Point[] route)
    {
        if (Node.Size == null)
            return null;

        var isTarget = link.Target == this;
        var nodeCenter = Node.GetBounds()!.Center;
        Point? pt;
        if (route.Length > 0)
        {
            pt = route[isTarget ? ^1 : 0];
        }
        else
        {
            pt = GetOtherPosition(link, isTarget);
        }

        if (pt is null) return null;

        var line = new Line(pt, nodeCenter);
        var intersections = Node.GetShape().GetIntersectionsWithLine(line);
        return GetClosestPointTo(intersections, pt);
    }

    public override Point? GetPlainPosition() => Node.GetBounds()?.Center ?? null;
}
