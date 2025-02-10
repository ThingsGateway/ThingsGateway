namespace ThingsGateway.Blazor.Diagrams.Core.Geometry;

public record Size
{
    public static Size Zero { get; } = new(0, 0);

    public Size(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public double Width { get; init; }
    public double Height { get; init; }
}
