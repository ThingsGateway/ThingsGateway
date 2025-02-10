namespace ThingsGateway.Blazor.Diagrams.Core.Geometry;

public interface IShape
{
    public IEnumerable<Point> GetIntersectionsWithLine(Line line);
    public Point? GetPointAtAngle(double a);
}
