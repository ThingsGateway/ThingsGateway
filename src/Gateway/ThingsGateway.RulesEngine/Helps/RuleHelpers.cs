// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using System.Reflection;

using ThingsGateway.Blazor.Diagrams.Core.Anchors;

namespace ThingsGateway.RulesEngine;

internal static class RuleHelpers
{
    /// <summary>
    /// 构造选择项，ID/Name
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildRulesSelectList(this IEnumerable<Rules> items)
    {
        var data = items
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Name)
            {
            }
        ).ToList();
        return data;
    }

    public static readonly Dictionary<Type, CategoryNode> CategoryNodeDict = new();
    public static List<IGrouping<string, KeyValuePair<Type, CategoryNode>>> CategoryNodeGroups = new();
    static RuleHelpers()
    {
        GetDict();
    }
    private static void GetDict()
    {
        foreach (var item in App.EffectiveTypes)
        {
            var categoryNode = item.GetCustomAttribute<CategoryNode>();
            if (categoryNode != null)
            {
                categoryNode.StringLocalizer = App.CreateLocalizerByType(categoryNode.LocalizerType);
                CategoryNodeDict.Add(item, categoryNode);
            }
        }
        CategoryNodeGroups = RuleHelpers.CategoryNodeDict.GroupBy(a => a.Value.Category).ToList();
    }

    internal static NodeModel GetNodeModel(string key, string id, Point point)
    {
        var type = Type.GetType(key.Contains('.') ? key :
                    $"ThingsGateway.RulesEngine.{key}");
        if (type != null)
        {
            switch (key)
            {
                case "ThingsGateway.RulesEngine.StartNode":
                    {
                        var node = new StartNode(id, point);
                        node.AddPort(PortAlignment.Bottom);
                        return node;
                    }
                case "ThingsGateway.RulesEngine.EndNode":
                    {
                        var node = new EndNode(id, point);
                        node.AddPort(PortAlignment.Top);
                        return node;
                    }
                default:
                    {
                        var node = Activator.CreateInstance(type, id, point) as NodeModel;
                        node.AddPort(PortAlignment.Top);
                        node.AddPort(PortAlignment.Bottom);
                        return node;

                    }
            }
        }
        else
        {
            return null;
        }
    }
    internal static Dictionary<string, JToken> GetModelValue(NodeModel nodeModel)
    {
        Dictionary<string, JToken> jtokens = new();
        var propertyInfos = nodeModel.GetType().GetRuntimeProperties().Where(a => a.GetCustomAttribute<ModelValue>() != null);
        foreach (var item in propertyInfos)
        {
            jtokens.Add(item.Name, JToken.FromObject(item.GetValue(nodeModel) ?? JValue.CreateNull()));
        }
        return jtokens;
    }
    internal static void SetModelValue(NodeModel nodeModel, JObject jobject)
    {
        if (nodeModel == null)
            return;
        if (jobject == null)
            return;
        var propertyInfos = nodeModel?.GetType().GetRuntimeProperties().ToDictionary(a => a.Name);
        foreach (var item in jobject ?? new())
        {
            if (propertyInfos.TryGetValue(item.Key, out var propertyInfo))
            {
                propertyInfo.SetValue(nodeModel, item.Value?.ToObject(propertyInfo.PropertyType));
            }
        }
    }


    internal static NodeModel OnNodeJson(BlazorDiagram blazorDiagram, string draggedType, string id, Point point)
    {
        NodeModel node = RuleHelpers.GetNodeModel(draggedType, id, point);
        if (node != null)
            blazorDiagram.Nodes.Add(node);
        return node;
    }
    internal static RulesJson Save(BlazorDiagram blazorDiagram)
    {
        var rules = new RulesJson();
        foreach (var item in blazorDiagram.Nodes)
        {
            NodeJson nodeJson = new();
            rules.NodeJsons.Add(nodeJson);
            nodeJson.DraggedType = item.GetType().FullName;
            nodeJson.Point = item.Position;
            nodeJson.Id = item.Id;
            foreach (var keyValuePair in GetModelValue(item))
            {
                nodeJson.CValues.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
        foreach (var item in blazorDiagram.Links)
        {
            LinkJson linkJson = new();
            rules.LinkJsons.Add(linkJson);

            linkJson.SourcePortAnchor.PortAlignment = ((PortModel)item.Source.Model).Alignment;
            linkJson.SourcePortAnchor.NodelId = ((PortModel)item.Source.Model).Parent.Id;
            linkJson.TargetPortAnchor.PortAlignment = ((PortModel)item.Target.Model).Alignment;
            linkJson.TargetPortAnchor.NodelId = ((PortModel)item.Target.Model).Parent.Id;
        }
        return rules;
    }

    internal static void Load(BlazorDiagram blazorDiagram, RulesJson rules)
    {
        blazorDiagram.Nodes.Clear();
        blazorDiagram.Links.Clear();
        foreach (var item in rules.NodeJsons)
        {
            var nodeModel = OnNodeJson(blazorDiagram, item.DraggedType, item.Id, item.Point);

            SetModelValue(nodeModel, item.CValues);

        }
        foreach (var item in rules.LinkJsons)
        {
            var source = blazorDiagram.Nodes.FirstOrDefault(a => a.Id == item.SourcePortAnchor.NodelId).Ports.FirstOrDefault(a => a.Alignment == item.SourcePortAnchor.PortAlignment);
            var target = blazorDiagram.Nodes.FirstOrDefault(a => a.Id == item.TargetPortAnchor.NodelId).Ports.FirstOrDefault(a => a.Alignment == item.TargetPortAnchor.PortAlignment);

            var linkModel = blazorDiagram.Options.Links.Factory(blazorDiagram, source, new SinglePortAnchor(target));
            if (linkModel == null)
                continue;
            blazorDiagram.Links.Add(linkModel);

        }

        blazorDiagram.Refresh();
    }

}