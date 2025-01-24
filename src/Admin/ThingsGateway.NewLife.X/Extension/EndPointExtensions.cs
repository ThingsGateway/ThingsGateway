using System.Net;


namespace ThingsGateway.NewLife.Extension;

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
    private static readonly String[] SplitColon = new String[] { ":" };
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static IPEndPoint ToEndPoint(this String address)
    {
        var array = address.Split(SplitColon, StringSplitOptions.RemoveEmptyEntries);
        if (array.Length != 2)
        {
            throw new Exception("Invalid endpoint address: " + address);
        }
        var ip = IPAddress.Parse(array[0]);
        var port = Int32.Parse(array[1]);
        return new IPEndPoint(ip, port);
    }

    private static readonly String[] SplitComma = new String[] { "," };
    /// <summary>
    /// 
    /// </summary>
    /// <param name="addresses"></param>
    /// <returns></returns>
    public static IEnumerable<IPEndPoint> ToEndPoints(this String addresses)
    {
        var array = addresses.Split(SplitComma, StringSplitOptions.RemoveEmptyEntries);
        var list = new List<IPEndPoint>();
        foreach (var item in array)
        {
            list.Add(item.ToEndPoint());
        }
        return list;
    }
}
