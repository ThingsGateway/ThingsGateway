using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Anchors;

public sealed class PositionAnchor : Anchor
{
    private Point _position;

    public PositionAnchor(Point position) : base(null)
    {
        _position = position;
    }

    public void SetPosition(Point position) => _position = position;

    public override Point? GetPlainPosition() => _position;

    public override Point? GetPosition(BaseLinkModel link, Point[] route) => _position;
}
