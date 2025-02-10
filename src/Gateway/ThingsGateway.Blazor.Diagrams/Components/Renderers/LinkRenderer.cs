using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

using System.Text;

using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
using ThingsGateway.Blazor.Diagrams.Extensions;

namespace ThingsGateway.Blazor.Diagrams.Components.Renderers;

public class LinkRenderer : ComponentBase, IDisposable
{
    private bool _shouldRender = true;

    [CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;

    [Parameter] public BaseLinkModel Link { get; set; } = null!;

    public void Dispose()
    {
        Link.Changed -= OnLinkChanged;
        Link.VisibilityChanged -= OnLinkChanged;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Link.Changed += OnLinkChanged;
        Link.VisibilityChanged += OnLinkChanged;
    }

    protected override bool ShouldRender()
    {
        if (!_shouldRender)
            return false;

        _shouldRender = false;
        return true;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Link.Visible)
            return;

        var componentType = BlazorDiagram.GetComponent(Link) ?? typeof(LinkWidget);
        var classes = new StringBuilder()
            .Append("diagram-link")
            .AppendIf(" attached", Link.IsAttached)
            .ToString();

        builder.OpenElement(0, "g");
        builder.AddAttribute(1, "class", classes);
        builder.AddAttribute(2, "data-link-id", Link.Id);
        builder.AddAttribute(3, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerDown));
        builder.AddEventStopPropagationAttribute(4, "onpointerdown", true);
        builder.AddAttribute(5, "onpointerup", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerUp));
        builder.AddEventStopPropagationAttribute(6, "onpointerup", true);
        builder.AddAttribute(7, "onmouseenter", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseEnter));
        builder.AddAttribute(8, "onmouseleave", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseLeave));
        builder.OpenComponent(9, componentType);
        builder.AddAttribute(10, "Link", Link);
        builder.CloseComponent();
        builder.CloseElement();
    }

    private void OnLinkChanged(Model _)
    {
        _shouldRender = true;
        InvokeAsync(StateHasChanged);
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        BlazorDiagram.TriggerPointerDown(Link, e.ToCore());
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        BlazorDiagram.TriggerPointerUp(Link, e.ToCore());
    }

    private void OnMouseEnter(MouseEventArgs e)
    {
        BlazorDiagram.TriggerPointerEnter(Link, e.ToCore());
    }

    private void OnMouseLeave(MouseEventArgs e)
    {
        BlazorDiagram.TriggerPointerLeave(Link, e.ToCore());
    }
}