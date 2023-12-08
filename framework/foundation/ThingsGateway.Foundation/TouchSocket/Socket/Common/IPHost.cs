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

using System.Net;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// IP解析映射
    /// <para>
    /// 支持端口，ip，域名等。具体格式如下：
    /// <list type=" number">
    /// <item>端口：直接按<see cref="int"/>入参，该操作一般在监听时使用。</item>
    /// <item>ip：按127.0.0.1:7789入参。</item>
    /// <item>域名：按tcp://127.0.0.1:7789、或者http://baidu.com入参。</item>
    /// </list>
    /// </para>
    /// </summary>
    public class IPHost : Uri
    {
        private EndPoint m_endPoint;

        /// <summary>
        /// IP解析映射
        /// <para>
        /// 支持端口，ip，域名等。具体格式如下：
        /// <list type=" number">
        /// <item>端口：直接按<see cref="int"/>入参，该操作一般在监听时使用。</item>
        /// <item>ip：按127.0.0.1:7789入参。</item>
        /// <item>域名：按tcp://127.0.0.1:7789、或者http://baidu.com入参。</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="uriString"></param>
        public IPHost(string uriString) : base(VerifyUri(uriString))
        {
        }

        /// <summary>
        /// 从端口号创建。
        /// </summary>
        /// <param name="port"></param>
        public IPHost(int port) : this($"0.0.0.0:{port}")
        {
        }

        /// <summary>
        /// 从<see cref="IPAddress"/>
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public IPHost(IPAddress address, int port) : this($"{address}:{port}")
        {
        }

        /// <summary>
        /// 获取终结点。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public EndPoint EndPoint
        {
            get
            {
                if (this.m_endPoint != null)
                {
                    return this.m_endPoint;
                }
                if (this.HostNameType == UriHostNameType.Unknown || this.HostNameType == UriHostNameType.Basic)
                {
                    throw new Exception("无法获取终结点。");
                }

                if (this.HostNameType == UriHostNameType.Dns)
                {
                    this.m_endPoint = new DnsEndPoint(this.DnsSafeHost, this.Port);
                }
                else
                {
                    this.m_endPoint = new IPEndPoint(IPAddress.Parse(this.DnsSafeHost), this.Port);
                }
                return this.m_endPoint;
            }
        }

        /// <summary>
        /// 端口号
        /// </summary>
        public new int Port
        {
            get
            {
                if (this.IsDefaultPort)
                {
                    switch (this.Scheme.ToLower())
                    {
                        case "http": return 80;
                        case "https": return 443;
                        case "tcp": return 8080;
                        default:
                            break;
                    }
                }
                return base.Port;
            }
        }

        #region Implicit

        /// <summary>
        /// 由字符串向<see cref="IPHost"/>转换
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator IPHost(string value) => new IPHost(value);

        /// <summary>
        /// 由端口向<see cref="IPHost"/>转换
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator IPHost(int value) => new IPHost(value);

        #endregion Implicit

        /// <summary>
        /// 解析一个组的地址。
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static IPHost[] ParseIPHosts(string[] strs)
        {
            var iPs = new List<IPHost>();
            foreach (var item in strs)
            {
                iPs.Add(new IPHost(item));
            }
            return iPs.ToArray();
        }

        private static string VerifyUri(string uriString)
        {
            if (TouchSocketUtility.IsURL(uriString))
            {
                return uriString;
            }

            return $"tcp://{uriString}";
        }

        ///// <summary>
        ///// 返回EndPoint字符串
        ///// </summary>
        ///// <returns></returns>
        //public override string ToString()
        //{
        //    return this.EndPoint == null ? null : this.EndPoint.ToString();
        //}

        //private static bool HostNameToIP(string hostname, out IPAddress[] address)
        //{
        //    try
        //    {
        //        var hostInfo = Dns.GetHostEntry(hostname);
        //        address = hostInfo.AddressList;
        //        return address.Length > 0;
        //    }
        //    catch
        //    {
        //        address = null;
        //        return false;
        //    }
        //}

        //private void Analysis(string ip, string port)
        //{
        //    try
        //    {
        //        var portNum = int.Parse(port);
        //        var endPoint = new IPEndPoint(IPAddress.Parse(ip), portNum);
        //        this.AddressFamily = ip.Contains(":") ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
        //        this.EndPoint = endPoint;
        //        this.IP = ip;
        //        this.Port = portNum;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"IPHost初始化失败，信息:{ex.Message}", ex);
        //    }
        //}
    }
}