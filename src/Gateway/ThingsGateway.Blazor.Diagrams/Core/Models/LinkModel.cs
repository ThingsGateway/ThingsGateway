using ThingsGateway.Blazor.Diagrams.Core.Anchors;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Models;

public class LinkModel : BaseLinkModel
{
    public LinkModel(Anchor source, Anchor target) : base(source, target) { }

    public LinkModel(string id, Anchor source, Anchor target) : base(id, source, target) { }

    public LinkModel(PortModel sourcePort, PortModel targetPort)
        : base(new SinglePortAnchor(sourcePort), new SinglePortAnchor(targetPort)) { }

    public LinkModel(NodeModel sourceNode, NodeModel targetNode)
        : base(new ShapeIntersectionAnchor(sourceNode), new ShapeIntersectionAnchor(targetNode)) { }

    public LinkModel(string id, PortModel sourcePort, PortModel targetPort)
        : base(id, new SinglePortAnchor(sourcePort), new SinglePortAnchor(targetPort)) { }

    public LinkModel(string id, NodeModel sourceNode, NodeModel targetNode)
        : base(id, new ShapeIntersectionAnchor(sourceNode), new ShapeIntersectionAnchor(targetNode)) { }

    public string? Color { get; set; }
    public string? SelectedColor { get; set; }
    public double Width { get; set; } = 2;
}
