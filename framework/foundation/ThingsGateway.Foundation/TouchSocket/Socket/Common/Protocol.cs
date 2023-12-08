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
    /// 协议类
    /// </summary>
    public readonly struct Protocol
    {
        /// <summary>
        /// 值
        /// </summary>
        private readonly string m_value;

        /// <summary>
        /// 表示无协议
        /// </summary>
        public static readonly Protocol None = new Protocol();

        /// <summary>
        /// 获取http协议
        /// </summary>
        public static readonly Protocol Http = new Protocol("http");

        /// <summary>
        /// TCP协议
        /// </summary>
        public static readonly Protocol Tcp = new Protocol("tcp");

        /// <summary>
        /// 命名管道
        /// </summary>
        public static readonly Protocol NamedPipe = new Protocol("namedpipe");

        /// <summary>
        /// UDP协议
        /// </summary>
        public static readonly Protocol Udp = new Protocol("udp");

        /// <summary>
        /// 获取WebSocket协议
        /// </summary>
        public static readonly Protocol WebSocket = new Protocol("ws");

        /// <summary>
        /// 表示
        /// </summary>
        /// <param name="value">值</param>
        public Protocol(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException();
            }
            this.m_value = value.ToLower();
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(this.m_value) ? "none" : this.m_value;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.m_value == null ? string.Empty.GetHashCode() : this.m_value.GetHashCode();
        }

        /// <summary>
        /// 比较是否和目标相等
        /// </summary>
        /// <param name="obj">目标</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Protocol && this.GetHashCode() == obj.GetHashCode();
        }

        /// <summary>
        /// 等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Protocol a, Protocol b)
        {
            return string.IsNullOrEmpty(a.m_value) && string.IsNullOrEmpty(b.m_value) || string.Equals(a.m_value, b.m_value);
        }

        /// <summary>
        /// 不等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Protocol a, Protocol b)
        {
            var state = a == b;
            return !state;
        }
    }
}