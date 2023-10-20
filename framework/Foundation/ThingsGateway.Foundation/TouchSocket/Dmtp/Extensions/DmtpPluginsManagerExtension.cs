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
using ThingsGateway.Foundation.Core;

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// DmtpPluginsManagerExtension
    /// </summary>
    public static class DmtpPluginsManagerExtension
    {
        /// <summary>
        /// DmtpRpc心跳。客户端、服务器均，但是一般建议仅客户端使用即可。
        /// <para>
        /// 默认心跳每3秒进行一次。最大失败3次即判定为断开连接。
        /// </para>
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static DmtpHeartbeatPlugin UseDmtpHeartbeat(this IPluginsManager pluginsManager)
        {
            var heartbeat = new DmtpHeartbeatPlugin();
            pluginsManager.Add(heartbeat);
            return heartbeat;
        }
    }
}