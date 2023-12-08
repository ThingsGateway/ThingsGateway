#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Microsoft.AspNetCore.Components;

using System.Reflection;

using ThingsGateway.Foundation.Adapter.OPCDA.Rcw;

#if Plugin

using ThingsGateway.Plugin.OPCDA;

#endif

namespace ThingsGateway.Foundation.Demo;

/// <summary>
/// 导入变量
/// </summary>
public partial class OPCDAImportVariable
{
    private List<BrowseElement> actived = new();

    private ItemProperty[] nodeAttributes;

    private List<OPCDATagModel> Nodes = new();

    private bool overlay = true;

    /// <summary>
    /// opc对象
    /// </summary>
    [Parameter]
    public ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient PLC { get; set; }

    private List<BrowseElement> Actived
    {
        get => actived;
        set
        {
            if (actived?.FirstOrDefault() != value?.FirstOrDefault() && value?.Count > 0)
            {
                actived = value;
                nodeAttributes = actived.FirstOrDefault().Properties;
            }
        }
    }

    private List<BrowseElement> Selected { get; set; } = new();

#if Plugin

    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    public (CollectDevice, List<DeviceVariable>) GetImportVariableList()
    {
        var device = GetImportDevice();
        //动态加载子项时，导出内容需要添加手动加载代码
        foreach (var node in Selected.ToList())
        {
            var nodes = PopulateBranch(node.ItemName, true);
            if (nodes.Count > 0)
            {
                Selected.AddRange(nodes.SelectMany(a => a.GetAllTags()).Select(a => a.Tag).Where(a => a != null).ToList());
            }
        }
        var data = Selected.Select(a =>
        {
            if (!a.IsItem || string.IsNullOrEmpty(a.ItemName))
            {
                return null;
            }

            ProtectTypeEnum level = ProtectTypeEnum.ReadOnly;
            try
            {
                var userAccessLevel = (accessRights)(a.Properties.FirstOrDefault(b => b.ID.Code == 5).Value);
                level = userAccessLevel == accessRights.readable ? userAccessLevel == accessRights.writable ? ProtectTypeEnum.WriteOnly : ProtectTypeEnum.ReadOnly : ProtectTypeEnum.ReadWrite;
            }
            catch
            {
            }

            var id = Yitter.IdGenerator.YitIdHelper.NextId();
            return new DeviceVariable()
            {
                Name = a.Name + "-" + id,
                Address = a.ItemName,
                DeviceId = device.Id,
                Id = id,
                ProtectTypeEnum = level,
                IntervalTime = 1000,
                RpcWriteEnable = true,
            };
        }).Where(a => a != null).ToList();
        return (device, data);
    }

    private CollectDevice GetImportDevice()
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();
        var data = new CollectDevice()
        {
            Name = PLC.OPCNode.OPCName + "-" + id,
            Id = id,
            Enable = true,
            IsLogOut = true,
            DevicePropertys = new(),
            PluginName = "ThingsGateway.Plugin.OPCDA.OPCDAClient",
        };
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCDAClientProperty.OPCName), Value = PLC.OPCNode.OPCName, Description = typeof(OPCDAClientProperty).GetProperty(nameof(OPCDAClientProperty.OPCName)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCDAClientProperty.OPCIP), Value = PLC.OPCNode.OPCIP, Description = typeof(OPCDAClientProperty).GetProperty(nameof(OPCDAClientProperty.OPCIP)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCDAClientProperty.ActiveSubscribe), Value = PLC.OPCNode.ActiveSubscribe.ToString(), Description = typeof(OPCDAClientProperty).GetProperty(nameof(OPCDAClientProperty.ActiveSubscribe)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCDAClientProperty.CheckRate), Value = PLC.OPCNode.CheckRate.ToString(), Description = typeof(OPCDAClientProperty).GetProperty(nameof(OPCDAClientProperty.CheckRate)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCDAClientProperty.DeadBand), Value = PLC.OPCNode.DeadBand.ToString(), Description = typeof(OPCDAClientProperty).GetProperty(nameof(OPCDAClientProperty.DeadBand)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCDAClientProperty.GroupSize), Value = PLC.OPCNode.GroupSize.ToString(), Description = typeof(OPCDAClientProperty).GetProperty(nameof(OPCDAClientProperty.GroupSize)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCDAClientProperty.UpdateRate), Value = PLC.OPCNode.UpdateRate.ToString(), Description = typeof(OPCDAClientProperty).GetProperty(nameof(OPCDAClientProperty.UpdateRate)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        return data;
    }

#endif

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await Task.Factory.StartNew(async () =>
        {
            Nodes = PopulateBranch("");
            overlay = false;
            await InvokeAsync(StateHasChanged);
        });
        await base.OnInitializedAsync();
    }

    private List<OPCDATagModel> PopulateBranch(string sourceId, bool isAll = false)
    {
        List<OPCDATagModel> nodes = new()
        {
            new OPCDATagModel() { Name = "Browsering..." }
        };
        try
        {
            var references = PLC.GetBrowseElements(sourceId);
            List<OPCDATagModel> list = new();
            if (references != null)
            {
                for (int ii = 0; ii < references.Count; ii++)
                {
                    var target = references[ii];
                    OPCDATagModel child = new()
                    {
                        Name = target.Name,
                        Tag = target
                    };

                    if (target.HasChildren)
                    {
                        if (isAll)
                            child.Nodes = PopulateBranch(target.ItemName);
                        else
                            child.Nodes = new();
                    }
                    else
                    {
                        child.Nodes = null;
                    }

                    list.Add(child);
                }
            }

            List<OPCDATagModel> listNode = list;
            nodes.Clear();
            nodes.AddRange(listNode.ToArray());
            return nodes;
        }
        catch (Exception ex)
        {
            return new()
            {
                new()
                {
                    Name = ex.Message,
                    Tag = new(),
                    Nodes = null
                }
            };
        }
    }

    private async Task PopulateBranchAsync(OPCDATagModel model)
    {
        await Task.Run(() =>
       {
           var sourceId = model.Tag.ItemName;
           model.Nodes = PopulateBranch(sourceId);
       });
    }

    internal class OPCDATagModel
    {
        internal string Name { get; set; }
        internal string NodeId => (Tag?.ItemName)?.ToString();
        internal List<OPCDATagModel> Nodes { get; set; } = new();
        internal BrowseElement Tag { get; set; }

        public List<OPCDATagModel> GetAllTags()
        {
            List<OPCDATagModel> allTags = new();
            GetAllTagsRecursive(this, allTags);
            return allTags;
        }

        private void GetAllTagsRecursive(OPCDATagModel parentTag, List<OPCDATagModel> allTags)
        {
            allTags.Add(parentTag);
            if (parentTag.Nodes != null)
                foreach (OPCDATagModel childTag in parentTag.Nodes)
                {
                    GetAllTagsRecursive(childTag, allTags);
                }
        }
    }
}