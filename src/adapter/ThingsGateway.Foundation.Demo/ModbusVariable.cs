//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

[GeneratorVariable]
public partial class ModbusVariable : VariableObject
{
    public ModbusVariable(IProtocol protocol, int maxPack) : base(protocol, maxPack)
    {
    }

    [VariableRuntime(RegisterAddress = "400001;arraylen=2", ReadExpressions = "raw*10")]
    public ushort[] Data1 { get; set; }

    [VariableRuntime(RegisterAddress = "400051")]
    public ushort Data2 { get; set; }

    [VariableRuntime(RegisterAddress = "400061;len=10")]
    public string Data3 { get; set; }
}
