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

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 报警类型
/// </summary>
public enum AlarmEnum
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// Bool On
    /// </summary>
    Open,
    /// <summary>
    /// Bool Off
    /// </summary>
    Close,
    /// <summary>
    /// HH
    /// </summary>
    HH,
    /// <summary>
    /// H
    /// </summary>
    H,
    /// <summary>
    /// L
    /// </summary>
    L,
    /// <summary>
    /// LL
    /// </summary>
    LL,
}

/// <summary>
/// 设备在线状态
/// </summary>
public enum DeviceStatusEnum
{
    /// <summary>
    /// 初始化
    /// </summary>
    [Description("初始化")]
    None = 0,
    /// <summary>
    /// 在线
    /// </summary>
    [Description("在线")]
    OnLine = 1,
    /// <summary>
    /// 离线
    /// </summary>
    [Description("离线")]
    OffLine = 2,
    /// <summary>
    /// 暂停
    /// </summary>
    [Description("暂停")]
    Pause = 3,
}

/// <summary>
/// 插件类型
/// </summary>
public enum DriverEnum
{
    /// <summary>
    /// 采集
    /// </summary>
    [Description("采集")]
    Collect,
    /// <summary>
    /// 上传
    /// </summary>
    [Description("上传")]
    Upload,
}

/// <summary>
/// 报警事件类型
/// </summary>
public enum EventEnum
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// 报警产生
    /// </summary>
    Alarm,
    /// <summary>
    /// 报警确认
    /// </summary>
    Check,
    /// <summary>
    /// 报警恢复
    /// </summary>
    Finish,
}


/// <summary>
/// 用户权限
/// </summary>
public enum ProtectTypeEnum
{
    /// <summary>
    /// 只读
    /// </summary>
    [Description("只读")]
    ReadOnly,
    /// <summary>
    /// 读写
    /// </summary>
    [Description("读写")]
    ReadWrite,
    /// <summary>
    /// 只写
    /// </summary>
    [Description("只写")]
    WriteOnly,
}
/// <summary>
/// 冗余
/// </summary>
public enum RedundantEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None,
    /// <summary>
    /// 主站
    /// </summary>
    [Description("主站")]
    Primary,
    /// <summary>
    /// 备用
    /// </summary>
    [Description("备用站")]
    Standby,
}

/// <summary>
/// 返回状态
/// </summary>
public enum ThreadRunReturn
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// 继续
    /// </summary>
    Continue,
    /// <summary>
    /// 跳出
    /// </summary>
    Break,
}
