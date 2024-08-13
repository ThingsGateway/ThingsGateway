//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Net;

namespace ThingsGateway.NewLife.X;

/// <summary>网络结点扩展</summary>
public static class EndPointExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public static String ToAddress(this EndPoint endpoint)
    {
        return ((IPEndPoint)endpoint).ToAddress();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public static String ToAddress(this IPEndPoint endpoint)
    {
        return String.Format("{0}:{1}", endpoint.Address, endpoint.Port);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static IPEndPoint ToEndPoint(this String address)
    {
        var array = address.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
        if (array.Length != 2)
        {
            throw new Exception("Invalid endpoint address: " + address);
        }
        var ip = IPAddress.Parse(array[0]);
        var port = Int32.Parse(array[1]);
        return new IPEndPoint(ip, port);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="addresses"></param>
    /// <returns></returns>
    public static IEnumerable<IPEndPoint> ToEndPoints(this String addresses)
    {
        var array = addresses.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        var list = new List<IPEndPoint>();
        foreach (var item in array)
        {
            list.Add(item.ToEndPoint());
        }
        return list;
    }
}
