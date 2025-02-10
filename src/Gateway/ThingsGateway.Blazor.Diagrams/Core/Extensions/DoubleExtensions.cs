namespace ThingsGateway.Blazor.Diagrams.Core.Extensions;

public static class DoubleExtensions
{
    public static bool AlmostEqualTo(this double double1, double double2, double tolerance = 0.0001)
        => Math.Abs(double1 - double2) < tolerance;
}
