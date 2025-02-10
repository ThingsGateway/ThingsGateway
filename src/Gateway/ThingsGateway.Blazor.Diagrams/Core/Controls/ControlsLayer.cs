using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Controls;

public class ControlsLayer
{
    private readonly Dictionary<Model, ControlsContainer> _containers;

    public event Action<Model>? ChangeCaused;

    public ControlsLayer()
    {
        _containers = new Dictionary<Model, ControlsContainer>();
    }

    public IReadOnlyCollection<Model> Models => _containers.Keys;

    public ControlsContainer AddFor(Model model, ControlsType type = ControlsType.OnSelection)
    {
        if (_containers.TryGetValue(model, out ControlsContainer? value))
            return value;

        var container = new ControlsContainer(model, type);
        container.VisibilityChanged += OnVisibilityChanged;
        container.ControlsChanged += RefreshIfVisible;
        model.Changed += RefreshIfVisible;
        _containers.Add(model, container);
        return container;
    }

    public ControlsContainer? GetFor(Model model)
    {
        return _containers.TryGetValue(model, out var container) ? container : null;
    }

    public bool RemoveFor(Model model)
    {
        if (!_containers.TryGetValue(model, out var container))
            return false;

        container.VisibilityChanged -= OnVisibilityChanged;
        container.ControlsChanged -= RefreshIfVisible;
        model.Changed -= RefreshIfVisible;
        _containers.Remove(model);
        ChangeCaused?.Invoke(model);
        return true;
    }

    public bool AreVisibleFor(Model model) => GetFor(model)?.Visible ?? false;

    private void RefreshIfVisible(Model cause)
    {
        if (!AreVisibleFor(cause))
            return;

        ChangeCaused?.Invoke(cause);
    }

    private void OnVisibilityChanged(Model cause) => ChangeCaused?.Invoke(cause);
}