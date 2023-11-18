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
using System.Runtime.InteropServices;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 保活机制
    /// </summary>
    public class KeepAliveValue
    {
        /// <summary>
        /// 保活机制
        /// </summary>
        public byte[] KeepAliveTime
        {
            get
            {
                uint dummy = 0;
                var inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes(this.Interval).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes(this.AckInterval).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                return inOptionValues;
            }
        }

        /// <summary>
        /// 发送间隔，默认20*1000ms
        /// </summary>
        public uint Interval { get; set; } = 20 * 1000;

        /// <summary>
        /// 确认间隔，默认2*1000ms
        /// </summary>
        public uint AckInterval { get; set; } = 2 * 1000;
    }
}