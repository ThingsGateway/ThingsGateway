#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation.Adapter.OPCUA;
public class OPCNode
{
    public string OPCURL { get; set; } = "opc.tcp://127.0.0.1:49320";
    public int UpdateRate { get; set; } = 1000;
    public bool ActiveSubscribe { get; set; } = true;
    public int GroupSize { get; set; } = 500;
    public double DeadBand { get; set; } = 0;
    public int ReconnectPeriod { get; set; } = 5000;
    public bool IsUseSecurity { get; set; } = true;

    public override string ToString()
    {
        return OPCURL;
    }
}
