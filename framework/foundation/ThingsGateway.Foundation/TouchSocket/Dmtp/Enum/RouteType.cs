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
    /// 路由类型
    /// </summary>
    public readonly struct RouteType
    {
        private readonly string m_value;

        /// <summary>
        /// 路由类型
        /// </summary>
        /// <param name="value"></param>
        public RouteType(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"“{nameof(value)}”不能为 null 或空。", nameof(value));
            }

            this.m_value = value.ToLower().Trim();
        }

        /// <summary>
        /// 判断RouteType是相等。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(RouteType a, RouteType b)
        {
            return a.m_value == b.m_value;
        }

        /// <summary>
        /// 判断RouteType不相等。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(RouteType a, RouteType b)
        {
            return a.m_value != b.m_value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is RouteType type && this == type;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.m_value.GetHashCode();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.m_value;

        /// <summary>
        /// 一个Ping探测路由包
        /// </summary>
        public static readonly RouteType Ping = new RouteType("Ping");

        /// <summary>
        /// 创建通道路由包。
        /// </summary>
        public static readonly RouteType CreateChannel = new RouteType("CreateChannel");

        /// <summary>
        /// Rpc调用的路由包
        /// </summary>
        public static readonly RouteType Rpc = new RouteType("Rpc");

        /// <summary>
        /// 拉取文件的路由包
        /// </summary>
        public static readonly RouteType PullFile = new RouteType("PullFile");

        /// <summary>
        /// 推送文件的路由包
        /// </summary>
        public static readonly RouteType PushFile = new RouteType("PushFile");
    }
}