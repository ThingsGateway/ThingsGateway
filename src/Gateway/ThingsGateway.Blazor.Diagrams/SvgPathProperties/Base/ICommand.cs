namespace SvgPathProperties.Base
{
    public interface ICommand
    {
        double Length { get; }

        Point GetPointAtLength(double pos);
        Point GetTangentAtLength(double pos);
        PointProperties GetPropertiesAtLength(double pos);
        Rect GetBBox();
    }
}
