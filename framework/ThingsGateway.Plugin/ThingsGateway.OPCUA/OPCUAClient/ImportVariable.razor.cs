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

using Opc.Ua;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using ThingsGateway.Admin.Core;
using ThingsGateway.Application;
using ThingsGateway.Foundation.Adapter.OPCUA;

using Yitter.IdGenerator;

namespace ThingsGateway.OPCUA;

/// <summary>
/// 导入变量
/// </summary>
public partial class ImportVariable
{
    private List<ReferenceDescription> actived = new();

    private OPCNodeAttribute[] nodeAttributes;

    private List<OPCUATagModel> Nodes = new();

    private bool overlay = true;
    /// <summary>
    /// opc对象
    /// </summary>
    [Parameter]
    public ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient PLC { get; set; }

    private List<ReferenceDescription> Actived
    {
        get => actived;
        set
        {
            if (actived?.FirstOrDefault() != value?.FirstOrDefault() && value?.Count > 0)
            {
                actived = value;
                if (actived.FirstOrDefault().NodeId != null)
                    nodeAttributes = PLC.ReadNoteAttributes(actived.FirstOrDefault().NodeId.ToString());
            }

        }

    }

    [Inject]
    IDriverPluginService DriverPluginService { get; set; }

    private List<ReferenceDescription> Selected { get; set; } = new();
    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    public async Task<(CollectDevice, List<DeviceVariable>)> GetImportVariableListAsync()
    {
        var device = GetImportDevice();
        //动态加载子项时，导出内容需要添加手动加载代码
        foreach (var node in Selected.ToList())
        {
            List<OPCUATagModel> nodes = await PopulateBranchAsync((NodeId)node.NodeId, true);
            if (nodes.Count > 0)
            {
                Selected.AddRange(nodes.SelectMany(a => a.GetAllTags()).Select(a => a.Tag).Where(a => a != null).ToList());
            }
        }
        var data = (await SelectAsync(Selected, async a =>
        {
            var nodeClass = (await PLC.ReadNoteAttributeAsync(a.NodeId.ToString(), Attributes.NodeClass)).Content.FirstOrDefault().Value.ToString();
            if (nodeClass == nameof(NodeClass.Variable))
            {
                ProtectTypeEnum level = ProtectTypeEnum.ReadOnly;
                DataTypeEnum dataTypeEnum = DataTypeEnum.Object;
                try
                {
                    var userAccessLevel = (AccessLevelType)(await PLC.ReadNoteAttributeAsync(a.NodeId.ToString(), Attributes.UserAccessLevel)).Content.FirstOrDefault().Value;
                    level = (userAccessLevel.HasFlag(AccessLevelType.CurrentRead)) ?
        userAccessLevel.HasFlag(AccessLevelType.CurrentWrite) ?
        ProtectTypeEnum.ReadWrite : ProtectTypeEnum.ReadOnly : ProtectTypeEnum.WriteOnly;

                    var dataTypeId = (Opc.Ua.NodeId)(await PLC.ReadNoteAttributeAsync(a.NodeId.ToString(), Attributes.DataType)).Content.FirstOrDefault().Value;
                    var dataType = Opc.Ua.TypeInfo.GetSystemType(dataTypeId, PLC.Session.Factory);
                    var result = dataType != null && Enum.TryParse<DataTypeEnum>(dataType.Name, out dataTypeEnum);
                    if (!result)
                    {
                        dataTypeEnum = DataTypeEnum.Object;
                    }
                }
                catch
                {

                }

                var id = YitIdHelper.NextId();

                return new DeviceVariable()
                {
                    Name = a.DisplayName.Text + "-" + id,
                    VariableAddress = a.NodeId.ToString(),
                    DeviceId = device.Id,
                    DataTypeEnum = dataTypeEnum,
                    Id = id,
                    ProtectTypeEnum = level,
                    IntervalTime = 1000,
                    RpcWriteEnable = true,
                };
            }
            else
            {
                return null;
            }
        })).Where(a => a != null).ToList();
        return (device, data);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        Task.Run(async () =>
        {
            Nodes = await PopulateBranchAsync(ObjectIds.ObjectsFolder);
            overlay = false;
            await InvokeAsync(StateHasChanged);
        });
    }

    /// <inheritdoc/>
    private CollectDevice GetImportDevice()
    {
        var id = YitIdHelper.NextId();
        var data = new CollectDevice()
        {
            Name = PLC.OPCNode.OPCUrl + "-" + id,
            Id = id,
            Enable = true,
            IsLogOut = true,
            DevicePropertys = new(),
            PluginId = DriverPluginService.GetIdByName(nameof(OPCUAClient)).ToLong(),
        };

        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.OPCURL), Value = PLC.OPCNode.OPCUrl, Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.OPCURL)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.UserName), Value = PLC.OPCNode.UserName, Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.UserName)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.Password), Value = PLC.OPCNode.Password, Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.Password)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.CheckDomain), Value = PLC.OPCNode.CheckDomain.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.CheckDomain)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.IsUseSecurity), Value = PLC.OPCNode.IsUseSecurity.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.IsUseSecurity)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.ActiveSubscribe), Value = true.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.ActiveSubscribe)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.DeadBand), Value = PLC.OPCNode.DeadBand.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.DeadBand)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.GroupSize), Value = PLC.OPCNode.GroupSize.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.GroupSize)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.UpdateRate), Value = PLC.OPCNode.UpdateRate.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.UpdateRate)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.KeepAliveInterval), Value = PLC.OPCNode.KeepAliveInterval.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.KeepAliveInterval)).GetCustomAttribute<DevicePropertyAttribute>().Description });
        return data;
    }
    private async Task<ReferenceDescriptionCollection> GetReferenceDescriptionCollectionAsync(NodeId sourceId)
    {
        BrowseDescription nodeToBrowse1 = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Aggregates,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.View | NodeClass.VariableType | NodeClass.DataType),
            ResultMask = (uint)BrowseResultMask.All
        };

        BrowseDescription nodeToBrowse2 = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.View | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.VariableType | NodeClass.DataType),
            ResultMask = (uint)BrowseResultMask.All
        };

        BrowseDescriptionCollection nodesToBrowse = new()
        {
            nodeToBrowse1,
            nodeToBrowse2
        };

        ReferenceDescriptionCollection references = await FormUtils.BrowseAsync(PLC.Session, nodesToBrowse, false);
        return references;
    }
    private async Task PopulateBranchAsync(OPCUATagModel model)
    {
        var sourceId = (NodeId)model.Tag.NodeId;
        model.Nodes = await PopulateBranchAsync(sourceId);
    }

    private async Task<List<OPCUATagModel>> PopulateBranchAsync(NodeId sourceId, bool isAll = false)
    {
        if (!PLC.Connected)
        {
            return new() { new() { Name = "未完成连接", Tag = new(), Nodes = null } };
        }
        List<OPCUATagModel> nodes = new()
        {
            new OPCUATagModel() { Name = "Browsering..." }
        };

        ReferenceDescriptionCollection references = await GetReferenceDescriptionCollectionAsync(sourceId);
        List<OPCUATagModel> list = new();
        if (references != null)
        {
            for (int ii = 0; ii < references.Count; ii++)
            {
                ReferenceDescription target = references[ii];
                OPCUATagModel child = new()
                {
                    Name = Utils.Format("{0}", target),
                    Tag = target
                };
                //if (target.NodeClass != NodeClass.Variable)
                {
                    var data = await GetReferenceDescriptionCollectionAsync((NodeId)target.NodeId);
                    if (data != null && data.Count > 0)
                    {
                        if (isAll)
                        {
                            child.Nodes = new();
                        }
                        else
                        {
                            child.Nodes = await PopulateBranchAsync((NodeId)target.NodeId);

                        }
                    }
                    else
                    {
                        child.Nodes = null;
                    }
                }
                //else
                //{
                //    child.Nodes = null;
                //}
                list.Add(child);
            }
        }

        List<OPCUATagModel> listNode = list;

        nodes.Clear();
        nodes.AddRange(listNode.ToArray());
        return nodes;
    }

    private Task<TResult[]> SelectAsync<T, TResult>(IEnumerable<T> source, Func<T, Task<TResult>> selector)
    {
        return Task.WhenAll(source.Select(selector));
    }
    internal class OPCUATagModel
    {
        internal string Name { get; set; }
        internal string NodeId => (Tag.NodeId).ToString();
        internal List<OPCUATagModel> Nodes { get; set; } = new();
        internal ReferenceDescription Tag { get; set; }

        public List<OPCUATagModel> GetAllTags()
        {
            List<OPCUATagModel> allTags = new();
            GetAllTagsRecursive(this, allTags);
            return allTags;
        }

        private void GetAllTagsRecursive(OPCUATagModel parentTag, List<OPCUATagModel> allTags)
        {
            allTags.Add(parentTag);

            if (parentTag.Nodes != null)
                foreach (OPCUATagModel childTag in parentTag.Nodes)
                {
                    GetAllTagsRecursive(childTag, allTags);
                }
        }
    }
}