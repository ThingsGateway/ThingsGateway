namespace ThingsGateway.Blazor.Diagrams.Core.Events;

public record TouchEventArgs(TouchPoint[] ChangedTouches, bool CtrlKey, bool ShiftKey, bool AltKey);
public record TouchPoint(long Identifier, double ClientX, double ClientY);
