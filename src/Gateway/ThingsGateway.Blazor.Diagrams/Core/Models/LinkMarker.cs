namespace ThingsGateway.Blazor.Diagrams.Core.Models;

public class LinkMarker
{
    public static LinkMarker Arrow { get; } = new LinkMarker("M 0 -5 10 0 0 5 z", 10);
    public static LinkMarker Circle { get; } = new LinkMarker("M 0, 0 a 5,5 0 1,0 10,0 a 5,5 0 1,0 -10,0", 10);
    public static LinkMarker Square { get; } = new LinkMarker("M 0 -5 10 -5 10 5 0 5 z", 10);

    public LinkMarker(string path, double width)
    {
        Path = path;
        Width = width;
    }

    public string Path { get; }
    public double Width { get; }

    public static LinkMarker NewArrow(double width, double height)
        => new LinkMarker(FormattableString.Invariant($"M 0 -{height / 2} {width} 0 0 {height / 2}"), width);

    public static LinkMarker NewCircle(double r)
        => new LinkMarker(FormattableString.Invariant($"M 0, 0 a {r},{r} 0 1,0 {r * 2},0 a {r},{r} 0 1,0 -{r * 2},0"), r * 2);

    public static LinkMarker NewRectangle(double width, double height)
        => new LinkMarker(FormattableString.Invariant($"M 0 -{height / 2} {width} -{height / 2} {width} {height / 2} 0 {height / 2} z"), width);

    public static LinkMarker NewSquare(double size) => NewRectangle(size, size);
}
