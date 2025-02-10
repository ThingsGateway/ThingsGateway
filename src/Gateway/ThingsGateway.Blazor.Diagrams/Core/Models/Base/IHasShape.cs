using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Blazor.Diagrams.Core.Models.Base;

public interface IHasShape
{
    public IShape GetShape();
}