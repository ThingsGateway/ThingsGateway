using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Positions;

public class BoundsBasedPositionProvider : IPositionProvider
{
    public BoundsBasedPositionProvider(double x, double y, double offsetX = 0, double offsetY = 0)
    {
        X = x;
        Y = y;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public double X { get; }
    public double Y { get; }
    public double OffsetX { get; }
    public double OffsetY { get; }

    public Point? GetPosition(Model model)
    {
        if (model is not IHasBounds ihb)
            throw new DiagramsException("BoundsBasedPositionProvider requires an IHasBounds model");

        var bounds = ihb.GetBounds();
        if (bounds == null)
            return null;

        return new Point(bounds.Left + X * bounds.Width + OffsetX, bounds.Top + Y * bounds.Height + OffsetY);
    }
}