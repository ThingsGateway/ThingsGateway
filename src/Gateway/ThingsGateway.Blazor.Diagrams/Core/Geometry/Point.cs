namespace ThingsGateway.Blazor.Diagrams.Core.Geometry;

public record Point
{
    public static Point Zero { get; } = new(0, 0);

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; init; }
    public double Y { get; init; }

    public double Length => Math.Sqrt(Dot(this));

    public double Dot(Point other) => X * other.X + Y * other.Y;
    public Point Lerp(Point other, double t)
        => new(X * (1.0 - t) + other.X * t, Y * (1.0 - t) + other.Y * t);

    public Point Add(double value) => new(X + value, Y + value);
    public Point Add(double x, double y) => new(X + x, Y + y);

    public Point Subtract(double value) => new(X - value, Y - value);
    public Point Subtract(double x, double y) => new(X - x, Y - y);
    public Point Subtract(Point other) => new(X - other.X, Y - other.Y);

    public Point Divide(Point other) => new(X / other.X, Y / other.Y);

    public Point Multiply(double value) => new(X * value, Y * value);

    public Point Normalize()
    {
        var length = Length;
        return new Point(X / length, Y / length);
    }

    public double DistanceTo(Point other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
    public double DistanceTo(double x, double y) => Math.Sqrt(Math.Pow(X - x, 2) + Math.Pow(Y - y, 2));

    public Point MoveAlongLine(Point from, double dist)
    {
        var x = X - from.X;
        var y = Y - from.Y;
        var angle = Math.Atan2(y, x);
        var xOffset = Math.Cos(angle) * dist;
        var yOffset = Math.Sin(angle) * dist;
        return new Point(X + xOffset, Y + yOffset);
    }

    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

    public void Deconstruct(out double x, out double y)
    {
        x = X;
        y = Y;
    }
}
