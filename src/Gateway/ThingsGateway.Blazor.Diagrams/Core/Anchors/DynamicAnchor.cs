using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
using ThingsGateway.Blazor.Diagrams.Core.Positions;

namespace ThingsGateway.Blazor.Diagrams.Core.Anchors;

public sealed class DynamicAnchor : Anchor
{
    public DynamicAnchor(NodeModel model, IPositionProvider[] providers) : base(model)
    {
        if (providers.Length == 0)
            throw new InvalidOperationException("No providers provided");

        Node = model;
        Providers = providers;
    }

    public NodeModel Node { get; }
    public IPositionProvider[] Providers { get; }

    public override Point? GetPosition(BaseLinkModel link, Point[] route)
    {
        if (Node.Size == null)
            return null;

        var isTarget = link.Target == this;
        var pt = route.Length > 0 ? route[isTarget ? ^1 : 0] : GetOtherPosition(link, isTarget);
        var positions = Providers.Select(e => e.GetPosition(Node));
        return pt is null ? null : GetClosestPointTo(positions, pt);
    }

    public override Point? GetPlainPosition() => Node.GetBounds()?.Center ?? null;
}