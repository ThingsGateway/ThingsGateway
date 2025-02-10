using ThingsGateway.Blazor.Diagrams.Core.Models;

namespace ThingsGateway.Blazor.Diagrams.Core.Layers;

public class GroupLayer : BaseLayer<GroupModel>
{
    public GroupLayer(Diagram diagram) : base(diagram)
    {
    }

    public GroupModel Group(params NodeModel[] children)
    {
        return Add(Diagram.Options.Groups.Factory(Diagram, children));
    }

    /// <summary>
    /// Removes the group AND its children
    /// </summary>
    public void Delete(GroupModel group)
    {
        Diagram.Batch(() =>
        {
            var children = group.Children.ToArray();

            Remove(group);

            foreach (var child in children)
            {
                if (child is GroupModel g)
                {
                    Delete(g);
                }
                else
                {
                    Diagram.Nodes.Remove(child);
                }
            }
        });
    }

    protected override void OnItemRemoved(GroupModel group)
    {
        Diagram.Links.Remove(group.PortLinks.ToArray());
        Diagram.Links.Remove(group.Links.ToArray());
        group.Ungroup();
        group.Group?.RemoveChild(group);
    }
}
