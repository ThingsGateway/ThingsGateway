using OpcRcw.Comn;

using System.Collections;
using System.Linq;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Json;

namespace OpcDaClient.Discovery
{
    public class OpcDiscovery
    {
        private static readonly Guid CATID_OPC_DA10 = new("63D5F430-CFE4-11d1-B2C8-0060083BA1FB");

        private static readonly Guid CATID_OPC_DA20 = new("63D5F432-CFE4-11d1-B2C8-0060083BA1FB");

        private static readonly Guid CATID_OPC_DA30 = new("CC603642-66D7-48f1-B69A-B625E73652D7");

        private static readonly Guid OPCEnumCLSID = new("13486D51-4821-11D2-A494-3CB306C10000");

        public static OperResult<ServerInfo> GetOpcServer(string serverName, string host)
        {
            try
            {
                if (serverName.IsNullOrEmpty())
                {
                    return new OperResult<ServerInfo>("检索失败，需提供OPCName");
                }
                ServerInfo result = null;
                ServerInfo[] serverInfos = null;
                object o_Server = Comn.ComInterop.CreateInstance(OPCEnumCLSID, host);
                if (o_Server == null)
                    return new OperResult<ServerInfo>("检索失败，请检查是否安装OPC Runtime");
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
                    catch (Exception)
                    {
                        IOPCServerList m_server = (IOPCServerList)o_Server;
                        GetIOPCServerList(ref result, ref serverInfos, serverName, host, m_server, catid);
                    }
                    if (result == null)
                    {
                        return new OperResult<ServerInfo>($"无法创建OPCServer连接，请检查OPC名称是否一致，以下为Host{host}中的OPC列表:"
                            +
                            serverInfos.ToJson().FormatJson()
                            );
                    }
                    return OperResult.CreateSuccessResult(result);
                }
                finally
                {
                    Comn.ComInterop.RealseComServer(o_Server);
                    o_Server = null;
                }
            }
            catch (Exception ex)
            {
                return new OperResult<ServerInfo>(ex);
            }
        }

        private static void GetIOPCServerList(ref ServerInfo result, ref ServerInfo[] serverInfos, string serverName, string host, IOPCServerList m_server, Guid catid)
        {
            object enumerator = null;
            //2
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            m_server.EnumClassesOfCategories(
                1,
                new Guid[] { catid },
                0,
                null,
                out enumerator);
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
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
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            m_server.EnumClassesOfCategories(
                1,
                new Guid[] { catid },
                0,
                null,
                out enumerator);
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
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
    public class ServerInfo
    {
        public Guid CLSID { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string ProgID { get; set; } = string.Empty;
        public string VerIndProgID { get; set; } = string.Empty;
    }
}
