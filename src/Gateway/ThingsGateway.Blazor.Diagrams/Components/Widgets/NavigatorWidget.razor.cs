using Microsoft.AspNetCore.Components.Rendering;

using ThingsGateway.Blazor.Diagrams.Core.Extensions;
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Components.Widgets;

public partial class NavigatorWidget : IDisposable
{
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private double _scaledMargin;
    private double _vX;
    private double _vY;
    private double _vWidth;
    private double _vHeight;

    [CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;
    [Parameter] public bool UseNodeShape { get; set; } = true;
    [Parameter] public double Width { get; set; }
    [Parameter] public double Height { get; set; }
    [Parameter] public double Margin { get; set; } = 5;
    [Parameter] public string NodeColor { get; set; } = "#40babd";
    [Parameter] public string GroupColor { get; set; } = "#9fd0d1";
    [Parameter] public string ViewStrokeColor { get; set; } = "#40babd";
    [Parameter] public int ViewStrokeWidth { get; set; } = 4;
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    public void Dispose()
    {
        BlazorDiagram.Changed -= Refresh;
        BlazorDiagram.Nodes.Added -= OnNodeAdded;
        BlazorDiagram.Nodes.Removed -= OnNodeRemoved;
        BlazorDiagram.Groups.Added -= OnNodeAdded;
        BlazorDiagram.Groups.Removed -= OnNodeRemoved;

        foreach (var node in BlazorDiagram.Nodes)
            node.Changed -= OnNodeChanged;

        foreach (var group in BlazorDiagram.Groups)
            group.Changed -= OnNodeChanged;
    }

    protected override void OnInitialized()
    {
        BlazorDiagram.Changed += Refresh;
        BlazorDiagram.Nodes.Added += OnNodeAdded;
        BlazorDiagram.Nodes.Removed += OnNodeRemoved;
        BlazorDiagram.Groups.Added += OnNodeAdded;
        BlazorDiagram.Groups.Removed += OnNodeRemoved;

        foreach (var node in BlazorDiagram.Nodes)
            node.Changed += OnNodeChanged;

        foreach (var group in BlazorDiagram.Groups)
            group.Changed += OnNodeChanged;
    }

    private void OnNodeAdded(NodeModel node) => node.Changed += OnNodeChanged;

    private void OnNodeRemoved(NodeModel node) => node.Changed -= OnNodeChanged;

    private void OnNodeChanged(Model _) => Refresh();

    private void Refresh()
    {
        if (BlazorDiagram.Container == null)
            return;

        _vX = -BlazorDiagram.Pan.X / BlazorDiagram.Zoom;
        _vY = -BlazorDiagram.Pan.Y / BlazorDiagram.Zoom;
        _vWidth = BlazorDiagram.Container.Width / BlazorDiagram.Zoom;
        _vHeight = BlazorDiagram.Container.Height / BlazorDiagram.Zoom;

        var minX = _vX;
        var minY = _vY;
        var maxX = _vX + _vWidth;
        var maxY = _vY + _vHeight;

        foreach (var node in BlazorDiagram.Nodes.Union(BlazorDiagram.Groups))
        {
            if (node.Size == null)
                continue;

            minX = Math.Min(minX, node.Position.X);
            minY = Math.Min(minY, node.Position.Y);
            maxX = Math.Max(maxX, node.Position.X + node.Size.Width);
            maxY = Math.Max(maxY, node.Position.Y + node.Size.Height);
        }

        var width = maxX - minX;
        var height = maxY - minY;
        var scaledWidth = width / Width;
        var scaledHeight = height / Height;
        var scale = Math.Max(scaledWidth, scaledHeight);
        var viewWidth = scale * Width;
        var viewHeight = scale * Height;

        _scaledMargin = Margin * scale;
        _x = minX - (viewWidth - width) / 2 - _scaledMargin;
        _y = minY - (viewHeight - height) / 2 - _scaledMargin;
        _width = viewWidth + _scaledMargin * 2;
        _height = viewHeight + _scaledMargin * 2;
        InvokeAsync(StateHasChanged);
    }

    private RenderFragment GetNodeRenderFragment(NodeModel node)
    {
        return builder =>
        {
            if (UseNodeShape)
            {
                var shape = node.GetShape();
                if (shape is Ellipse ellipse)
                {
                    RenderEllipse(node, builder, ellipse);
                    return;
                }
            }

            RenderRect(node, builder);
        };
    }

    private void RenderRect(NodeModel node, RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "rect");
        builder.SetKey(node);
        builder.AddAttribute(1, "class", "navigator-node");
        builder.AddAttribute(2, "fill", NodeColor);
        builder.AddAttribute(2, "x", node.Position.X.ToInvariantString());
        builder.AddAttribute(2, "y", node.Position.Y.ToInvariantString());
        builder.AddAttribute(2, "width", node.Size!.Width.ToInvariantString());
        builder.AddAttribute(2, "height", node.Size.Height.ToInvariantString());
        builder.CloseElement();
    }

    private void RenderEllipse(NodeModel node, RenderTreeBuilder builder, Ellipse ellipse)
    {
        builder.OpenElement(0, "ellipse");
        builder.SetKey(node);
        builder.AddAttribute(1, "class", "navigator-node");
        builder.AddAttribute(2, "fill", NodeColor);
        builder.AddAttribute(2, "cx", ellipse.Cx.ToInvariantString());
        builder.AddAttribute(2, "cy", ellipse.Cy.ToInvariantString());
        builder.AddAttribute(2, "rx", ellipse.Rx.ToInvariantString());
        builder.AddAttribute(2, "ry", ellipse.Ry.ToInvariantString());
        builder.CloseElement();
    }
}