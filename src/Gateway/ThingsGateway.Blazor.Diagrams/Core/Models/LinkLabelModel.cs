using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Models;

public class LinkLabelModel : Model
{
    public LinkLabelModel(BaseLinkModel parent, string id, string content, double? distance = null, Point? offset = null) : base(id)
    {
        Parent = parent;
        Content = content;
        Distance = distance;
        Offset = offset;
    }

    public LinkLabelModel(BaseLinkModel parent, string content, double? distance = null, Point? offset = null)
    {
        Parent = parent;
        Content = content;
        Distance = distance;
        Offset = offset;
    }

    public BaseLinkModel Parent { get; }
    public string Content { get; set; }
    /// <summary>
    /// 3 types of values are possible:
    /// <para>- A number between 0 and 1: Position relative to the link's length</para>
    /// <para>- A positive number, greater than 1: Position away from the start</para>
    /// <para>- A negative number, less than 0: Position away from the end</para>
    /// </summary>
    public double? Distance { get; set; }
    public Point? Offset { get; set; }
}
