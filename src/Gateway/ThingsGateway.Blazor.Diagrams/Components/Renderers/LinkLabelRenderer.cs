using Microsoft.AspNetCore.Components.Rendering;

using SvgPathProperties;

using ThingsGateway.Blazor.Diagrams.Core.Extensions;
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Components.Renderers;

public class LinkLabelRenderer : ComponentBase, IDisposable
{
    [CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;
    [Parameter] public LinkLabelModel Label { get; set; } = null!;
    [Parameter] public SvgPath Path { get; set; } = null!;

    public void Dispose()
    {
        Label.Changed -= OnLabelChanged;
        Label.VisibilityChanged -= OnLabelChanged;
    }

    protected override void OnInitialized()
    {
        Label.Changed += OnLabelChanged;
        Label.VisibilityChanged += OnLabelChanged;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Label.Visible)
            return;

        var position = FindPosition();
        if (position == null)
            return;

        var componentType = BlazorDiagram.GetComponent(Label) ?? typeof(DefaultLinkLabelWidget);

        builder.OpenElement(0, "foreignObject");
        builder.AddAttribute(1, "class", "diagram-link-label");
        builder.AddAttribute(2, "x", (position.X + (Label.Offset?.X ?? 0)).ToInvariantString());
        builder.AddAttribute(3, "y", (position.Y + (Label.Offset?.Y ?? 0)).ToInvariantString());

        builder.OpenComponent(4, componentType);
        builder.AddAttribute(5, "Label", Label);
        builder.CloseComponent();

        builder.CloseElement();
    }

    private void OnLabelChanged(Model _)
    {
        InvokeAsync(StateHasChanged);
    }

    private Point? FindPosition()
    {
        var totalLength = Path.Length;
        var length = Label.Distance switch
        {
            <= 1 and >= 0 => Label.Distance.Value * totalLength,
            > 1 => Label.Distance.Value,
            < 0 => totalLength + Label.Distance.Value,
            _ => totalLength * (Label.Parent.Labels.IndexOf(Label) + 1) / (Label.Parent.Labels.Count + 1)
        };

        var pt = Path.GetPointAtLength(length);
        return new Point(pt.X, pt.Y);
    }
}