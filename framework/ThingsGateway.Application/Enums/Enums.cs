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

namespace ThingsGateway.Application;

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
/// 数据类型
/// </summary>
public enum DataTypeEnum
{
    /// <inheritdoc/>
    Object,

    /// <inheritdoc/>
    String,
    /// <inheritdoc/>
    Boolean,
    /// <inheritdoc/>
    Byte,
    /// <inheritdoc/>
    Int16,
    /// <inheritdoc/>
    UInt16,
    /// <inheritdoc/>
    Int32,
    /// <inheritdoc/>
    UInt32,
    /// <inheritdoc/>
    Int64,
    /// <inheritdoc/>
    UInt64,
    /// <inheritdoc/>
    Single,
    /// <inheritdoc/>
    Double,
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
/// 数据库类型
/// </summary>
public enum HisDbType
{
    /// <summary>
    /// 时序库QuestDB
    /// </summary>
    QuestDB,
    /// <summary>
    /// 时序库TDengine
    /// </summary>
    TDengine,
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
/// 共享通道
/// </summary>
public enum RedundantEnum
{
    /// <summary>
    /// 主站
    /// </summary>
    Primary,
    /// <summary>
    /// 备用
    /// </summary>
    Standby,
}

/// <summary>
/// 共享通道
/// </summary>
public enum ShareChannelEnum
{
    /// <summary>
    /// 不支持共享
    /// </summary>
    None,
    /// <summary>
    /// 串口
    /// </summary>
    SerialPort,
    /// <summary>
    /// TCP
    /// </summary>
    TcpClientEx,
    /// <summary>
    /// UDP
    /// </summary>
    UdpSession
}

/// <summary>
/// 数据库类型
/// </summary>
public enum SqlDbType
{
    /// <summary>
    /// SqlServer
    /// </summary>
    SqlServer,
    /// <summary>
    /// Mysql
    /// </summary>
    Mysql,
    /// <summary>
    /// Sqlite
    /// </summary>
    Sqlite,
    /// <summary>
    /// PostgreSQL
    /// </summary>
    PostgreSQL,
    /// <summary>
    /// Oracle
    /// </summary>
    Oracle,
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
/// <summary>
/// 历史存储类型
/// </summary>
public enum HisType
{
    /// <summary>
    /// 改变存储
    /// </summary>
    Change,
    /// <summary>
    /// 采集存储
    /// </summary>
    Collect,
}
