using Opc.Ua;
namespace ThingsGateway.OPCUA;
internal class OPCUATag : BaseDataVariableState
{
    public OPCUATag(NodeState parent) : base(parent)
    {
    }
    /// <summary>
    /// 变量数据类型
    /// </summary>
    public Type NETDataType { get; set; }
    /// <summary>
    /// 变量Id
    /// </summary>
    public long Id { get; set; }
}
