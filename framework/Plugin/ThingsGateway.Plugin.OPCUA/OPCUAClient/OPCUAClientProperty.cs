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

namespace ThingsGateway.Plugin.OPCUA;

/// <inheritdoc/>
public class OPCUAClientProperty : CollectDriverPropertyBase
{
    /// <summary>
    /// 连接Url
    /// </summary>
    [DeviceProperty("连接Url", "")]
    public string OPCURL { get; set; } = "opc.tcp://127.0.0.1:49320";
    /// <summary>
    /// 登录账号
    /// </summary>
    [DeviceProperty("登录账号", "为空时将采用匿名方式登录")]
    public string UserName { get; set; }


    /// <summary>
    /// 登录密码
    /// </summary>
    [DeviceProperty("登录密码", "")]
    public string Password { get; set; }

    /// <summary>
    /// 检查域
    /// </summary>
    [DeviceProperty("检查域", "默认false")]
    public bool CheckDomain { get; set; }

    /// <summary>
    /// 安全策略
    /// </summary>
    [DeviceProperty("安全策略", "True为使用安全策略，False为无")]
    public bool IsUseSecurity { get; set; } = true;

    /// <summary>
    /// 是否使用SourceTime
    /// </summary>
    [DeviceProperty("使用SourceTime", "")]
    public bool SourceTimestampEnable { get; set; } = true;

    /// <summary>
    /// 激活订阅
    /// </summary>
    [DeviceProperty("激活订阅", "")]
    public bool ActiveSubscribe { get; set; } = true;

    /// <summary>
    /// 更新频率
    /// </summary>
    [DeviceProperty("更新频率", "")]
    public int UpdateRate { get; set; } = 1000;


    /// <summary>
    /// 死区
    /// </summary>
    [DeviceProperty("死区", "")]
    public double DeadBand { get; set; } = 0;

    /// <summary>
    /// 自动分组大小
    /// </summary>
    [DeviceProperty("自动分组大小", "")]
    public int GroupSize { get; set; } = 500;
    /// <summary>
    /// 心跳频率
    /// </summary>
    [DeviceProperty("心跳频率", "")]
    public int KeepAliveInterval { get; set; } = 3000;
    /// <inheritdoc/>
    public override bool IsShareChannel { get; set; } = false;
    /// <inheritdoc/>
    public override ChannelEnum ShareChannel => ChannelEnum.None;

}
