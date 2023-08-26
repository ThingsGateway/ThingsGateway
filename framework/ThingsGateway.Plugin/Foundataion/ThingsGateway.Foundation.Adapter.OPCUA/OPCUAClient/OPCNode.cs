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

namespace ThingsGateway.Foundation.Adapter.OPCUA;
/// <summary>
/// OPCUAClient配置项
/// </summary>
public class OPCNode
{
    /// <summary>
    /// OPCUrl
    /// </summary>
    [Description("OPCUrl")]
    public string OPCUrl { get; set; } = "opc.tcp://127.0.0.1:49320";
    /// <summary>
    /// 登录账号
    /// </summary>
    [Description("登录账号")]
    public string UserName { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    [Description("登录密码")]
    public string Password { get; set; }
    /// <summary>
    /// 检查域
    /// </summary>
    [Description("检查域")]
    public bool CheckDomain { get; set; }
    
    /// <summary>
    /// 更新间隔
    /// </summary>
    [Description("更新间隔")]
    public int UpdateRate { get; set; } = 1000;
    /// <summary>
    /// 是否订阅
    /// </summary>
    [Description("是否订阅")]
    public bool ActiveSubscribe { get; set; } = true;
    /// <summary>
    /// 分组大小
    /// </summary>
    [Description("分组大小")]
    public int GroupSize { get; set; } = 500;
    /// <summary>
    /// 死区
    /// </summary>
    [Description("死区")]
    public double DeadBand { get; set; } = 0;
    /// <summary>
    /// KeepAliveInterval/ms
    /// </summary>
    [Description("KeepAliveInterval/ms")]
    public int KeepAliveInterval { get; set; } = 3000;
    /// <summary>
    /// 安全策略
    /// </summary>
    [Description("安全策略")]
    public bool IsUseSecurity { get; set; } = false;
    /// <inheritdoc/>
    public override string ToString()
    {
        return OPCUrl;
    }
}
