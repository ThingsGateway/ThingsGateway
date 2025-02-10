using System.Text.Json.Serialization;

namespace ThingsGateway.Blazor.Diagrams.Core.Geometry;

public class Rectangle : IShape
{
    public static Rectangle Zero { get; } = new(0, 0, 0, 0);

    public double Width { get; }
    public double Height { get; }
    public double Top { get; }
    public double Right { get; }
    public double Bottom { get; }
    public double Left { get; }

    [JsonConstructor]
    public Rectangle(double left, double top, double right, double bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        Width = Math.Abs(Left - Right);
        Height = Math.Abs(Top - Bottom);
    }

    public Rectangle(Point position, Size size)
    {
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        ArgumentNullException.ThrowIfNull(size, nameof(size));

        Left = position.X;
        Top = position.Y;
        Right = Left + size.Width;
        Bottom = Top + size.Height;
        Width = size.Width;
        Height = size.Height;
    }

    public bool Overlap(Rectangle r)
        => Left < r.Right && Right > r.Left && Top < r.Bottom && Bottom > r.Top;

    public bool Intersects(Rectangle r)
    {
        var thisX = Left;
        var thisY = Top;
        var thisW = Width;
        var thisH = Height;
        var rectX = r.Left;
        var rectY = r.Top;
        var rectW = r.Width;
        var rectH = r.Height;
        return rectX < thisX + thisW && thisX < rectX + rectW && rectY < thisY + thisH && thisY < rectY + rectH;
    }

    public Rectangle Inflate(double horizontal, double vertical)
        => new(Left - horizontal, Top - vertical, Right + horizontal, Bottom + vertical);

    public Rectangle Union(Rectangle r)
    {
        var x1 = Math.Min(Left, r.Left);
        var x2 = Math.Max(Left + Width, r.Left + r.Width);
        var y1 = Math.Min(Top, r.Top);
        var y2 = Math.Max(Top + Height, r.Top + r.Height);
        return new(x1, y1, x2, y2);
    }

    public bool ContainsPoint(Point point) => ContainsPoint(point.X, point.Y);

    public bool ContainsPoint(double x, double y)
        => x >= Left && x <= Right && y >= Top && y <= Bottom;

    public IEnumerable<Point> GetIntersectionsWithLine(Line line)
    {
        var borders = new[] {
            new Line(NorthWest, NorthEast),
            new Line(NorthEast, SouthEast),
            new Line(SouthWest, SouthEast),
            new Line(NorthWest, SouthWest)
        };

        for (var i = 0; i < borders.Length; i++)
        {
            var intersectionPt = borders[i].GetIntersection(line);
            if (intersectionPt != null)
                yield return intersectionPt;
        }
    }

    public Point? GetPointAtAngle(double a)
    {
        var vx = Math.Cos(a * Math.PI / 180);
        var vy = Math.Sin(a * Math.PI / 180);
        var px = Left + Width / 2;
        var py = Top + Height / 2;
        double? t1 = (Left - px) / vx; // left
        double? t2 = (Right - px) / vx; // right
        double? t3 = (Top - py) / vy; // top
        double? t4 = (Bottom - py) / vy; // bottom
        var t = (new[] { t1, t2, t3, t4 }).Where(n => n.HasValue && double.IsFinite(n.Value) && n.Value > 0).DefaultIfEmpty(null).Min();
        if (t == null) return null;

        var x = px + t.Value * vx;
        var y = py + t.Value * vy;
        return new Point(x, y);
    }

    public Point Center => new(Left + Width / 2, Top + Height / 2);
    public Point NorthEast => new(Right, Top);
    public Point SouthEast => new(Right, Bottom);
    public Point SouthWest => new(Left, Bottom);
    public Point NorthWest => new(Left, Top);
    public Point East => new(Right, Top + Height / 2);
    public Point North => new(Left + Width / 2, Top);
    public Point South => new(Left + Width / 2, Bottom);
    public Point West => new(Left, Top + Height / 2);

    public bool Equals(Rectangle? other)
    {
        return other != null && Left == other.Left && Right == other.Right && Top == other.Top &&
            Bottom == other.Bottom && Width == other.Width && Height == other.Height;
    }

    public override string ToString()
                => $"Rectangle(width={Width}, height={Height}, top={Top}, right={Right}, bottom={Bottom}, left={Left})";
}
