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

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// 针对Dmtp的配置项
    /// </summary>
    public class DmtpOption
    {
        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken { get; set; }

        /// <summary>
        /// 连接时指定Id。
        /// <para>
        /// 使用该功能时，仅在服务器的Handshaking之后生效。且如果id重复，则会连接失败。
        /// </para>
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 设置DmtpClient连接时的元数据
        /// </summary>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// 验证连接超时时间。仅用于服务器。意为：当服务器收到基础链接，在指定的时间内如果没有收到握手信息，则直接视为无效链接，直接断开。
        /// </summary>
        public TimeSpan VerifyTimeout { get; set; } = TimeSpan.FromSeconds(3);
    }
}