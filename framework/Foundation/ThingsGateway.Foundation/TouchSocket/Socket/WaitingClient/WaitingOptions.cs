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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 适配器筛选
    /// </summary>
    public enum AdapterFilter
    {
        /// <summary>
        /// 发送和接收都经过适配器
        /// </summary>
        AllAdapter,

        /// <summary>
        /// 发送经过适配器，接收不经过
        /// </summary>
        SendAdapter,

        /// <summary>
        /// 发送不经过适配器，接收经过
        /// </summary>
        WaitAdapter,

        /// <summary>
        /// 全都不经过适配器。
        /// </summary>
        NoneAll
    }

    /// <summary>
    /// 等待设置
    /// </summary>
    public class WaitingOptions
    {
        /// <summary>
        /// 适配器筛选
        /// </summary>
        public AdapterFilter AdapterFilter { get; set; } = AdapterFilter.AllAdapter;
        /// <summary>
        /// 当Client为Tcp系时。是否在断开连接时立即触发结果。默认会返回null。当<see cref="ThrowBreakException"/>为<see langword="true"/>时，会触发异常。
        /// </summary>
        public bool BreakTrigger { get; set; }

        /// <summary>
        /// 远程地址(仅在Udp模式下生效)
        /// </summary>
        public IPHost RemoteIPHost { get; set; }

        /// <summary>
        /// 当Client为Tcp系时。是否在断开连接时以异常返回结果。
        /// </summary>
        public bool ThrowBreakException { get; set; } = true;
    }
}