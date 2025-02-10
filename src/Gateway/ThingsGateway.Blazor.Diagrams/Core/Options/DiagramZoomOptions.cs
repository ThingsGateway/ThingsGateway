namespace ThingsGateway.Blazor.Diagrams.Core.Options;

public class DiagramZoomOptions
{
    private double _minimum = 0.1;
    private double _scaleFactor = 1.05;

    public bool Enabled { get; set; } = true;
    public bool Inverse { get; set; }
    public double Minimum
    {
        get => _minimum;
        set
        {
            if (value <= 0)
                throw new ArgumentException($"Minimum can't be less than zero");

            _minimum = value;
        }
    }
    public double Maximum { get; set; } = 2;
    public double ScaleFactor
    {
        get => _scaleFactor;
        set
        {
            if (value is < 1.01 or > 2)
                throw new ArgumentException($"ScaleFactor can't be lower than 1.01 or greater than 2");

            _scaleFactor = value;
        }
    }
}