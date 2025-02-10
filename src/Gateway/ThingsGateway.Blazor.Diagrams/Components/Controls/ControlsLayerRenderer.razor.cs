using Microsoft.AspNetCore.Components.Web;

using ThingsGateway.Blazor.Diagrams.Core.Controls;
using ThingsGateway.Blazor.Diagrams.Core.Extensions;
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
using ThingsGateway.Blazor.Diagrams.Extensions;

namespace ThingsGateway.Blazor.Diagrams.Components.Controls;

public partial class ControlsLayerRenderer : IDisposable
{
    private bool _shouldRender;

    [CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;

    [Parameter] public bool Svg { get; set; }

    public void Dispose()
    {
        BlazorDiagram.Controls.ChangeCaused -= OnControlsChangeCaused;
    }

    protected override void OnInitialized()
    {
        BlazorDiagram.Controls.ChangeCaused += OnControlsChangeCaused;
    }

    protected override bool ShouldRender()
    {
        if (!_shouldRender)
            return false;

        _shouldRender = false;
        return true;
    }

    private void OnControlsChangeCaused(Model cause)
    {
        if (Svg != cause.IsSvg())
            return;

        _shouldRender = true;
        InvokeAsync(StateHasChanged);
    }

    private RenderFragment RenderControl(Model model, Control control, Point position, bool svg)
    {
        var componentType = BlazorDiagram.GetComponent(control.GetType());
        if (componentType == null)
            throw new BlazorDiagramsException(
                $"A component couldn't be found for the user action {control.GetType().Name}");

        return builder =>
        {
            builder.OpenElement(0, svg ? "g" : "div");
            builder.AddAttribute(1, "class",
                $"{(control is ExecutableControl ? "executable " : "")}diagram-control {control.GetType().Name}");
            if (svg)
                builder.AddAttribute(2, "transform",
                    $"translate({position.X.ToInvariantString()} {position.Y.ToInvariantString()})");
            else
                builder.AddAttribute(2, "style",
                    $"top: {position.Y.ToInvariantString()}px; left: {position.X.ToInvariantString()}px");

            if (control is ExecutableControl ec)
            {
                builder.AddAttribute(3, "onpointerdown",
                    EventCallback.Factory.Create<PointerEventArgs>(this, e => OnPointerDown(e, model, ec)));
                builder.AddEventStopPropagationAttribute(4, "onpointerdown", true);
            }

            builder.OpenComponent(5, componentType);
            builder.AddAttribute(6, "Control", control);
            builder.AddAttribute(7, "Model", model);
            builder.CloseComponent();
            builder.CloseElement();
        };
    }

    private async Task OnPointerDown(PointerEventArgs e, Model model, ExecutableControl control)
    {
        if (e.Button == 0 || e.Buttons == 1) await control.OnPointerDown(BlazorDiagram, model, e.ToCore()).ConfigureAwait(false);
    }
}
