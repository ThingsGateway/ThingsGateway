namespace SvgPathProperties
{
    public static class CurveMinMax
    {
        private const double CbezierMinmaxEpsilon = 0.00000001;

        public static double[] MinMaxQ(double[] A)
        {
            var min = Math.Min(A[0], A[2]);
            var max = Math.Max(A[0], A[2]);

            if (A[1] > A[0] ? A[2] >= A[1] : A[2] <= A[1])
            {
                // if no extremum in ]0,1[
                return new[] { min, max };
            }

            // check if the extremum E is min or max
            var E = (A[0] * A[2] - A[1] * A[1]) / (A[0] - 2 * A[1] + A[2]);
            return E < min ? new[] { E, max } : new[] { min, E };
        }

        public static double[] MinMaxC(double[] A)
        {
            var K = A[0] - 3 * A[1] + 3 * A[2] - A[3];

            // if the polynomial is (almost) quadratic and not cubic
            if (Math.Abs(K) < CbezierMinmaxEpsilon)
            {
                if (A[0] == A[3] && A[0] == A[1])
                {
                    // no curve, point targeting same location
                    return new[] { A[0], A[3] };
                }

                return MinMaxQ(new[]
                {
                    A[0],
                    -0.5 * A[0] + 1.5 * A[1],
                    A[0] - 3.0 * A[1] + 3.0 * A[2],
                });
            }

            // the reduced discriminant of the derivative
            var T =
                -A[0] * A[2] +
                A[0] * A[3] -
                A[1] * A[2] -
                A[1] * A[3] +
                A[1] * A[1] +
                A[2] * A[2];

            // if the polynomial is monotone in [0,1]
            if (T <= 0)
            {
                return new[] { Math.Min(A[0], A[3]), Math.Max(A[0], A[3]) };
            }

            var S = Math.Sqrt(T);

            // potential extrema
            var min = Math.Min(A[0], A[3]);
            var max = Math.Max(A[0], A[3]);

            var L = A[0] - 2 * A[1] + A[2];
            // check local extrema
            for (double R = (L + S) / K, i = 1.0; i <= 2.0; R = (L - S) / K, i++)
            {
                if (R > 0 && R < 1)
                {
                    // if the extrema is for R in [0,1]
                    var Q =
                        A[0] * (1.0 - R) * (1 - R) * (1.0 - R) +
                        A[1] * 3.0 * (1.0 - R) * (1.0 - R) * R +
                        A[2] * 3.0 * (1.0 - R) * R * R +
                        A[3] * R * R * R;
                    if (Q < min)
                    {
                        min = Q;
                    }

                    if (Q > max)
                    {
                        max = Q;
                    }
                }
            }

            return new[] { min, max };
        }
    }
}