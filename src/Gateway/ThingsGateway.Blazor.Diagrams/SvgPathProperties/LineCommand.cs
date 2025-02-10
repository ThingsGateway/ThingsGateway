using SvgPathProperties.Base;

namespace SvgPathProperties
{
    public class LineCommand : ICommand
    {
        public LineCommand(double fromX, double toX, double fromY, double toY, bool closePath = false)
        {
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;
            ClosePath = closePath;
            Length = Math.Sqrt(Math.Pow(FromX - ToX, 2) + Math.Pow(FromY - ToY, 2));
        }

        public double FromX { get; }
        public double FromY { get; }
        public double ToX { get; }
        public double ToY { get; }
        public bool ClosePath { get; }
        public double Length { get; }

        public Point GetPointAtLength(double pos)
        {
            var fraction = pos / Length;
            fraction = Double.IsNaN(fraction) ? 1 : fraction;
            var newDeltaX = (ToX - FromX) * fraction;
            var newDeltaY = (ToY - FromY) * fraction;
            return new Point(x: FromX + newDeltaX, y: FromY + newDeltaY);
        }

        public PointProperties GetPropertiesAtLength(double pos)
        {
            var point = GetPointAtLength(pos);
            var tangent = GetTangentAtLength(pos);
            return new PointProperties(x: point.X, y: point.Y, tangentX: tangent.X, tangentY: tangent.Y);
        }

        public Rect GetBBox()
        {
            var minX = Math.Min(FromX, ToX);
            var minY = Math.Min(FromY, ToY);
            var maxX = Math.Max(FromX, ToX);
            var maxY = Math.Max(FromY, ToY);
            return new Rect(minX, minY, maxX, maxY);
        }

        public Point GetTangentAtLength(double pos)
        {
            var module = Math.Sqrt((ToX - FromX) * (ToX - FromX) + (ToY - FromY) * (ToY - FromY));
            return new Point(x: (ToX - FromX) / module, y: (ToY - FromY) / module);
        }

        public override string ToString()
        {
            return ClosePath ? "Z" : FormattableString.Invariant($"L {ToX} {ToY}");
        }
    }
}
