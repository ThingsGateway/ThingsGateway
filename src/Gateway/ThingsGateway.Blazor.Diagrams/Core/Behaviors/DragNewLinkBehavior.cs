using ThingsGateway.Blazor.Diagrams.Core.Anchors;
using ThingsGateway.Blazor.Diagrams.Core.Events;
using ThingsGateway.Blazor.Diagrams.Core.Geometry;
using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Behaviors;

public class DragNewLinkBehavior : Behavior
{
    private PositionAnchor? _targetPositionAnchor;

    public BaseLinkModel? OngoingLink { get; private set; }

    public DragNewLinkBehavior(Diagram diagram) : base(diagram)
    {
        Diagram.PointerDown += OnPointerDown;
        Diagram.PointerMove += OnPointerMove;
        Diagram.PointerUp += OnPointerUp;
    }

    public void StartFrom(ILinkable source, double clientX, double clientY)
    {
        if (OngoingLink != null)
            return;

        _targetPositionAnchor = new PositionAnchor(CalculateTargetPosition(clientX, clientY));
        OngoingLink = Diagram.Options.Links.Factory(Diagram, source, _targetPositionAnchor);
        if (OngoingLink == null)
            return;

        Diagram.Links.Add(OngoingLink);
    }

    public void StartFrom(BaseLinkModel link, double clientX, double clientY)
    {
        if (OngoingLink != null)
            return;

        _targetPositionAnchor = new PositionAnchor(CalculateTargetPosition(clientX, clientY));
        OngoingLink = link;
        OngoingLink.SetTarget(_targetPositionAnchor);
        OngoingLink.Refresh();
        OngoingLink.RefreshLinks();
    }

    private void OnPointerDown(Model? model, MouseEventArgs e)
    {
        if (e.Button != (int)MouseEventButton.Left)
            return;

        OngoingLink = null;
        _targetPositionAnchor = null;

        if (model is PortModel port)
        {
            if (port.Locked)
                return;

            _targetPositionAnchor = new PositionAnchor(CalculateTargetPosition(e.ClientX, e.ClientY));
            OngoingLink = Diagram.Options.Links.Factory(Diagram, port, _targetPositionAnchor);
            if (OngoingLink == null)
                return;

            OngoingLink.SetTarget(_targetPositionAnchor);
            Diagram.Links.Add(OngoingLink);
        }
    }

    private void OnPointerMove(Model? model, MouseEventArgs e)
    {
        if (OngoingLink == null || model != null)
            return;

        _targetPositionAnchor!.SetPosition(CalculateTargetPosition(e.ClientX, e.ClientY));

        if (Diagram.Options.Links.EnableSnapping)
        {
            var nearPort = FindNearPortToAttachTo();
            if ((nearPort != null && nearPort?.Alignment == PortAlignment.Top) || OngoingLink.Target is not PositionAnchor)
            {
                OngoingLink.SetTarget(nearPort is null ? _targetPositionAnchor : new SinglePortAnchor(nearPort));
            }
        }

        OngoingLink.Refresh();
        OngoingLink.RefreshLinks();
    }

    private void OnPointerUp(Model? model, MouseEventArgs e)
    {
        if (OngoingLink == null)
            return;

        if (OngoingLink.IsAttached) // Snapped already
        {
            OngoingLink.TriggerTargetAttached();
            OngoingLink = null;
            return;
        }

        if (model is ILinkable linkable && (OngoingLink.Source.Model == null || OngoingLink.Source.Model.CanAttachTo(linkable)))
        {
            var targetAnchor = Diagram.Options.Links.TargetAnchorFactory(Diagram, OngoingLink, linkable);
            if (targetAnchor is SinglePortAnchor singlePortAnchor && singlePortAnchor.Model is PortModel portModel && portModel.Alignment == PortAlignment.Top)
            {
                OngoingLink.SetTarget(targetAnchor);
                OngoingLink.TriggerTargetAttached();
                OngoingLink.Refresh();
                OngoingLink.RefreshLinks();
            }
            else
            {
                Diagram.Links.Remove(OngoingLink);
            }
        }
        else if (Diagram.Options.Links.RequireTarget)
        {
            Diagram.Links.Remove(OngoingLink);
        }
        else if (!Diagram.Options.Links.RequireTarget)
        {
            OngoingLink.Refresh();
        }

        OngoingLink = null;
    }

    private Point CalculateTargetPosition(double clientX, double clientY)
    {
        var target = Diagram.GetRelativeMousePoint(clientX, clientY);

        if (OngoingLink == null)
        {
            return target;
        }

        var source = OngoingLink.Source.GetPlainPosition()!;
        var dirVector = target.Subtract(source).Normalize();
        var change = dirVector.Multiply(5);
        return target.Subtract(change);
    }

    private PortModel? FindNearPortToAttachTo()
    {
        if (OngoingLink is null || _targetPositionAnchor is null)
            return null;

        PortModel? nearestSnapPort = null;
        var nearestSnapPortDistance = double.PositiveInfinity;

        var position = _targetPositionAnchor!.GetPosition(OngoingLink)!;

        foreach (var port in Diagram.Nodes.SelectMany((NodeModel n) => n.Ports))
        {
            var distance = position.DistanceTo(port.Position);

            if (distance <= Diagram.Options.Links.SnappingRadius && (OngoingLink.Source.Model?.CanAttachTo(port) != false))
            {
                if (distance < nearestSnapPortDistance)
                {
                    nearestSnapPortDistance = distance;
                    nearestSnapPort = port;
                }
            }
        }

        return nearestSnapPort;
    }

    public override void Dispose()
    {
        Diagram.PointerDown -= OnPointerDown;
        Diagram.PointerMove -= OnPointerMove;
        Diagram.PointerUp -= OnPointerUp;
    }
}