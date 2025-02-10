using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using System.Text;

using ThingsGateway.Blazor.Diagrams.Core.Extensions;
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
using ThingsGateway.Blazor.Diagrams.Extensions;
using ThingsGateway.Blazor.Diagrams.Models;

namespace ThingsGateway.Blazor.Diagrams.Components.Renderers;

public class NodeRenderer : ComponentBase, IDisposable
{
    private bool _becameVisible;
    private ElementReference _element;
    private bool _isSvg;
    private DotNetObjectReference<NodeRenderer>? _reference;
    private bool _shouldRender;

    [CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;

    [Parameter] public NodeModel Node { get; set; } = null!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    public void Dispose()
    {
        Node.Changed -= OnNodeChanged;
        Node.VisibilityChanged -= OnVisibilityChanged;

        if (_element.Id != null && !Node.ControlledSize)
        {
            _ = JsRuntime.UnobserveResizes(_element);
        }

        _reference?.Dispose();
    }

    [JSInvokable]
    public void OnResize(Size size)
    {
        // When the node becomes invisible (a.k.a unrendered), the size is zero
        if (Size.Zero.Equals(size))
            return;

        size = new Size(size.Width / BlazorDiagram.Zoom, size.Height / BlazorDiagram.Zoom);
        if (Node.Size != null && Node.Size.Width.AlmostEqualTo(size.Width) &&
            Node.Size.Height.AlmostEqualTo(size.Height))
        {
            return;
        }

        Node.Size = size;
        Node.Refresh();
        Node.RefreshLinks();
        Node.ReinitializePorts();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _reference = DotNetObjectReference.Create(this);
        Node.Changed += OnNodeChanged;
        Node.VisibilityChanged += OnVisibilityChanged;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _isSvg = Node is SvgNodeModel;
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
        if (!Node.Visible)
            return;

        var componentType = BlazorDiagram.GetComponent(Node) ??
                            (_isSvg ? typeof(SvgNodeWidget) : typeof(NodeWidget));
        var classes = new StringBuilder("diagram-node")
            .AppendIf(" locked", Node.Locked)
            .AppendIf(" selected", Node.Selected)
            .AppendIf(" grouped", Node.Group != null);

        builder.OpenElement(0, _isSvg ? "g" : "div");
        builder.AddAttribute(1, "class", classes.ToString());
        builder.AddAttribute(2, "data-node-id", Node.Id);

        if (_isSvg)
        {
            builder.AddAttribute(3, "transform",
                $"translate({Node.Position.X.ToInvariantString()} {Node.Position.Y.ToInvariantString()})");
        }
        else
        {
            builder.AddAttribute(3, "style",
                $"top: {Node.Position.Y.ToInvariantString()}px; left: {Node.Position.X.ToInvariantString()}px");
        }

        builder.AddAttribute(4, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerDown));
        builder.AddEventStopPropagationAttribute(5, "onpointerdown", true);
        builder.AddAttribute(6, "onpointerup", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerUp));
        builder.AddEventStopPropagationAttribute(7, "onpointerup", true);
        builder.AddAttribute(8, "onmouseenter", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseEnter));
        builder.AddAttribute(9, "onmouseleave", EventCallback.Factory.Create<MouseEventArgs>(this, OnMouseLeave));
        builder.AddElementReferenceCapture(10, value => _element = value);
        builder.OpenComponent(11, componentType);
        builder.AddAttribute(12, "Node", Node);
        builder.CloseComponent();

        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !Node.Visible)
            return;


        if (firstRender || _becameVisible)
        {
            _becameVisible = false;

            if (!Node.ControlledSize)
            {
                await JsRuntime.ObserveResizes(_element, _reference!).ConfigureAwait(false);
            }
        }
    }

    private void OnNodeChanged(Model _)
    {
        ReRender();
    }

    private void OnVisibilityChanged(Model _)
    {
        _becameVisible = Node.Visible;
        ReRender();
    }

    private void ReRender()
    {
        _shouldRender = true;
        InvokeAsync(StateHasChanged);
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        BlazorDiagram.TriggerPointerDown(Node, e.ToCore());
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        BlazorDiagram.TriggerPointerUp(Node, e.ToCore());
    }

    private void OnMouseEnter(MouseEventArgs e)
    {
        BlazorDiagram.TriggerPointerEnter(Node, e.ToCore());
    }

    private void OnMouseLeave(MouseEventArgs e)
    {
        BlazorDiagram.TriggerPointerLeave(Node, e.ToCore());
    }
}