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

using System.Reflection;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Adapter.OPCUA;
using ThingsGateway.Web.Foundation;

using Yitter.IdGenerator;

namespace ThingsGateway.OPCUA
{
    public partial class ImportVariable
    {
        [Inject]
        IDriverPluginService DriverPluginService { get; set; }
        public CollectDevice GetImportDevice()
        {
            var data =
                   new CollectDevice()
                   {
                       Name = PLC.OPCNode.OPCURL,
                       Id = YitIdHelper.NextId(),
                       Enable = true,
                       IsLogOut = true,
                       DevicePropertys = new(),
                       PluginId = DriverPluginService.GetIdByName("ThingsGateway.OPCUA.OPCUAClient").ToLong(),
                   };

            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.OPCURL), Value = PLC.OPCNode.OPCURL, Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.OPCURL)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.UserName), Value = PLC.UserIdentity.DisplayName, Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.UserName)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.Password), Value = "", Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.Password)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.IsUseSecurity), Value = PLC.OPCNode.IsUseSecurity.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.IsUseSecurity)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.ActiveSubscribe), Value = true.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.ActiveSubscribe)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.DeadBand), Value = PLC.OPCNode.DeadBand.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.DeadBand)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.GroupSize), Value = PLC.OPCNode.GroupSize.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.GroupSize)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.UpdateRate), Value = PLC.OPCNode.UpdateRate.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.UpdateRate)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            data.DevicePropertys.Add(new() { PropertyName = nameof(OPCUAClientProperty.ReconnectPeriod), Value = PLC.OPCNode.ReconnectPeriod.ToString(), Description = typeof(OPCUAClientProperty).GetProperty(nameof(OPCUAClientProperty.ReconnectPeriod)).GetCustomAttribute<DevicePropertyAttribute>().Description });
            return data;
        }

        protected override void OnInitialized()
        {
            Task.Run(async () =>
            {
                Nodes = await PopulateBranchAsync(ObjectIds.ObjectsFolder);
                overlay = false;
                await InvokeAsync(StateHasChanged);
            });
        }
        private bool overlay = true;
        [Parameter]
        public ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient PLC { get; set; }
        private List<OPCUATagModel> Nodes = new();
        private List<ReferenceDescription> _selected { get; set; } = new();
        private List<ReferenceDescription> actived = new();
        private OPCNodeAttribute[] nodeAttributes;
        private List<ReferenceDescription> _actived
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

        private async Task PopulateBranchAsync(OPCUATagModel model)
        {
            var sourceId = (NodeId)model.Tag.NodeId;
            model.Nodes = await PopulateBranchAsync(sourceId);
        }

        public async Task<(CollectDevice, List<DeviceVariable>)> GetImportVariableList()
        {
            var device = GetImportDevice();
            var data = (await _selected.SelectAsync(async a =>
            {
                var nodeClass = (await PLC.ReadNoteAttributeAsync(a.NodeId.ToString(), Attributes.NodeClass)).Content.FirstOrDefault().Value.ToString();
                if (nodeClass == nameof(NodeClass.Variable))
                {
                    ProtectTypeEnum level = ProtectTypeEnum.ReadOnly;
                    try
                    {
                        var userAccessLevel = (AccessLevelType)(await PLC.ReadNoteAttributeAsync(a.NodeId.ToString(), Attributes.UserAccessLevel)).Content.FirstOrDefault().Value;
                        level = (userAccessLevel.HasFlag(AccessLevelType.CurrentRead)) ?
            userAccessLevel.HasFlag(AccessLevelType.CurrentWrite) ?
            ProtectTypeEnum.ReadWrite : ProtectTypeEnum.ReadOnly : ProtectTypeEnum.WriteOnly;
                    }
                    catch
                    {

                    }
                    var dataTypeId = (Opc.Ua.NodeId)(await PLC.ReadNoteAttributeAsync(a.NodeId.ToString(), Attributes.DataType)).Content.FirstOrDefault().Value;
                    var dataType = Opc.Ua.DataTypes.GetSystemType(dataTypeId, null);
                    var result = Enum.TryParse(typeof(DataTypeEnum), dataType.Name, out object dataTypeEnum);
                    if (!result)
                    {
                        dataTypeEnum = DataTypeEnum.Object;
                    }
                    return new DeviceVariable()
                    {
                        Name = device.Name + "_" + a.DisplayName.Text,
                        VariableAddress = a.NodeId.ToString(),
                        DeviceId = device.Id,
                        DataTypeEnum = (DataTypeEnum)dataTypeEnum,
                        Id = YitIdHelper.NextId(),
                        ProtectTypeEnum = level,
                        IntervalTime = 1000,
                    };
                }
                else
                {
                    return null;
                }
            })).Where(a => a != null).ToList();
            return (device, data);
        }

        private async Task<List<OPCUATagModel>> PopulateBranchAsync(NodeId sourceId)
        {
            if (!PLC.Connected)
            {
                return new() { new() { Name = "未完成连接", Tag = new(), Nodes = null } };
            }
            List<OPCUATagModel> nodes = new List<OPCUATagModel>();
            nodes.Add(new OPCUATagModel() { Name = "Browsering..." });

            ReferenceDescriptionCollection references = await GetReferenceDescriptionCollectionAsync(sourceId);
            List<OPCUATagModel> list = new List<OPCUATagModel>();
            if (references != null)
            {
                for (int ii = 0; ii < references.Count; ii++)
                {
                    ReferenceDescription target = references[ii];
                    OPCUATagModel child = new OPCUATagModel { Name = Utils.Format("{0}", target) };

                    child.Tag = target;
                    var data = await GetReferenceDescriptionCollectionAsync((NodeId)target.NodeId);
                    if (data != null && data.Count > 0)
                    {
                        //child.Nodes = PopulateBranch((NodeId)target.NodeId); 
                    }
                    else
                    {
                        child.Nodes = null;
                    }
                    list.Add(child);
                }
            }

            List<OPCUATagModel> listNode = list;

            nodes.Clear();
            nodes.AddRange(listNode.ToArray());
            return nodes;
        }

        private async Task<ReferenceDescriptionCollection> GetReferenceDescriptionCollectionAsync(NodeId sourceId)
        {
            TaskCompletionSource<ReferenceDescriptionCollection> task = new TaskCompletionSource<ReferenceDescriptionCollection>();

            BrowseDescription nodeToBrowse1 = new BrowseDescription();

            nodeToBrowse1.NodeId = sourceId;
            nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.Aggregates;
            nodeToBrowse1.IncludeSubtypes = true;
            nodeToBrowse1.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.View | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

            BrowseDescription nodeToBrowse2 = new BrowseDescription();

            nodeToBrowse2.NodeId = sourceId;
            nodeToBrowse2.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse2.ReferenceTypeId = ReferenceTypeIds.Organizes;
            nodeToBrowse2.IncludeSubtypes = true;
            nodeToBrowse2.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.View | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse2.ResultMask = (uint)BrowseResultMask.All;

            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(nodeToBrowse1);
            nodesToBrowse.Add(nodeToBrowse2);

            ReferenceDescriptionCollection references = await FormUtils.BrowseAsync(PLC.Session, nodesToBrowse, false);
            return references;
        }

        internal class OPCUATagModel
        {
            internal string Name { get; set; }
            internal ReferenceDescription Tag { get; set; }
            internal string NodeId => (Tag.NodeId).ToString();
            internal List<OPCUATagModel> Nodes { get; set; } = new();
        }
    }
}