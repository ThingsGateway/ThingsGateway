using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Positions;

public class ShapeAnglePositionProvider : IPositionProvider
{
    public ShapeAnglePositionProvider(double angle, double offsetX = 0, double offsetY = 0)
    {
        Angle = angle;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public double Angle { get; }
    public double OffsetX { get; }
    public double OffsetY { get; }

    public Point? GetPosition(Model model)
    {
        if (model is not IHasShape ihs)
            throw new DiagramsException("ShapeAnglePositionProvider requires an IHasShape model");

        var shape = ihs.GetShape();
        return shape.GetPointAtAngle(Angle)?.Add(OffsetX, OffsetY);
    }
}