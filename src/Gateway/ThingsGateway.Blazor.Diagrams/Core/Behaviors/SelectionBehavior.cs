using ThingsGateway.Blazor.Diagrams.Core.Events;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Behaviors;

public class SelectionBehavior : Behavior
{
    public SelectionBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
    }

    private void OnPointerDown(Model? model, PointerEventArgs e)
    {
        var ctrlKey = e.CtrlKey;
        switch (model)
        {
            case null:
                Diagram.UnselectAll();
                break;
            case SelectableModel sm when ctrlKey && sm.Selected:
                Diagram.UnselectModel(sm);
                break;
            case SelectableModel sm:
                {
                    if (!sm.Selected)
                    {
                        Diagram.SelectModel(sm, !ctrlKey || !Diagram.Options.AllowMultiSelection);
                    }

                    break;
                }
        }
    }

    public override void Dispose()
    {
        Diagram.PointerDown -= OnPointerDown;
    }
}
