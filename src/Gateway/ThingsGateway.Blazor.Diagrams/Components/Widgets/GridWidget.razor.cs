using System.Text;

using ThingsGateway.Blazor.Diagrams.Core.Extensions;

namespace ThingsGateway.Blazor.Diagrams.Components.Widgets;

public partial class GridWidget : IDisposable
{
    private bool _visible;
    private double _scaledSize;
    private double _posX;
    private double _posY;

    [CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;
    [Parameter] public double Size { get; set; } = 20;
    [Parameter] public double ZoomThreshold { get; set; } = 0;
    [Parameter] public GridMode Mode { get; set; } = GridMode.Line;
    [Parameter] public string BackgroundColor { get; set; } = "rgb(241 241 241)";

    public void Dispose()
    {
        BlazorDiagram.PanChanged -= RefreshPosition;
        BlazorDiagram.ZoomChanged -= RefreshPosition;
    }

    protected override void OnInitialized()
    {
        BlazorDiagram.PanChanged += RefreshPosition;
        BlazorDiagram.ZoomChanged += RefreshPosition;
    }

    protected override void OnParametersSet()
    {
        _posX = BlazorDiagram.Pan.X;
        _posY = BlazorDiagram.Pan.Y;
        _scaledSize = Size * BlazorDiagram.Zoom;
        _visible = BlazorDiagram.Zoom > ZoomThreshold;
    }

    private void RefreshPosition()
    {
        _posX = BlazorDiagram.Pan.X;
        _posY = BlazorDiagram.Pan.Y;
        _scaledSize = Size * BlazorDiagram.Zoom;
        _visible = BlazorDiagram.Zoom > ZoomThreshold;
        InvokeAsync(StateHasChanged);
    }

    private string GenerateStyle()
    {
        var sb = new StringBuilder();

        sb.Append($"background-color: {BackgroundColor};");
        sb.Append($"background-size: {_scaledSize.ToInvariantString()}px {_scaledSize.ToInvariantString()}px;");
        sb.Append($"background-position-x: {_posX.ToInvariantString()}px;");
        sb.Append($"background-position-y: {_posY.ToInvariantString()}px;");

        switch (Mode)
        {
            case GridMode.Line:
                sb.Append("background-image: linear-gradient(rgb(211, 211, 211) 1px, transparent 1px), linear-gradient(90deg, rgb(211, 211, 211) 1px, transparent 1px);");
                break;
            case GridMode.Point:
                sb.Append("background-image: radial-gradient(circle at 0 0, rgb(129, 129, 129) 1px, transparent 1px);");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sb.ToString();
    }
}

public enum GridMode
{
    Line,
    Point
}