using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Anchors;

public abstract class Anchor
{
    protected Anchor(ILinkable? model = null)
    {
        Model = model;
    }

    public ILinkable? Model { get; }

    public abstract Point? GetPosition(BaseLinkModel link, Point[] route);

    public abstract Point? GetPlainPosition();

    public Point? GetPosition(BaseLinkModel link) => GetPosition(link, Array.Empty<Point>());

    protected static Point? GetOtherPosition(BaseLinkModel link, bool isTarget)
    {
        var anchor = isTarget ? link.Source : link.Target!;
        return anchor.GetPlainPosition();
    }

    protected static Point? GetClosestPointTo(IEnumerable<Point?> points, Point point)
    {
        var minDist = double.MaxValue;
        Point? minPoint = null;

        foreach (var pt in points)
        {
            if (pt == null)
                continue;

            var dist = pt.DistanceTo(point);
            if (dist < minDist)
            {
                minDist = dist;
                minPoint = pt;
            }
        }

        return minPoint;
    }
}
