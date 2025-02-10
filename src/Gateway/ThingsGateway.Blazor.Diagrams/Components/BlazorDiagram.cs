using ThingsGateway.Blazor.Diagrams.Components.Controls;
using ThingsGateway.Blazor.Diagrams.Core;
using ThingsGateway.Blazor.Diagrams.Core.Controls.Default;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;
using ThingsGateway.Blazor.Diagrams.Options;

namespace ThingsGateway.Blazor.Diagrams;

public class BlazorDiagram : Diagram
{
    private readonly Dictionary<Type, Type> _componentsMapping;

    public BlazorDiagram(BlazorDiagramOptions? options = null, bool registerDefaultBehaviors = true) : base(registerDefaultBehaviors)
    {
        _componentsMapping = new Dictionary<Type, Type>
        {
            [typeof(RemoveControl)] = typeof(RemoveControlWidget),
            [typeof(BoundaryControl)] = typeof(BoundaryControlWidget),
            [typeof(DragNewLinkControl)] = typeof(DragNewLinkControlWidget),
            [typeof(ArrowHeadControl)] = typeof(ArrowHeadControlWidget)
        };

        Options = options ?? new BlazorDiagramOptions();
    }

    public override BlazorDiagramOptions Options { get; }

    public void RegisterComponent<TModel, TComponent>(bool replace = false)
    {
        RegisterComponent(typeof(TModel), typeof(TComponent), replace);
    }

    public void RegisterComponent(Type modelType, Type componentType, bool replace = false)
    {
        if (!replace && _componentsMapping.ContainsKey(modelType))
            throw new Exception($"Component already registered for model '{modelType.Name}'.");

        _componentsMapping[modelType] = componentType;
    }

    public Type? GetComponent(Type modelType, bool checkSubclasses = true)
    {
        if (_componentsMapping.TryGetValue(modelType, out Type? value))
            return value;

        if (!checkSubclasses)
            return null;

        foreach (var rmt in _componentsMapping.Keys)
        {
            if (modelType.IsSubclassOf(rmt))
                return _componentsMapping[rmt];
        }

        return null;
    }

    public Type? GetComponent<TModel>(bool checkSubclasses = true)
    {
        return GetComponent(typeof(TModel), checkSubclasses);
    }

    public Type? GetComponent(Model model, bool checkSubclasses = true)
    {
        return GetComponent(model.GetType(), checkSubclasses);
    }
}