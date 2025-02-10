using System.Diagnostics;

using ThingsGateway.Blazor.Diagrams.Core.Events;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Behaviors;

public class EventsBehavior : Behavior
{
    private readonly Stopwatch _mouseClickSw;
    private Model? _model;
    private bool _captureMouseMove;
    private int _mouseMovedCount;

    public EventsBehavior(Diagram diagram) : base(diagram)
    {
        _mouseClickSw = new Stopwatch();

        Diagram.PointerDown += OnPointerDown;
        Diagram.PointerMove += OnPointerMove;
        Diagram.PointerUp += OnPointerUp;
        Diagram.PointerClick += OnPointerClick;
    }

    private void OnPointerClick(Model? model, PointerEventArgs e)
    {
        if (_mouseClickSw.IsRunning && _mouseClickSw.ElapsedMilliseconds <= 500)
        {
            Diagram.TriggerPointerDoubleClick(model, e);
        }

        _mouseClickSw.Restart();
    }

    private void OnPointerDown(Model? model, PointerEventArgs e)
    {
        _captureMouseMove = true;
        _mouseMovedCount = 0;
        _model = model;
    }

    private void OnPointerMove(Model? model, PointerEventArgs e)
    {
        if (!_captureMouseMove)
            return;

        _mouseMovedCount++;
    }

    private void OnPointerUp(Model? model, PointerEventArgs e)
    {
        if (!_captureMouseMove) return; // Only set by OnMouseDown
        _captureMouseMove = false;
        if (_mouseMovedCount > 0) return;

        if (_model == model)
        {
            Diagram.TriggerPointerClick(model, e);
            _model = null;
        }
    }

    public override void Dispose()
    {
        Diagram.PointerDown -= OnPointerDown;
        Diagram.PointerMove -= OnPointerMove;
        Diagram.PointerUp -= OnPointerUp;
        Diagram.PointerClick -= OnPointerClick;
        _model = null;
    }
}
