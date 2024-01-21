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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备在线状态
/// </summary>
public enum DeviceStatusEnum
{
    /// <summary>
    /// 初始化
    /// </summary>
    [Description("初始化")]
    Default = 0,

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