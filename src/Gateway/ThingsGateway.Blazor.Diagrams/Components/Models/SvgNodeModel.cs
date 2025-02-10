using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;

namespace ThingsGateway.Blazor.Diagrams.Models;

public class SvgNodeModel : NodeModel
{
    public SvgNodeModel(Point? position = null) : base(position)
    {
    }

    public SvgNodeModel(string id, Point? position = null) : base(id, position)
    {
    }
}