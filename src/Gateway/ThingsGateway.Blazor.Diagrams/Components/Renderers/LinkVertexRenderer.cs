using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

using ThingsGateway.Blazor.Diagrams.Core.Extensions;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
using ThingsGateway.Blazor.Diagrams.Extensions;

namespace ThingsGateway.Blazor.Diagrams.Components.Renderers;

public class LinkVertexRenderer : ComponentBase, IDisposable
{
    private bool _shouldRender = true;

    [CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;
    [Parameter] public LinkVertexModel Vertex { get; set; } = null!;
    [Parameter] public string? Color { get; set; }
    [Parameter] public string? SelectedColor { get; set; }

    private string? ColorToUse => Vertex.Selected ? SelectedColor : Color;

    public void Dispose()
    {
        Vertex.Changed -= OnVertexChanged;
    }

    protected override void OnInitialized()
    {
        Vertex.Changed += OnVertexChanged;
    }

    protected override bool ShouldRender()
    {
        if (!_shouldRender) return false;

        _shouldRender = false;
        return true;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var componentType = BlazorDiagram.GetComponent(Vertex);

        builder.OpenElement(0, "g");
        builder.AddAttribute(1, "class", "diagram-link-vertex");
        builder.AddAttribute(4, "cursor", "move");
        builder.AddAttribute(5, "ondblclick", value: EventCallback.Factory.Create<MouseEventArgs>(this, OnDoubleClick));
        builder.AddAttribute(6, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerDown));
        builder.AddAttribute(7, "onpointerup", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerUp));
        builder.AddEventStopPropagationAttribute(8, "onpointerdown", true);
        builder.AddEventStopPropagationAttribute(9, "onpointerup", true);

        if (componentType == null)
        {
            builder.OpenElement(10, "circle");
            builder.AddAttribute(11, "cx", Vertex.Position.X.ToInvariantString());
            builder.AddAttribute(12, "cy", Vertex.Position.Y.ToInvariantString());
            builder.AddAttribute(13, "r", "5");
            builder.AddAttribute(14, "fill", ColorToUse);
            builder.CloseElement();
        }
        else
        {
            builder.OpenComponent(15, componentType);
            builder.AddAttribute(16, "Vertex", Vertex);
            builder.AddAttribute(17, "Color", ColorToUse);
            builder.CloseComponent();
        }

        builder.CloseElement();
    }

    private void OnVertexChanged(Model _)
    {
        _shouldRender = true;
        InvokeAsync(StateHasChanged);
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        BlazorDiagram.TriggerPointerDown(Vertex, e.ToCore());
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        BlazorDiagram.TriggerPointerUp(Vertex, e.ToCore());
    }

    private void OnDoubleClick(MouseEventArgs e)
    {
        Vertex.Parent.Vertices.Remove(Vertex);
        Vertex.Parent.Refresh();
    }
}
