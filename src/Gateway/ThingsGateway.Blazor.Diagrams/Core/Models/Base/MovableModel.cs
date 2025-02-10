using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Blazor.Diagrams.Core.Models.Base;

// I'm assuming that all movable models (nodes & groups for now) are also selectable,
// I believe it makes sense since if you click to move something then you're also selecting
public abstract class MovableModel : SelectableModel
{
    public event Action<MovableModel>? Moved;

    public MovableModel(Point? position = null)
    {
        Position = position ?? Point.Zero;
    }

    public MovableModel(string id, Point? position = null) : base(id)
    {
        Position = position ?? Point.Zero;
    }

    public Point Position { get; set; }

    public virtual void SetPosition(double x, double y) => Position = new Point(x, y);

    /// <summary>
    /// Only use this if you know what you're doing
    /// </summary>
    public void TriggerMoved() => Moved?.Invoke(this);
}
