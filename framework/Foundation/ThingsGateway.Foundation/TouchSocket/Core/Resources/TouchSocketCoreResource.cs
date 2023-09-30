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

using System.ComponentModel;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TouchSocketCore资源枚举
    /// </summary>
    public enum TouchSocketCoreResource
    {
        /// <summary>
        /// 未知错误
        /// </summary>
        [Description("未知错误")]
        UnknownError,

        /// <summary>
        /// 操作成功
        /// </summary>
        [Description("操作成功")]
        Success,

        /// <summary>
        /// 操作超时
        /// </summary>
        [Description("操作超时")]
        Overtime,

        /// <summary>
        /// 用户主动取消操作。
        /// </summary>
        [Description("用户主动取消操作。")]
        Canceled,

        /// <summary>
        /// 参数‘{0}’为空。
        /// </summary>
        [Description("参数‘{0}’为空。")]
        ArgumentNull,

        /// <summary>
        ///发生异常，信息：{0}。
        /// </summary>
        [Description("发生异常，信息：{0}。")]
        Exception,

        /// <summary>
        /// Token消息为‘{0}’的已注册。
        /// </summary>
        [Description("Token消息为‘{0}’的已注册。")]
        TokenExisted,

        /// <summary>
        /// Token消息为‘{0}’的未注册。
        /// </summary>
        [Description("Token消息为‘{0}’的未注册。")]
        MessageNotFound,

        /// <summary>
        /// 无法创建未被注册的类型{0}的实例。
        /// </summary>
        [Description("无法创建未被注册的类型{0}的实例。")]
        UnregisteredType,

        /// <summary>
        /// 没有找到类型{0}的公共构造函数。
        /// </summary>
        [Description("没有找到类型{0}的公共构造函数。")]
        NotFindPublicConstructor,
        /// <summary>
        /// 路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。
        /// </summary>
        [Description("路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。")]
        StreamReferencing,

        /// <summary>
        ///没有找到路径‘{0}’对应的流文件。
        /// </summary>
        [Description("没有找到路径‘{0}’对应的流文件。")]
        StreamNotFind,

        /// <summary>
        /// 文件‘{0}’不存在。
        /// </summary>
        [Description("文件‘{0}’不存在。")]
        FileNotExists,
    }
}