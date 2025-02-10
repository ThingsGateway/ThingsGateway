using SvgPathProperties.Base;

namespace SvgPathProperties
{
    public static class BezierFunctions
    {
        public static Point CubicPoint(double[] xs, double[] ys, double t)
        {
            var x =
              (1 - t) * (1 - t) * (1 - t) * xs[0] +
              3 * (1 - t) * (1 - t) * t * xs[1] +
              3 * (1 - t) * t * t * xs[2] +
              t * t * t * xs[3];
            var y =
              (1 - t) * (1 - t) * (1 - t) * ys[0] +
              3 * (1 - t) * (1 - t) * t * ys[1] +
              3 * (1 - t) * t * t * ys[2] +
              t * t * t * ys[3];

            return new Point(x, y);
        }

        public static Point CubicDerivative(double[] xs, double[] ys, double t)
        {
            return QuadraticPoint(
                new[] { 3 * (xs[1] - xs[0]), 3 * (xs[2] - xs[1]), 3 * (xs[3] - xs[2]) },
                new[] { 3 * (ys[1] - ys[0]), 3 * (ys[2] - ys[1]), 3 * (ys[3] - ys[2]) },
                t
            );
        }

        public static double GetCubicArcLength(double[] xs, double[] ys, double t)
        {
            double z;
            double sum;
            double correctedT;

            /*if (xs.length >= tValues.length) {
                  throw new Error('too high n bezier');
                }*/

            var n = 20;

            z = t / 2;
            sum = 0;
            for (var i = 0; i < n; i++)
            {
                correctedT = z * BezierValues.TValues[n][i] + z;
                sum += BezierValues.CValues[n][i] * BFunc(xs, ys, correctedT);
            }
            return z * sum;
        }

        public static Point QuadraticPoint(double[] xs, double[] ys, double t)
        {
            var x = (1 - t) * (1 - t) * xs[0] + 2 * (1 - t) * t * xs[1] + t * t * xs[2];
            var y = (1 - t) * (1 - t) * ys[0] + 2 * (1 - t) * t * ys[1] + t * t * ys[2];
            return new Point(x, y);
        }

        public static double GetQuadraticArcLength(double[] xs, double[] ys, double t)
        {
            //  if (t === undefined) {
            //    t = 1; TODO
            //}
            var ax = xs[0] - 2 * xs[1] + xs[2];
            var ay = ys[0] - 2 * ys[1] + ys[2];
            var bx = 2 * xs[1] - 2 * xs[0];
            var by = 2 * ys[1] - 2 * ys[0];

            var A = 4 * (ax * ax + ay * ay);
            var B = 4 * (ax * bx + ay * by);
            var C = bx * bx + by * by;

            if (A == 0)
                return t * Math.Sqrt(Math.Pow(xs[2] - xs[0], 2) + Math.Pow(ys[2] - ys[0], 2));

            var b = B / (2 * A);
            var c = C / A;
            var u = t + b;
            var k = c - b * b;

            var uuk = u * u + k > 0 ? Math.Sqrt(u * u + k) : 0;
            var bbk = b * b + k > 0 ? Math.Sqrt(b * b + k) : 0;
            var term = b + Math.Sqrt(b * b + k) != 0 ? k * Math.Log(Math.Abs((u + uuk) / (b + bbk))) : 0;
            return (Math.Sqrt(A) / 2) * (u * uuk - b * bbk + term);
        }

        public static Point QuadraticDerivative(double[] xs, double[] ys, double t)
        {
            return new Point((1 - t) * 2 * (xs[1] - xs[0]) + t * 2 * (xs[2] - xs[1]),
                (1 - t) * 2 * (ys[1] - ys[0]) + t * 2 * (ys[2] - ys[1]));
        }

        public static double BFunc(double[] xs, double[] ys, double t)
        {
            var xbase = GetDerivative(1, t, xs);
            var ybase = GetDerivative(1, t, ys);
            var combined = xbase * xbase + ybase * ybase;
            return Math.Sqrt(combined);
        }

        public static double GetDerivative(double derivative, double t, double[] vs)
        {
            // the derivative of any 't'-less function is zero.
            var n = vs.Length - 1;
            double[] _vs;
            double value;

            if (n == 0)
                return 0;

            // direct values? compute!
            if (derivative == 0)
            {
                value = 0;
                for (var k = 0; k <= n; k++)
                {
                    value += BezierValues.BinomialCoefficients[n][k] * Math.Pow(1 - t, n - k) * Math.Pow(t, k) * vs[k];
                }
                return value;
            }
            else
            {
                // Still some derivative? go down one order, then try
                // for the lower order curve's.
                _vs = new double[n];
                for (var k = 0; k < n; k++)
                {
                    _vs[k] = n * (vs[k + 1] - vs[k]);
                }
                return GetDerivative(derivative - 1, t, _vs);
            }
        }

        public static double T2length(double length, double totalLength, Func<double, double> func)
        {
            double error = 1;
            var t = length / totalLength;
            var step = (length - func(t)) / totalLength;

            var numIterations = 0;
            while (error > 0.001)
            {
                var increasedTLength = func(t + step);
                var increasedTError = Math.Abs(length - increasedTLength) / totalLength;
                if (increasedTError < error)
                {
                    error = increasedTError;
                    t += step;
                }
                else
                {
                    var decreasedTLength = func(t - step);
                    var decreasedTError = Math.Abs(length - decreasedTLength) / totalLength;
                    if (decreasedTError < error)
                    {
                        error = decreasedTError;
                        t -= step;
                    }
                    else
                    {
                        step /= 2;
                    }
                }

                numIterations++;
                if (numIterations > 500)
                {
                    break;
                }
            }

            return t;
        }
    }
}
