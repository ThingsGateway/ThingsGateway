using SvgPathProperties;

using ThingsGateway.Blazor.Diagrams.Core.Geometry;

namespace ThingsGateway.Blazor.Diagrams.Core;

public class PathGeneratorResult
{
    public PathGeneratorResult(SvgPath fullPath, SvgPath[] paths, double? sourceMarkerAngle = null, Point? sourceMarkerPosition = null,
        double? targetMarkerAngle = null, Point? targetMarkerPosition = null)
    {
        FullPath = fullPath;
        Paths = paths;
        SourceMarkerAngle = sourceMarkerAngle;
        SourceMarkerPosition = sourceMarkerPosition;
        TargetMarkerAngle = targetMarkerAngle;
        TargetMarkerPosition = targetMarkerPosition;
    }

    public SvgPath FullPath { get; }
    public SvgPath[] Paths { get; }
    public double? SourceMarkerAngle { get; }
    public Point? SourceMarkerPosition { get; }
    public double? TargetMarkerAngle { get; }
    public Point? TargetMarkerPosition { get; }
}
