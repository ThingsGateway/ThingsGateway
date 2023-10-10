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

namespace ThingsGateway.Foundation.Resources
{
    /// <summary>
    /// TouchSocketDmtp资源枚举
    /// </summary>
    public enum TouchSocketDmtpResource
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
        /// DmtpRpcActor为空，可能需要启用DmtpRpc插件。
        /// </summary>
        [Description("DmtpRpcActor为空，可能需要启用DmtpRpc插件。")]
        DmtpRpcActorArgumentNull,

        /// <summary>
        /// DmtpFileTransferActor为空，可能需要启用DmtpFileTransfer插件。
        /// </summary>
        [Description("DmtpFileTransferActor为空，可能需要启用DmtpFileTransfer插件。")]
        DmtpFileTransferActorNull,

        /// <summary>
        /// RedisActor为空，可能需要启用RedisActor插件。
        /// </summary>
        [Description("RedisActor为空，可能需要启用RedisActor插件。")]
        RedisActorNull,

        /// <summary>
        /// RemoteAccessActor为空，可能需要启用RemoteAccess插件。
        /// </summary>
        [Description("RemoteAccessActor为空，可能需要启用RemoteAccess插件。")]
        RemoteAccessActorNull,

        /// <summary>
        /// RemoteStreamActor为空，可能需要启用RemoteStream插件。
        /// </summary>
        [Description("RemoteStreamActor为空，可能需要启用RemoteStream插件。")]
        RemoteStreamActorNull,

        #region DmtpRpc

        /// <summary>
        /// 不允许路由该包，信息：{0}。
        /// </summary>
        [Description("不允许路由该包，信息：{0}。")]
        RoutingNotAllowed,

        /// <summary>
        /// 未找到该公共方法，或该方法未标记为Rpc
        /// </summary>
        [Description("未找到该公共方法，或该方法未标记为Rpc")]
        RpcMethodNotFind,

        /// <summary>
        /// 方法已被禁用
        /// </summary>
        [Description("方法已被禁用")]
        RpcMethodDisable,

        /// <summary>
        /// 函数执行异常，详细信息：{0}
        /// </summary>
        [Description("函数执行异常，详细信息：{0}")]
        RpcInvokeException,

        /// <summary>
        /// 事件操作器异常
        /// </summary>
        [Description("事件操作器异常。")]
        GetEventArgsFail,

        /// <summary>
        /// 通道设置失败。
        /// </summary>
        [Description("通道设置失败。")]
        SetChannelFail,

        /// <summary>
        /// Id为{0}的通道已存在。
        /// </summary>
        [Description("Id为{0}的通道已存在。")]
        ChannelExisted,

        /// <summary>
        /// 远程终端拒绝该操作，反馈信息：{0}。
        /// </summary>
        [Description("远程终端拒绝该操作，反馈信息：{0}。")]
        RemoteRefuse,

        /// <summary>
        /// 从‘{0}’创建写入流失败，信息：{1}。"
        /// </summary>
        [Description("从‘{0}’创建写入流失败，信息：{1}。")]
        CreateWriteStreamFail,

        /// <summary>
        ///没有找到路径‘{0}’对应的流文件。
        /// </summary>
        [Description("没有找到路径‘{0}’对应的流文件。")]
        StreamNotFind,

        /// <summary>
        /// 没有找到Id为{0}的客户端。
        /// </summary>
        [Description("没有找到Id为{0}的客户端。")]
        ClientNotFind,

        /// <summary>
        /// 路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。
        /// </summary>
        [Description("路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。")]
        StreamReferencing,

        /// <summary>
        /// 接收流容器为空
        /// </summary>
        [Description("流容器为空。")]
        StreamBucketNull,

        /// <summary>
        /// 从‘{0}’路径加载流异常，信息：‘{1}’。
        /// </summary>
        [Description("从‘{0}’路径加载流异常，信息：‘{1}’。")]
        LoadStreamFail,

        /// <summary>
        /// 目录‘{0}’已存在。
        /// </summary>
        [Description("目录‘{0}’已存在。")]
        DirectoryExisted,

        /// <summary>
        /// 文件‘{0}’已存在。
        /// </summary>
        [Description("文件‘{0}’已存在。")]
        FileExisted,

        /// <summary>
        /// 文件‘{0}’不存在。
        /// </summary>
        [Description("文件‘{0}’不存在。")]
        FileNotExists,

        /// <summary>
        /// 目录‘{0}’不存在。
        /// </summary>
        [Description("目录‘{0}’不存在。")]
        DirectoryNotExists,

        /// <summary>
        /// 名称为“{0}”的事件已存在
        /// </summary>
        [Description("名称为“{0}”的事件已存在。")]
        EventExisted,

        /// <summary>
        /// 名称为“{0}”的事件不存在
        /// </summary>
        [Description("名称为“{0}”的事件不存在。")]
        EventNotExist,

        /// <summary>
        /// 资源句柄{0}对应的资源没有找到，可能操作已超时。
        /// </summary>
        [Description("资源句柄{0}对应的资源没有找到，可能操作已超时。")]
        ResourceHandleNotFind,

        /// <summary>
        /// 还有{0}个资源没有完成。
        /// </summary>
        [Description("还有{0}个资源没有完成。")]
        HasUnFinished,

        /// <summary>
        /// 文件长度太长。
        /// </summary>
        [Description("文件长度太长。")]
        FileLengthTooLong,

        /// <summary>
        /// 读取文件长度错误。
        /// </summary>
        [Description("读取文件长度错误。")]
        LengthErrorWhenRead,

        /// <summary>
        /// 没有找到任何可用的目标Id。
        /// </summary>
        [Description("没有找到任何可用的目标Id。")]
        NotFindAnyTargetId,

        #endregion DmtpRpc

        #region Core

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

        #endregion Core

        #region Client

        /// <summary>
        /// 数据处理适配器为空，可能客户端已掉线。
        /// </summary>
        [Description("数据处理适配器为空，可能客户端已掉线。")]
        NullDataAdapter,

        /// <summary>
        /// 客户端没有连接
        /// </summary>
        [Description("客户端没有连接。")]
        NotConnected,

        /// <summary>
        /// 授权密钥无效，程序将在5秒后退出。请检查密钥，或者不使用企业版功能。
        /// </summary>
        [Description("授权密钥无效，程序将在5秒后退出。请检查密钥，或者不使用企业版功能。")]
        LicenceKeyInvalid,

        #endregion Client
    }
}