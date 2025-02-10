namespace SvgPathProperties.Base
{
    public class PartProperties
    {
        public Point Start { get; }
        public Point End { get; }
        public double Length { get; }
        public ICommand Properties { get; }

        public PartProperties(Point start, Point end, double length, ICommand properties)
        {
            Start = start;
            End = end;
            Length = length;
            Properties = properties;
        }
    }
}
