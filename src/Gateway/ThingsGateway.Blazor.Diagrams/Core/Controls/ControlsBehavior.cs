using ThingsGateway.Blazor.Diagrams.Core.Events;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Controls;

public class ControlsBehavior : Behavior
{
    public ControlsBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerEnter += OnPointerEnter;
        Diagram.PointerLeave += OnPointerLeave;
        Diagram.SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(SelectableModel model)
    {
        var controls = Diagram.Controls.GetFor(model);
        if (controls is not { Type: ControlsType.OnSelection })
            return;

        if (model.Selected)
        {
            controls.Show();
        }
        else
        {
            controls.Hide();
        }
    }

    private void OnPointerEnter(Model? model, PointerEventArgs e)
    {
        if (model == null)
            return;

        var controls = Diagram.Controls.GetFor(model);
        if (controls is not { Type: ControlsType.OnHover })
            return;

        controls.Show();
    }

    private void OnPointerLeave(Model? model, PointerEventArgs e)
    {
        if (model == null)
            return;

        var controls = Diagram.Controls.GetFor(model);
        if (controls is not { Type: ControlsType.OnHover })
            return;

        controls.Hide();
    }

    public override void Dispose()
    {
        Diagram.PointerEnter -= OnPointerEnter;
        Diagram.PointerLeave -= OnPointerLeave;
        Diagram.SelectionChanged -= OnSelectionChanged;
    }
}