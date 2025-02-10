using System.Collections;

using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Controls;

public class ControlsContainer : IReadOnlyList<Control>
{
    private readonly List<Control> _controls = new(4);

    public event Action<Model>? VisibilityChanged;
    public event Action<Model>? ControlsChanged;

    public ControlsContainer(Model model, ControlsType type = ControlsType.OnSelection)
    {
        Model = model;
        Type = type;
    }

    public Model Model { get; }
    public ControlsType Type { get; set; }
    public bool Visible { get; private set; }

    public void Show()
    {
        if (Visible)
            return;

        Visible = true;
        VisibilityChanged?.Invoke(Model);
    }

    public void Hide()
    {
        if (!Visible)
            return;

        Visible = false;
        VisibilityChanged?.Invoke(Model);
    }

    public ControlsContainer Add(Control control)
    {
        _controls.Add(control);
        ControlsChanged?.Invoke(Model);
        return this;
    }

    public ControlsContainer Remove(Control control)
    {
        if (_controls.Remove(control))
        {
            ControlsChanged?.Invoke(Model);
        }

        return this;
    }

    public int Count => _controls.Count;
    public Control this[int index] => _controls[index];
    public IEnumerator<Control> GetEnumerator() => _controls.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _controls.GetEnumerator();
}