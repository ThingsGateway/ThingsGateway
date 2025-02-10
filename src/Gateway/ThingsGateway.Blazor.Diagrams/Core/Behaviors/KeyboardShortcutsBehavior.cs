using ThingsGateway.Blazor.Diagrams.Core.Events;
using ThingsGateway.Blazor.Diagrams.Core.Utils;

namespace ThingsGateway.Blazor.Diagrams.Core.Behaviors;

public class CaseInsensitiveComparer : IEqualityComparer<string>
{
    public bool Equals(string x, string y)
    {
        // 比较两个字符串，忽略大小写
        return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string obj)
    {
        // 生成不区分大小写的哈希码
        return obj == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
    }
}

public class KeyboardShortcutsBehavior : Behavior
{
    private readonly Dictionary<string, Func<Diagram, ValueTask>> _shortcuts;

    public KeyboardShortcutsBehavior(Diagram diagram) : base(diagram)
    {
        _shortcuts = new Dictionary<string, Func<Diagram, ValueTask>>(10000, new CaseInsensitiveComparer());
        SetShortcut("Delete", false, false, false, KeyboardShortcutsDefaults.DeleteSelection);
        SetShortcut("g", true, false, true, KeyboardShortcutsDefaults.Grouping);

        Diagram.KeyDown += OnDiagramKeyDown;
    }

    public void SetShortcut(string key, bool ctrl, bool shift, bool alt, Func<Diagram, ValueTask> action)
    {
        var k = KeysUtils.GetStringRepresentation(ctrl, shift, alt, key);
        _shortcuts[k] = action;
    }

    public bool RemoveShortcut(string key, bool ctrl, bool shift, bool alt)
    {
        var k = KeysUtils.GetStringRepresentation(ctrl, shift, alt, key);
        return _shortcuts.Remove(k);
    }

    private async void OnDiagramKeyDown(KeyboardEventArgs e)
    {
        var k = KeysUtils.GetStringRepresentation(e.CtrlKey, e.ShiftKey, e.AltKey, e.Key);
        if (_shortcuts.TryGetValue(k, out var action))
        {
            await action(Diagram).ConfigureAwait(false);
        }
    }

    public override void Dispose()
    {
        Diagram.KeyDown -= OnDiagramKeyDown;
    }
}
