using ThingsGateway.Blazor.Diagrams.Core.Models;
using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core.Behaviors;

public static class KeyboardShortcutsDefaults
{
    public static async ValueTask DeleteSelection(Diagram diagram)
    {
        var wasSuspended = diagram.SuspendRefresh;
        if (!wasSuspended) diagram.SuspendRefresh = true;

        foreach (var sm in diagram.GetSelectedModels().ToArray())
        {
            if (sm.Locked)
                continue;

            if (sm is GroupModel group && (await diagram.Options.Constraints.ShouldDeleteGroup(group).ConfigureAwait(false)))
            {
                diagram.Groups.Delete(group);
            }
            else if (sm is NodeModel node && (await diagram.Options.Constraints.ShouldDeleteNode(node).ConfigureAwait(false)))
            {
                diagram.Nodes.Remove(node);
            }
            else if (sm is BaseLinkModel link && (await diagram.Options.Constraints.ShouldDeleteLink(link).ConfigureAwait(false)))
            {
                diagram.Links.Remove(link);
            }
        }

        if (!wasSuspended)
        {
            diagram.SuspendRefresh = false;
            diagram.Refresh();
        }
    }

    public static ValueTask Grouping(Diagram diagram)
    {
        if (!diagram.Options.Groups.Enabled)
            return ValueTask.CompletedTask;

        if (!diagram.GetSelectedModels().Any())
            return ValueTask.CompletedTask;

        var selectedNodes = diagram.Nodes.Where(n => n.Selected).ToArray();
        var nodesWithGroup = selectedNodes.Where(n => n.Group != null).ToArray();
        if (nodesWithGroup.Length > 0)
        {
            // Ungroup
            foreach (var group in nodesWithGroup.GroupBy(n => n.Group!).Select(g => g.Key))
            {
                diagram.Groups.Remove(group);
            }
        }
        else
        {
            // Group
            if (selectedNodes.Length < 2)
                return ValueTask.CompletedTask;

            if (selectedNodes.Any(n => n.Group != null))
                return ValueTask.CompletedTask;

            diagram.Groups.Group(selectedNodes);
        }

        return ValueTask.CompletedTask;
    }
}
