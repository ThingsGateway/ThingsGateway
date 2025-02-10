using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Positions;

public class LinkPathPositionProvider : IPositionProvider
{
    public LinkPathPositionProvider(double distance, double offsetX = 0, double offsetY = 0)
    {
        Distance = distance;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public double Distance { get; }
    public double OffsetX { get; }
    public double OffsetY { get; }

    public Point? GetPosition(Model model)
    {
        if (model is not BaseLinkModel link)
            throw new DiagramsException("LinkPathPositionProvider requires a link model");

        if (link.PathGeneratorResult == null)
            return null;

        var totalLength = link.PathGeneratorResult.FullPath.Length;
        var length = Distance switch
        {
            >= 0 and <= 1 => Distance * totalLength,
            > 1 => Distance,
            < 0 => totalLength + Distance,
            _ => throw new NotImplementedException()
        };

        var pt = link.PathGeneratorResult.FullPath.GetPointAtLength(length);
        return new Point(pt.X + OffsetX, pt.Y + OffsetY);
    }
}