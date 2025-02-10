namespace SvgPathProperties.Base
{
    public struct PointProperties
    {
        public double X { get; }
        public double Y { get; }
        public double TangentX { get; }
        public double TangentY { get; }

        public PointProperties(double x, double y, double tangentX, double tangentY)
        {
            X = x;
            Y = y;
            TangentX = tangentX;
            TangentY = tangentY;
        }
    }
}
