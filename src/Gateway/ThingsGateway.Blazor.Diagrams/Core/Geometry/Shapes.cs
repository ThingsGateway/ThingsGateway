using ThingsGateway.Blazor.Diagrams.Core.Models;

namespace ThingsGateway.Blazor.Diagrams.Core.Geometry;

public static class Shapes
{
    public static IShape Rectangle(NodeModel node) => Rectangle(node.Position, node.Size!);

    public static IShape Circle(NodeModel node) => Circle(node.Position, node.Size!);

    public static IShape Ellipse(NodeModel node) => Ellipse(node.Position, node.Size!);

    public static IShape Rectangle(PortModel port) => Rectangle(port.Position, port.Size!);

    public static IShape Circle(PortModel port) => Circle(port.Position, port.Size!);

    public static IShape Ellipse(PortModel port) => Ellipse(port.Position, port.Size!);

    private static Rectangle Rectangle(Point position, Size size) => new Rectangle(position, size);

    private static Ellipse Circle(Point position, Size size)
    {
        var halfWidth = size.Width / 2;
        var centerX = position.X + halfWidth;
        var centerY = position.Y + size.Height / 2;
        return new Ellipse(centerX, centerY, halfWidth, halfWidth);
    }

    private static Ellipse Ellipse(Point position, Size size)
    {
        var halfWidth = size.Width / 2;
        var halfHeight = size.Height / 2;
        var centerX = position.X + halfWidth;
        var centerY = position.Y + halfHeight;
        return new Ellipse(centerX, centerY, halfWidth, halfHeight);
    }
}
