#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using ThingsGateway.Foundation.Adapter.OPCDA.Rcw;

namespace ThingsGateway.Foundation.Adapter.OPCDA.Da;

internal class OpcServer : IDisposable
{

    private bool disposedValue;

    private IOPCServer m_OpcServer = null;

    internal OpcServer(string name, string host = "localhost")
    {
        Name = name;
        if (host.IsNullOrEmpty())
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

    internal OperResult<OpcGroup> AddGroup(string groupName)
    {
        return AddGroup(groupName, true, 1000, 0);
    }

    /// <returns></returns>
    internal OperResult<OpcGroup> AddGroup(string groupName, bool active, int reqUpdateRate, float deadBand)
    {
        if (null == m_OpcServer || IsConnected == false)
            return new OperResult<OpcGroup>("未初始化连接！");
        OpcGroup group = new OpcGroup(groupName, active, reqUpdateRate, deadBand);
        Guid riid = typeof(IOPCItemMgt).GUID;
        try
        {
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
                return new OperResult<OpcGroup>("添加OPC组错误，OPC服务器返回null");
            }
            return OperResult.CreateSuccessResult(group);
        }
        catch (Exception ex)
        {
            return new OperResult<OpcGroup>(ex);
        }
    }

    /// <summary>
    /// 获取节点
    /// </summary>
    internal OperResult<List<BrowseElement>> Browse(string itemId = null)
    {
        lock (this)
        {
            if (null == m_OpcServer || IsConnected == false)
                return new OperResult<List<BrowseElement>>("未初始化连接！");


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
            try
            {

                var server = m_OpcServer as IOPCBrowse;
                server.Browse(
                         itemId.IsNullOrEmpty() ? "" : itemId,
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
            }
            catch (Exception ex)
            {
                return new OperResult<List<BrowseElement>>(ex);
            }
            BrowseElement[] browseElements = Interop.GetBrowseElements(ref pElements, count, true);
            string stringUni = Marshal.PtrToStringUni(pContinuationPoint);
            Marshal.FreeCoTaskMem(pContinuationPoint);
            this.ProcessResults(browseElements, filterId);
            return OperResult.CreateSuccessResult(browseElements?.ToList());
        }
    }

    internal OperResult Connect()
    {
        if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Name))
        {
            var info = Discovery.OpcDiscovery.GetOpcServer(Name, Host);
            if (!info.IsSuccess)
            {
                return info;
            }
            object o = Comn.ComInterop.CreateInstance(info.Content.CLSID, Host);
            if (o == null)
            {
                return new(string.Format("{0}{1}无法创建com对象", info.Content.CLSID, Host));
            }
            m_OpcServer = (IOPCServer)o;
            IsConnected = true;
            return OperResult.CreateSuccessResult();
        }
        return new("应初始化Host与Name");
    }
    /// <summary>
    /// 服务器状态
    /// </summary>
    /// <returns></returns>
    internal OperResult<ServerStatus> GetServerStatus()
    {
        if (null == m_OpcServer || IsConnected == false)
            return new OperResult<ServerStatus>("未初始化连接！");
        IntPtr statusPtr = IntPtr.Zero;
        try
        {
            m_OpcServer?.GetStatus(out statusPtr);
            OPCSERVERSTATUS status;
            ServerStatus = new ServerStatus();
            if (statusPtr != IntPtr.Zero)
            {
                try
                {
                    object o = Marshal.PtrToStructure(statusPtr, typeof(OPCSERVERSTATUS));
                    if (null != o)
                    {
                        status = (OPCSERVERSTATUS)o;
                        ServerStatus.Version = status.wMajorVersion.ToString() + "." + status.wMinorVersion.ToString() + "." + status.wBuildNumber.ToString();
                        ServerStatus.ServerState = status.dwServerState;
                        ServerStatus.StartTime = Comn.Convert.FileTimeToDateTime(status.ftStartTime);
                        ServerStatus.CurrentTime = Comn.Convert.FileTimeToDateTime(status.ftCurrentTime);
                        ServerStatus.LastUpdateTime = Comn.Convert.FileTimeToDateTime(status.ftLastUpdateTime);
                        ServerStatus.VendorInfo = status.szVendorInfo;
                    }
                    IsConnected = true;
                    return OperResult.CreateSuccessResult(ServerStatus);
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    return new OperResult<ServerStatus>(ex);
                }

            }
            else
            {
                IsConnected = false;
                return new OperResult<ServerStatus>("获取状态失败");
            }
        }
        catch (Exception ex)
        {
            IsConnected = false;
            return new OperResult<ServerStatus>(ex);
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
