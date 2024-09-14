//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;

using ThingsGateway.Foundation.OpcDa.Rcw;

namespace ThingsGateway.Foundation.OpcDa.Da;
#pragma warning disable CA1416 // 验证平台兼容性

internal class OpcServer : IDisposable
{
    private bool disposedValue;

    private IOPCServer m_OpcServer = null;

    internal OpcServer(string name, string host = "localhost")
    {
        Name = name;
        if (string.IsNullOrEmpty(host))
        {
            Host = "localhost";
        }
        else
        {
            Host = host;
        }
    }

    internal string Host { get; private set; }
    internal bool IsConnected { get; private set; } = false;
    internal string Name { get; private set; }
    internal List<OpcGroup> OpcGroups { get; private set; } = new List<OpcGroup>(10);
    internal ServerStatus ServerStatus { get; private set; } = new ServerStatus();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal OpcGroup AddGroup(string groupName)
    {
        return AddGroup(groupName, true, 1000, 0);
    }

    /// <returns></returns>
    internal OpcGroup AddGroup(string groupName, bool active, int reqUpdateRate, float deadBand)
    {
        if (null == m_OpcServer || IsConnected == false)
            throw new("Uninitialized connection");
        OpcGroup group = new(groupName, active, reqUpdateRate, deadBand);
        Guid riid = typeof(IOPCItemMgt).GUID;
        m_OpcServer?.AddGroup(group.Name,
            group.IsActive ? 1 : 0,//IsActive
            group.RequestUpdateRate,//RequestedUpdateRate 1000ms
            group.ClientGroupHandle,
            group.TimeBias.AddrOfPinnedObject(),
            group.PercendDeadBand.AddrOfPinnedObject(),
            group.LCID,
            out group.serverGroupHandle,
            out group.revisedUpdateRate,
            ref riid,
            out group.groupPointer);
        if (group.groupPointer != null)
        {
            group.InitIoInterfaces(group.groupPointer);
            OpcGroups.Add(group);
        }
        else
        {
            throw new("Error adding OPC group, OPC server returns null");
        }
        return group;
    }

    /// <summary>
    /// 获取节点
    /// </summary>
    internal List<BrowseElement> Browse(string itemId = null)
    {
        lock (this)
        {
            if (null == m_OpcServer || IsConnected == false)
                throw new("Uninitialized connection");

            var count = 0;
            var moreElements = 0;

            var pContinuationPoint = IntPtr.Zero;
            var pElements = IntPtr.Zero;
            var filterId = new PropertyID[]
     {
                           new PropertyID(1),
                           new PropertyID(3),
                           new PropertyID(4),
                           new PropertyID(5),
                           new PropertyID(6),
                           new PropertyID(101),
                         };

            var server = m_OpcServer as IOPCBrowse;
            server.Browse(
                     string.IsNullOrEmpty(itemId) ? "" : itemId,
                 ref pContinuationPoint,
                 int.MaxValue,
                    OPCBROWSEFILTER.OPC_BROWSE_FILTER_ALL,
                      "",
                     "",
                     0,
                     1,
                     filterId.Length,
                     Interop.GetPropertyIDs(filterId),
                 out moreElements,
                 out count,
                 out pElements);
            BrowseElement[] browseElements = Interop.GetBrowseElements(ref pElements, count, true);
            string stringUni = Marshal.PtrToStringUni(pContinuationPoint);
            Marshal.FreeCoTaskMem(pContinuationPoint);
            ProcessResults(browseElements, filterId);
            return browseElements?.ToList();
        }
    }

    internal void Connect()
    {
        if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Name))
        {
            var info = Discovery.OpcDiscovery.GetOpcServer(Name, Host);
            object o = Comn.ComInterop.CreateInstance(info.CLSID, Host);
            if (o == null)
            {
                throw new(string.Format("{0} {1} Unable to create com object", info.CLSID, Host));
            }
            m_OpcServer = (IOPCServer)o;
            IsConnected = true;
        }
        else
            throw new("Host and Name should be initialized");
    }

    /// <summary>
    /// 服务器状态
    /// </summary>
    /// <returns></returns>
    internal ServerStatus GetServerStatus()
    {
        ServerStatus serverStatus = null;
        try
        {
            if (null == m_OpcServer || IsConnected == false)
                throw new("Uninitialized connection");
            IntPtr statusPtr = IntPtr.Zero;
            m_OpcServer?.GetStatus(out statusPtr);
            OPCSERVERSTATUS status;
            if (statusPtr != IntPtr.Zero)
            {
                object o = Marshal.PtrToStructure(statusPtr, typeof(OPCSERVERSTATUS));

                Marshal.FreeCoTaskMem(statusPtr);

                if (o != null)
                {
                    status = (OPCSERVERSTATUS)o;
                    serverStatus = new();
                    serverStatus.Version = $"{status.wMajorVersion}.{status.wMinorVersion}.{status.wBuildNumber}";
                    serverStatus.ServerState = status.dwServerState;
                    serverStatus.StartTime = Comn.Convert.FileTimeToDateTime(status.ftStartTime);
                    serverStatus.CurrentTime = Comn.Convert.FileTimeToDateTime(status.ftCurrentTime);
                    serverStatus.LastUpdateTime = Comn.Convert.FileTimeToDateTime(status.ftLastUpdateTime);
                    serverStatus.VendorInfo = status.szVendorInfo;
                    IsConnected = true;

                    return serverStatus;
                }
                else
                {
                    IsConnected = false;
                    throw new("GetServerStatus error");
                }
            }
            else
            {
                IsConnected = false;
                throw new("GetServerStatus error");
            }
        }
        finally
        {
            if (serverStatus != null)
                IsConnected = true;
            else
                IsConnected = false;
            ServerStatus = serverStatus;
        }
    }

    internal void RemoveGroup(OpcGroup group)
    {
        if (OpcGroups.Contains(group))
        {
            m_OpcServer?.RemoveGroup(group.ServerGroupHandle, 1);
            OpcGroups.Remove(group);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            try
            {
                for (int i = 0; i < OpcGroups.Count; i++)
                    RemoveGroup(OpcGroups[i]);
            }
            catch
            {
            }
            if (m_OpcServer != null)
            {
                Marshal.ReleaseComObject(m_OpcServer);
                m_OpcServer = null;
            }
            if (disposing)
            {
                OpcGroups.Clear();
            }
            disposedValue = true;
        }
    }

    private void ProcessResults(BrowseElement[] elements, PropertyID[] propertyIDs)
    {
        if (elements == null)
            return;
        foreach (BrowseElement element in elements)
        {
            if (element.Properties != null)
            {
                foreach (ItemProperty property in element.Properties)
                {
                    if (propertyIDs != null)
                    {
                        foreach (PropertyID propertyId in propertyIDs)
                        {
                            if (property.ID.Code == propertyId.Code)
                            {
                                property.ID = propertyId;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}