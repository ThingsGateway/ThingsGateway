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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// 日志类型。
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// 更为详细的步骤型日志输出
    /// </summary>
    Trace = 0,

    /// <summary>
    /// 调试信息日志
    /// </summary>
    Debug = 1,

    /// <summary>
    /// 消息类日志输出
    /// </summary>
    Info = 2,

    /// <summary>
    /// 警告类日志输出
    /// </summary>
    Warning = 3,

    /// <summary>
    /// 错误类日志输出
    /// </summary>
    Error = 4,

    /// <summary>
    /// 不可控中断类日输出
    /// </summary>
    Critical = 5,

    /// <summary>
    /// 不使用日志类输出
    /// </summary>
    None = 6,
}