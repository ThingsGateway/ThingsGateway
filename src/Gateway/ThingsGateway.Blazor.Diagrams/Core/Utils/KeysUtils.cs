using System.Text;

namespace ThingsGateway.Blazor.Diagrams.Core.Utils;

public static class KeysUtils
{
    public static string GetStringRepresentation(bool ctrl, bool shift, bool alt, string key)
    {
        var sb = new StringBuilder();

        if (ctrl) sb.Append("Ctrl+");
        if (shift) sb.Append("Shift+");
        if (alt) sb.Append("Alt+");
        sb.Append(key);

        return sb.ToString();
    }
}
