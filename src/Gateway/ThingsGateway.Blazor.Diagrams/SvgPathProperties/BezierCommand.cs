using SvgPathProperties.Base;

namespace SvgPathProperties
{
    public class BezierCommand : ICommand
    {
        private readonly Func<double[], double[], double, double> _getArcLength;
        private readonly Func<double[], double[], double, Point> _getPoint;
        private readonly Func<double[], double[], double, Point> _getDerivative;

        public BezierCommand(double ax, double ay, double bx, double by, double cx, double cy, double? dx, double? dy)
        {
            From = new Point(ax, ay);
            Cp1 = new Point(bx, by);
            Cp2OrEnd = new Point(cx, cy);

            if (dx != null && dy != null)
            {
                _getArcLength = BezierFunctions.GetCubicArcLength;
                _getPoint = BezierFunctions.CubicPoint;
                _getDerivative = BezierFunctions.CubicDerivative;
                End = new Point(dx.Value, dy.Value);
            }
            else
            {
                _getArcLength = BezierFunctions.GetQuadraticArcLength;
                _getPoint = BezierFunctions.QuadraticPoint;
                _getDerivative = BezierFunctions.QuadraticDerivative;
                End = new Point(0, 0);
            }

            Length = _getArcLength(new[] { From.X, Cp1.X, Cp2OrEnd.X, End.X }, new[] { From.Y, Cp1.Y, Cp2OrEnd.Y, End.Y }, 1);
        }

        public Point From { get; }
        public Point Cp1 { get; }
        public Point Cp2OrEnd { get; }
        public Point End { get; }
        public double Length { get; }
        public bool IsQuadratic => End.X == 0 && End.Y == 0;

        public Point GetPointAtLength(double length)
        {
            var xs = new[] { From.X, Cp1.X, Cp2OrEnd.X, End.X };
            var xy = new[] { From.Y, Cp1.Y, Cp2OrEnd.Y, End.Y };
            var t = BezierFunctions.T2length(length, Length, i => _getArcLength(xs, xy, i));
            return _getPoint(xs, xy, t);
        }

        public PointProperties GetPropertiesAtLength(double length)
        {
            var xs = new[] { From.X, Cp1.X, Cp2OrEnd.X, End.X };
            var xy = new[] { From.Y, Cp1.Y, Cp2OrEnd.Y, End.Y };
            var t = BezierFunctions.T2length(length, Length, i => _getArcLength(xs, xy, i));

            var derivative = _getDerivative(xs, xy, t);
            var mdl = Math.Sqrt(derivative.X * derivative.X + derivative.Y * derivative.Y);
            Point tangent;
            if (mdl > 0)
            {
                tangent = new Point(x: derivative.X / mdl, y: derivative.Y / mdl);
            }
            else
            {
                tangent = new Point(0, 0);
            }

            var point = _getPoint(xs, xy, t);
            return new PointProperties(x: point.X, y: point.Y, tangentX: tangent.X, tangentY: tangent.Y);
        }

        public Rect GetBBox()
        {
            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;
            var x = From.X;
            var y = From.Y;

            if (IsQuadratic)
            {
                var qxMinMax = CurveMinMax.MinMaxQ(new[] { x, Cp1.X, Cp2OrEnd.X });
                if (minX > qxMinMax[0])
                {
                    minX = qxMinMax[0];
                }

                if (maxX < qxMinMax[1])
                {
                    maxX = qxMinMax[1];
                }

                var qyMinMax = CurveMinMax.MinMaxQ(new[] { y, Cp1.Y, Cp2OrEnd.Y });
                if (minY > qyMinMax[0])
                {
                    minY = qyMinMax[0];
                }

                if (maxY < qyMinMax[1])
                {
                    maxY = qyMinMax[1];
                }
            }
            else
            {
                var cxMinMax = CurveMinMax.MinMaxC(new[] { x, Cp1.X, Cp2OrEnd.X, End.X });
                if (minX > cxMinMax[0])
                {
                    minX = cxMinMax[0];
                }

                if (maxX < cxMinMax[1])
                {
                    maxX = cxMinMax[1];
                }

                var cyMinMax = CurveMinMax.MinMaxC(new[] { y, Cp1.Y, Cp2OrEnd.Y, End.Y });
                if (minY > cyMinMax[0])
                {
                    minY = cyMinMax[0];
                }

                if (maxY < cyMinMax[1])
                {
                    maxY = cyMinMax[1];
                }
            }

            return new Rect(minX, minY, maxX, maxY);
        }

        public Point GetTangentAtLength(double length)
        {
            var xs = new[] { From.X, Cp1.X, Cp2OrEnd.X, End.X };
            var xy = new[] { From.Y, Cp1.Y, Cp2OrEnd.Y, End.Y };
            var t = BezierFunctions.T2length(length, Length, i => _getArcLength(xs, xy, i));

            var derivative = _getDerivative(xs, xy, t);
            var mdl = Math.Sqrt(derivative.X * derivative.X + derivative.Y * derivative.Y);
            if (mdl > 0)
            {
                return new Point(x: derivative.X / mdl, y: derivative.Y / mdl);
            }
            else
            {
                return new Point(0, 0);
            }
        }

        public override string ToString()
        {
            if (IsQuadratic)
            {
                return FormattableString.Invariant($"Q {Cp1.X} {Cp1.Y} {Cp2OrEnd.X} {Cp2OrEnd.Y}");
            }
            else
            {
                return FormattableString.Invariant($"C {Cp1.X} {Cp1.Y} {Cp2OrEnd.X} {Cp2OrEnd.Y} {End.X} {End.Y}");
            }
        }
    }
}