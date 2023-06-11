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

using System.Net;
using System.Net.Sockets;

namespace ThingsGateway.Core
{
    public static class IPAddressExtensions
    {
        /// <summary>
        /// 判断IP是否是私有地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(this IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip)) return true;
            ip = ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip;
            byte[] bytes = ip.GetAddressBytes();
            return ip.AddressFamily switch
            {
                AddressFamily.InterNetwork when bytes[0] == 10 => true,
                AddressFamily.InterNetwork when bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127 => true,
                AddressFamily.InterNetwork when bytes[0] == 169 && bytes[1] == 254 => true,
                AddressFamily.InterNetwork when bytes[0] == 172 && bytes[1] == 16 => true,
                AddressFamily.InterNetwork when bytes[0] == 192 && bytes[1] == 88 && bytes[2] == 99 => true,
                AddressFamily.InterNetwork when bytes[0] == 192 && bytes[1] == 168 => true,
                AddressFamily.InterNetwork when bytes[0] == 198 && bytes[1] == 18 => true,
                AddressFamily.InterNetwork when bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100 => true,
                AddressFamily.InterNetwork when bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113 => true,
                AddressFamily.InterNetwork when bytes[0] >= 233 => true,
                AddressFamily.InterNetworkV6 when ip.IsIPv6Teredo || ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || bytes[0] == 0 || bytes[0] >= 252 => true,
                _ => false
            };
        }
    }
}