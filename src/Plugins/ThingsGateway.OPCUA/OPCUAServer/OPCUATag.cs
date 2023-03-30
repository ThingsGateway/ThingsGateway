using Opc.Ua;
namespace ThingsGateway.OPCUA;
internal class OPCUATag : BaseDataVariableState
{
    public OPCUATag(NodeState parent) : base(parent)
    {
    }

    /// <summary>
    /// 变量Id
    /// </summary>
    public long Id { get; set; }
}
