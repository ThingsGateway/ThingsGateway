namespace ThingsGateway.Blazor.Diagrams.Core.Events;

public record PointerEventArgs(double ClientX, double ClientY, long Button, long Buttons, bool CtrlKey, bool ShiftKey,
    bool AltKey, long PointerId, float Width, float Height, float Pressure, float TiltX, float TiltY,
    string PointerType, bool IsPrimary) : MouseEventArgs(ClientX, ClientY, Button, Buttons, CtrlKey, ShiftKey, AltKey);