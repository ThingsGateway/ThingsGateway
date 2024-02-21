//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.OpcUa;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class OpcUaMasterProperty : CollectPropertyBase
{
    /// <summary>
    /// 连接Url
    /// </summary>
    [DynamicProperty("连接Url", "")]
    public string OpcUrl { get; set; } = "opc.tcp://127.0.0.1:49320";

    /// <summary>
    /// 登录账号
    /// </summary>
    [DynamicProperty("登录账号", "为空时将采用匿名方式登录")]
    public string? UserName { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    [DynamicProperty("登录密码", "")]
    public string? Password { get; set; }

    /// <summary>
    /// 检查域
    /// </summary>
    [DynamicProperty("检查域", "默认false")]
    public bool CheckDomain { get; set; }

    /// <summary>
    /// 安全策略
    /// </summary>
    [DynamicProperty("安全策略", "True为使用安全策略，False为无")]
    public bool IsUseSecurity { get; set; } = true;

    /// <summary>
    /// 是否使用SourceTime
    /// </summary>
    [DynamicProperty("使用SourceTime", "")]
    public bool SourceTimestampEnable { get; set; } = true;

    /// <summary>
    /// 加载服务端数据类型
    /// </summary>
    [DynamicProperty("加载服务端数据类型")]
    public bool LoadType { get; set; } = true;

    /// <summary>
    /// 激活订阅
    /// </summary>
    [DynamicProperty("激活订阅", "")]
    public bool ActiveSubscribe { get; set; } = true;

    /// <summary>
    /// 更新频率
    /// </summary>
    [DynamicProperty("更新频率", "")]
    public int UpdateRate { get; set; } = 1000;

    /// <summary>
    /// 死区
    /// </summary>
    [DynamicProperty("死区", "")]
    public double DeadBand { get; set; } = 0;

    /// <summary>
    /// 最大组大小
    /// </summary>
    [DynamicProperty("最大组大小", "")]
    public int GroupSize { get; set; } = 500;

    /// <summary>
    /// 心跳频率
    /// </summary>
    [DynamicProperty("心跳频率", "")]
    public int KeepAliveInterval { get; set; } = 3000;

    public override int ConcurrentCount { get; set; } = 1;
}