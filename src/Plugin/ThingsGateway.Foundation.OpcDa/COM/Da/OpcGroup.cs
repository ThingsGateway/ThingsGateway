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

internal sealed class OpcGroup : IOPCDataCallback, IDisposable
{
    internal object groupPointer = null;
    internal int revisedUpdateRate = 0;
    internal int serverGroupHandle = 0;
    private static int _handle = 0;
    private bool _bSubscribe = false;
    private bool disposedValue;
    private int lcid = 0x0;
    private IOPCAsyncIO2 m_Async2IO = null;
    private IConnectionPoint m_ConnectionPoint = null;
    private int m_connectionpoint_cookie = 0;
    private IConnectionPointContainer m_ConnectionPointContainer = null;
    private IOPCItemMgt m_ItemManagement = null;
    private IOPCGroupStateMgt m_StateManagement = null;
    private IOPCSyncIO m_SyncIO = null;
    private GCHandle percendDeadBand = GCHandle.Alloc(0, GCHandleType.Pinned);
    private GCHandle timeBias = GCHandle.Alloc(0, GCHandleType.Pinned);

    internal OpcGroup(string name)
    {
        Name = name;
        ClientGroupHandle = ++_handle;
    }

    internal OpcGroup(string groupName, bool active, int reqUpdateRate, float deadBand)
    {
        Name = groupName;
        IsActive = active;
        RequestUpdateRate = reqUpdateRate;
        DeadBand = deadBand;
        ClientGroupHandle = ++_handle;
    }

    internal delegate void CancelCompletedHandler(int dwTransid, int hGroup);

    internal event CancelCompletedHandler OnCancelCompleted;

    internal event DataChangedHandler OnDataChanged;

    internal event ReadCompletedHandler OnReadCompleted;

    internal event WriteCompletedHandler OnWriteCompleted;

    internal bool ActiveSubscribe
    {
        get
        {
            return _bSubscribe;
        }
        set
        {
            _bSubscribe = value;
            ActiveDataChanged(_bSubscribe);
        }
    }

    internal int ClientGroupHandle { get; private set; }
    internal float DeadBand { get; set; } = 0.0f;
    internal object GroupPointer => groupPointer;

    internal bool IsActive { get; set; } = true;

    internal int LCID
    {
        get => lcid;
        set => lcid = value;
    }

    internal string Name { get; private set; } = string.Empty;
    internal List<OpcItem> OpcItems { get; private set; } = new List<OpcItem> { };

    internal GCHandle PercendDeadBand
    {
        get => percendDeadBand;
        set => percendDeadBand = value;
    }

    internal int RequestUpdateRate { get; set; } = 1000;
    internal int RevisedUpdateRate => revisedUpdateRate;
    internal int ServerGroupHandle => serverGroupHandle;

    internal GCHandle TimeBias
    {
        get => timeBias;
        set => timeBias = value;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void OnCancelComplete(int dwTransid, int hGroup)
    {
        OnCancelCompleted?.Invoke(dwTransid, hGroup);
    }

    public void OnDataChange(int dwTransid,
                            int hGroup,
                            int hrMasterquality,
                            int hrMastererror,
                            int dwCount,
                            int[] phClientItems,
                            object[] pvValues,
                            short[] pwQualities,
                            System.Runtime.InteropServices.ComTypes.FILETIME[] pftTimeStamps,
                            int[] pErrors)
    {
        List<ItemReadResult> itemChanged = new();
        for (int i = 0; i < dwCount; i++)
        {
            int index = OpcItems.FindIndex(x => x.ClientHandle == phClientItems[i]);
            if (index >= 0)
            {
                OpcItems[index].Value = pvValues[i];
                OpcItems[index].Quality = pwQualities[i];
                OpcItems[index].TimeStamp = Comn.Convert.FileTimeToDateTime(pftTimeStamps[i]);
                itemChanged.Add(new ItemReadResult
                {
                    Name = OpcItems[index].ItemID,
                    Value = pvValues[i],
                    Quality = pwQualities[i],
                    TimeStamp = OpcItems[index].TimeStamp
                });
            }
        }
        OnDataChanged?.Invoke(Name, ServerGroupHandle, itemChanged);
    }

    public void OnReadComplete(int dwTransid,
                                int hGroup,
                                int hrMasterquality,
                                int hrMastererror,
                                int dwCount,
                                int[] phClientItems,
                                object[] pvValues,
                                short[] pwQualities,
                                System.Runtime.InteropServices.ComTypes.FILETIME[] pftTimeStamps,
                                int[] pErrors)
    {
        List<ItemReadResult> itemChanged = new();
        for (int i = 0; i < dwCount; i++)
        {
            int index = OpcItems.FindIndex(x => x.ClientHandle == phClientItems[i]);
            if (index >= 0)
            {
                OpcItems[index].Value = pvValues[i];
                OpcItems[index].Quality = pwQualities[i];
                OpcItems[index].TimeStamp = Comn.Convert.FileTimeToDateTime(pftTimeStamps[i]);
                itemChanged.Add(new ItemReadResult
                {
                    Name = OpcItems[index].ItemID,
                    Value = pvValues[i],
                    Quality = pwQualities[i],
                    TimeStamp = OpcItems[index].TimeStamp
                });
            }
        }
        OnReadCompleted?.Invoke(Name, ServerGroupHandle, itemChanged);
    }

    public void OnWriteComplete(int dwTransid,
                                int hGroup,
                                int hrMastererr,
                                int dwCount,
                                int[] pClienthandles,
                                int[] pErrors)
    {
        List<ItemWriteResult> itemwrite = new();
        for (int i = 0; i < dwCount; i++)
        {
            int index = OpcItems.FindIndex(x => x.ClientHandle == pClienthandles[i]);
            if (index >= 0)
            {
                itemwrite.Add(new ItemWriteResult
                {
                    Name = OpcItems[index].ItemID,
                    Exception = pErrors[i]
                });
            }
        }
        OnWriteCompleted?.Invoke(Name, ServerGroupHandle, itemwrite);
    }

    internal List<Tuple<OpcItem, int>> AddOpcItem(OpcItem[] items)
    {
        IntPtr pResults = IntPtr.Zero;
        IntPtr pErrors = IntPtr.Zero;
        OPCITEMDEF[] itemDefyArray = new OPCITEMDEF[items.Length];
        int i = 0;
        int[] errors = new int[items.Length];
        int[] itemServerHandle = new int[items.Length];
        try
        {
            foreach (OpcItem item in items)
            {
                if (item != null)
                {
                    itemDefyArray[i].szAccessPath = item.AccessPath;
                    itemDefyArray[i].szItemID = item.ItemID;
                    itemDefyArray[i].bActive = item.IsActive ? 1 : 0;
                    itemDefyArray[i].hClient = item.ClientHandle;
                    itemDefyArray[i].dwBlobSize = item.BlobSize;
                    itemDefyArray[i].pBlob = item.Blob;
                    i++;
                }
            }
            //添加OPC项组
            m_ItemManagement?.AddItems(items.Length, itemDefyArray, out pResults, out pErrors);
            IntPtr Position = pResults;
            Marshal.Copy(pErrors, errors, 0, items.Length);
            List<Tuple<OpcItem, int>> results = new();
            for (int j = 0; j < items.Length; j++)
            {
                if (errors[j] == 0)
                {
                    if (j != 0)
                    {
                        Position = IntPtr.Add(Position, Marshal.SizeOf(typeof(OPCITEMRESULT)));
                    }
                    object o = Marshal.PtrToStructure(Position, typeof(OPCITEMRESULT));
                    if (o != null)
                    {
                        var result = (OPCITEMRESULT)o;

                        items[j].RunTimeDataType = result.vtCanonicalDataType;
                        itemServerHandle[j] = items[j].ServerHandle = result.hServer;
                        Marshal.DestroyStructure(Position, typeof(OPCITEMRESULT));
                        OpcItems.Add(items[j]);
                    }
                }
                else
                {
                    results.Add(Tuple.Create(items[j], errors[j]));
                }
            }
            return results;
        }
        finally
        {
            if (pResults != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pResults);
            }
            if (pErrors != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pErrors);
            }
        }
    }

    /// <summary>
    /// 建立连接
    /// </summary>
    /// <param name="handle"></param>
    internal void InitIoInterfaces(object handle)
    {
        groupPointer = handle;
        m_ItemManagement = (IOPCItemMgt)groupPointer;
        m_Async2IO = (IOPCAsyncIO2)groupPointer;
        m_SyncIO = (IOPCSyncIO)groupPointer;
        m_StateManagement = (IOPCGroupStateMgt)groupPointer;
        m_ConnectionPointContainer = (IConnectionPointContainer)groupPointer;
        Guid iid = typeof(IOPCDataCallback).GUID;
        m_ConnectionPointContainer.FindConnectionPoint(ref iid, out m_ConnectionPoint);
        //创建客户端与服务端之间的连接
        m_ConnectionPoint.Advise(this, out m_connectionpoint_cookie);
    }

    /// <summary>
    /// 组读取
    /// </summary>
    /// <exception cref="ExternalException"></exception>
    internal void ReadAsync()
    {
        IntPtr pErrors = IntPtr.Zero;
        try
        {
            if (m_Async2IO != null)
            {
                int[] serverHandle = new int[OpcItems.Count];
                int[] PErrors = new int[OpcItems.Count];
                for (int j = 0; j < OpcItems.Count; j++)
                {
                    serverHandle[j] = OpcItems[j].ServerHandle;
                }
                m_Async2IO.Read(OpcItems.Count, serverHandle, 2, out int cancelId, out pErrors);
                Marshal.Copy(pErrors, PErrors, 0, OpcItems.Count);
                if (PErrors.Any(a => a > 0))
                {
                    throw new("Read fail，Code：" + pErrors);
                }
            }
            else
                throw new ArgumentNullException(nameof(m_Async2IO));
        }
        finally
        {
            if (pErrors != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pErrors);
            }
        }
    }

    internal List<Tuple<OpcItem, int>> RemoveItem(OpcItem[] items)
    {
        IntPtr pErrors = IntPtr.Zero;
        int[] errors = new int[items.Length];
        int[] handles = new int[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            handles[i] = items[i].ServerHandle;
        }
        try
        {
            m_ItemManagement?.RemoveItems(handles.Length, handles, out pErrors);
            Marshal.Copy(pErrors, errors, 0, items.Length);
        }
        finally
        {
            if (pErrors != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pErrors);
            }
        }
        List<Tuple<OpcItem, int>> results = new();
        for (int i = 0; i < errors.Length; i++)
        {
            if (errors[i] != 0)
            {
                results.Add(Tuple.Create(items[i], errors[i]));
            }
            else
            {
                OpcItems.Remove(items[i]);
            }
        }
        return results;
    }

    internal List<Tuple<int, int>> Write(object[] values, int[] serverHandle)
    {
        IntPtr pErrors = IntPtr.Zero;
        var errors = new int[values.Length];
        if (m_Async2IO != null)
        {
            try
            {
                m_SyncIO.Write(values.Length, serverHandle, values, out pErrors);
                Marshal.Copy(pErrors, errors, 0, values.Length);
                List<Tuple<int, int>> results = new();
                for (int i = 0; i < errors.Length; i++)
                {
                    if (errors[i] != 0)
                    {
                        results.Add(Tuple.Create(serverHandle[i], errors[i]));
                    }
                }
                return results;
            }
            finally
            {
                if (pErrors != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pErrors);
                }
            }
        }
        else
            throw new ArgumentNullException(nameof(m_Async2IO));
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (TimeBias.IsAllocated)
            {
                TimeBias.Free();
            }
            if (PercendDeadBand.IsAllocated)
            {
                PercendDeadBand.Free();
            }
            ActiveSubscribe = false;
            m_ConnectionPoint?.Unadvise(m_connectionpoint_cookie);
            m_connectionpoint_cookie = 0;
            if (null != m_ConnectionPoint) Marshal.ReleaseComObject(m_ConnectionPoint);
            m_ConnectionPoint = null;
            if (null != m_ConnectionPointContainer) Marshal.ReleaseComObject(m_ConnectionPointContainer);
            m_ConnectionPointContainer = null;
            if (m_Async2IO != null)
            {
                Marshal.ReleaseComObject(m_Async2IO);
                m_Async2IO = null;
            }
            if (m_SyncIO != null)
            {
                Marshal.ReleaseComObject(m_SyncIO);
                m_SyncIO = null;
            }
            if (m_StateManagement != null)
            {
                Marshal.ReleaseComObject(m_StateManagement);
                m_StateManagement = null;
            }
            if (groupPointer != null)
            {
                Marshal.ReleaseComObject(groupPointer);
                groupPointer = null;
            }
            m_ItemManagement = null;
            disposedValue = true;
        }
    }

    private void ActiveDataChanged(bool active)
    {
        IntPtr pRequestedUpdateRate = IntPtr.Zero;
        IntPtr hClientGroup = IntPtr.Zero;
        IntPtr pTimeBias = IntPtr.Zero;
        IntPtr pDeadband = IntPtr.Zero;
        IntPtr pLCID = IntPtr.Zero;
        int nActive = 0;
        GCHandle hActive = GCHandle.Alloc(nActive, GCHandleType.Pinned);
        hActive.Target = active ? 1 : 0;
        try
        {
            m_StateManagement?.SetState(pRequestedUpdateRate,
                                        out int nRevUpdateRate,
                                        hActive.AddrOfPinnedObject(),
                                        pTimeBias,
                                        pDeadband,
                                        pLCID,
                                        hClientGroup);
        }
        finally
        {
            hActive.Free();
        }
    }
}
