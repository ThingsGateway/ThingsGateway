using ThingsGateway.Blazor.Diagrams.Core.Anchors;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.PathGenerators;
using ThingsGateway.Blazor.Diagrams.Core.Routers;

namespace ThingsGateway.Blazor.Diagrams.Core.Options;

public class DiagramLinkOptions
{
    private double _snappingRadius = 50;

    public Router DefaultRouter { get; set; } = new NormalRouter();
    public PathGenerator DefaultPathGenerator { get; set; } = new SmoothPathGenerator();
    public bool EnableSnapping { get; set; } = false;
    public bool RequireTarget { get; set; } = true;

    public double SnappingRadius
    {
        get => _snappingRadius;
        set
        {
            if (value <= 0)
                throw new ArgumentException($"SnappingRadius must be greater than zero");

            _snappingRadius = value;
        }
    }

    public LinkFactory Factory { get; set; } = (diagram, source, targetAnchor) =>
    {
        Anchor sourceAnchor = source switch
        {
            NodeModel node => new ShapeIntersectionAnchor(node),
            PortModel port => new SinglePortAnchor(port),
            _ => throw new NotImplementedException()
        };
        if (sourceAnchor is SinglePortAnchor sourcePortAnchor &&
        sourcePortAnchor.Model is PortModel sourcePortModel &&
        sourcePortModel.Alignment == PortAlignment.Bottom
        )
        {

            if (targetAnchor.Model == null || (targetAnchor is SinglePortAnchor targetPortAnchor &&
               targetPortAnchor.Model is PortModel targetPortModel &&
               targetPortModel.Alignment == PortAlignment.Top
           ))
            {
                var link = new LinkModel(sourceAnchor, targetAnchor);
                link.TargetMarker = LinkMarker.Arrow;
                return link;
            }
        }
        return null;
    };

    public AnchorFactory TargetAnchorFactory { get; set; } = (diagram, link, model) =>
    {
        return model switch
        {
            NodeModel node => new ShapeIntersectionAnchor(node),
            PortModel port => new SinglePortAnchor(port),
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };
    };
}