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

using System.Collections;
using System.Text;

using ThingsGateway.Foundation.Adapter.OPCDA.Rcw;

namespace ThingsGateway.Foundation.Adapter.OPCDA.Discovery;
/// <summary>
/// OpcDiscovery
/// </summary>
internal class OpcDiscovery
{
    private static readonly Guid CATID_OPC_DA10 = new("63D5F430-CFE4-11d1-B2C8-0060083BA1FB");

    private static readonly Guid CATID_OPC_DA20 = new("63D5F432-CFE4-11d1-B2C8-0060083BA1FB");

    private static readonly Guid CATID_OPC_DA30 = new("CC603642-66D7-48f1-B69A-B625E73652D7");

    private static readonly Guid OPCEnumCLSID = new("13486D51-4821-11D2-A494-3CB306C10000");
    /// <summary>
    /// GetOpcServer
    /// </summary>
    /// <param name="serverName"></param>
    /// <param name="host"></param>
    /// <returns></returns>
    internal static ServerInfo GetOpcServer(string serverName, string host)
    {

        if (string.IsNullOrEmpty(serverName))
        {
            throw new("检索失败，需提供OPCName");
        }
        ServerInfo result = null;
        ServerInfo[] serverInfos = null;
        object o_Server = Comn.ComInterop.CreateInstance(OPCEnumCLSID, host);
        if (o_Server == null)
            throw new("检索失败，请检查是否安装OPC Runtime");
        try
        {
            Guid catid = CATID_OPC_DA20;

            //两种方式，兼容国产部分OPCServer不支持IOPCServerList2的情况
            try
            {
                IOPCServerList2 m_server2 = (IOPCServerList2)o_Server;
                GetIOPCServerList(ref result, ref serverInfos, serverName, host, m_server2, catid);
                if (result == null)
                {
                    IOPCServerList m_server = (IOPCServerList)o_Server;
                    GetIOPCServerList(ref result, ref serverInfos, serverName, host, m_server, catid);
                }
            }
            catch
            {
                IOPCServerList m_server = (IOPCServerList)o_Server;
                GetIOPCServerList(ref result, ref serverInfos, serverName, host, m_server, catid);
            }
            if (result == null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in serverInfos)
                {
                    sb.AppendLine(item.ToString());
                }
                throw new($"无法创建OPCServer连接，请检查OPC名称是否一致，以下为{host}中的OPC列表:"
                    + Environment.NewLine +
                  sb.ToString()
                    );
            }
            return result;
        }
        finally
        {
            Comn.ComInterop.RealseComServer(o_Server);
            o_Server = null;
        }

    }

    private static void GetIOPCServerList(ref ServerInfo result, ref ServerInfo[] serverInfos, string serverName, string host, IOPCServerList m_server, Guid catid)
    {
        object enumerator = null;
        //2
        m_server.EnumClassesOfCategories(
            1,
            new Guid[] { catid },
            0,
            null,
            out enumerator);
        Guid[] clsids = Comn.ComInterop.ReadClasses((IEnumGUID)enumerator);
        //释放
        Comn.ComInterop.RealseComServer(enumerator);
        enumerator = null;

        serverInfos = GetServerDetails(clsids?.ToArray(), host, m_server);
        for (int i = 0; i < serverInfos.Length; i++)
        {
            if (serverInfos[i].CLSID.ToString().ToLower() == serverName.ToLower() ||
                    serverInfos[i].ProgID.ToLower() == serverName.ToLower() ||
                    serverInfos[i].VerIndProgID.ToLower() == serverName.ToLower())
            {
                result = serverInfos[i];
                break;
            }
        }

    }

    private static void GetIOPCServerList(ref ServerInfo result, ref ServerInfo[] serverInfos, string serverName, string host, IOPCServerList2 m_server, Guid catid)
    {
        //1
        IOPCEnumGUID enumerator = null;
        m_server.EnumClassesOfCategories(
            1,
            new Guid[] { catid },
            0,
            null,
            out enumerator);
        Guid[] clsids = Comn.ComInterop.ReadClasses(enumerator);
        //释放
        Comn.ComInterop.RealseComServer(enumerator);
        enumerator = null;

        serverInfos = GetServerDetails(clsids?.ToArray(), host, m_server);
        for (int i = 0; i < serverInfos.Length; i++)
        {
            if (serverInfos[i].CLSID.ToString().ToLower() == serverName.ToLower() ||
                    serverInfos[i].ProgID.ToLower() == serverName.ToLower() ||
                    serverInfos[i].VerIndProgID.ToLower() == serverName.ToLower())
            {
                result = serverInfos[i];
                break;
            }
        }

    }


    private static ServerInfo[] GetServerDetails(Guid[] clsids, string host, IOPCServerList m_server)
    {
        ArrayList servers = new ArrayList();
        for (int i = 0; i < clsids?.Length; i++)
        {
            Guid clsid = clsids[i];
            try
            {
                string progID = null;
                string description = null;
                string verIndProgID = null;
                ServerInfo server1 = new();

                server1.Host = host;
                server1.CLSID = clsid;

                m_server?.GetClassDetails(
                    ref clsid,
                    out progID,
                    out description,
                    out verIndProgID);
                if (verIndProgID != null)
                {
                    server1.VerIndProgID = verIndProgID;
                }
                else if (progID != null)
                {
                    server1.ProgID = progID;
                }
                if (description != null)
                {
                    server1.Description = description;
                }
                servers.Add(server1);
            }
            catch
            {
            }
        }
        return (ServerInfo[])servers.ToArray(typeof(ServerInfo));
    }

    private static ServerInfo[] GetServerDetails(Guid[] clsids, string host, IOPCServerList2 m_server)
    {
        ArrayList servers = new ArrayList();
        for (int i = 0; i < clsids?.Length; i++)
        {
            Guid clsid = clsids[i];
            try
            {
                string progID = null;
                string description = null;
                string verIndProgID = null;
                ServerInfo server1 = new();

                server1.Host = host;
                server1.CLSID = clsid;

                m_server?.GetClassDetails(
                    ref clsid,
                    out progID,
                    out description,
                    out verIndProgID);
                if (verIndProgID != null)
                {
                    server1.VerIndProgID = verIndProgID;
                }
                else if (progID != null)
                {
                    server1.ProgID = progID;
                }
                if (description != null)
                {
                    server1.Description = description;
                }
                servers.Add(server1);
            }
            catch
            {
            }
        }
        return (ServerInfo[])servers.ToArray(typeof(ServerInfo));
    }


}


internal class ServerInfo
{
    internal Guid CLSID { get; set; }
    internal string Description { get; set; } = string.Empty;
    internal string Host { get; set; } = string.Empty;
    internal string ProgID { get; set; } = string.Empty;
    internal string VerIndProgID { get; set; } = string.Empty;

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{nameof(CLSID)}:{CLSID}");
        stringBuilder.AppendLine($"{nameof(Description)}:{Description}");
        stringBuilder.AppendLine($"{nameof(Host)}:{Host}");
        stringBuilder.AppendLine($"{nameof(ProgID)}:{ProgID}");
        stringBuilder.AppendLine($"{nameof(VerIndProgID)}:{VerIndProgID}");
        return stringBuilder.ToString();
    }
}
