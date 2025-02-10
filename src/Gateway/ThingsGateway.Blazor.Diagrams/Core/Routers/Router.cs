using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Routers;

public abstract class Router
{
    public abstract Point[] GetRoute(Diagram diagram, BaseLinkModel link);

    protected static Point GetPortPositionBasedOnAlignment(PortModel port)
    {
        var pt = port.Position;
        switch (port.Alignment)
        {
            case PortAlignment.Top:
                return new Point(pt.X + port.Size.Width / 2, pt.Y);
            case PortAlignment.TopRight:
                return new Point(pt.X + port.Size.Width, pt.Y);
            case PortAlignment.Right:
                return new Point(pt.X + port.Size.Width, pt.Y + port.Size.Height / 2);
            case PortAlignment.BottomRight:
                return new Point(pt.X + port.Size.Width, pt.Y + port.Size.Height);
            case PortAlignment.Bottom:
                return new Point(pt.X + port.Size.Width / 2, pt.Y + port.Size.Height);
            case PortAlignment.BottomLeft:
                return new Point(pt.X, pt.Y + port.Size.Height);
            case PortAlignment.Left:
                return new Point(pt.X, pt.Y + port.Size.Height / 2);
            default:
                return pt;
        }
    }
}
