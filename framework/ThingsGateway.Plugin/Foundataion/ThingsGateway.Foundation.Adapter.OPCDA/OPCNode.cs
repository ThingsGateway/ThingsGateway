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

using System.ComponentModel;

namespace ThingsGateway.Foundation.Adapter.OPCDA;
/// <summary>
/// OPCDA连接配置项
/// </summary>
public class OPCNode
{
    /// <summary>
    /// 是否订阅
    /// </summary>
    [Description("ActiveSubscribe")]
    public bool ActiveSubscribe { get; set; } = true;
    /// <summary>
    /// 内部检测重连间隔/min
    /// </summary>
    [Description("重连间隔/min")]
    public int CheckRate { get; set; } = 30;
    /// <summary>
    /// 死区
    /// </summary>
    [Description("死区")]
    public float DeadBand { get; set; } = 0;
    /// <summary>
    /// 分组大小
    /// </summary>
    [Description("分组大小")]
    public int GroupSize { get; set; } = 500;
    /// <summary>
    /// OPCIP
    /// </summary>
    [Description("OPCIP")]
    public string OPCIP { get; set; } = "localhost";
    /// <summary>
    /// OPCNAME
    /// </summary>
    [Description("OPCNAME")]
    public string OPCName { get; set; } = "Kepware.KEPServerEX.V6";
    /// <summary>
    /// 订阅间隔
    /// </summary>
    [Description("订阅间隔")]
    public int UpdateRate { get; set; } = 1000;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{(string.IsNullOrEmpty(OPCIP) ? "localhost" : OPCIP)}:{OPCName}";
    }
}
