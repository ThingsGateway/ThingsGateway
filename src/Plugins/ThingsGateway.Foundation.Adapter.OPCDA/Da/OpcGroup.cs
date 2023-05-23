#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion



using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using ThingsGateway.Foundation;

namespace OpcDaClient.Da
{
    public class OpcGroup : OpcRcw.Da.IOPCDataCallback, IDisposable
    {
        private OpcRcw.Da.IOPCGroupStateMgt m_StateManagement = null;
        private OpcRcw.Comn.IConnectionPointContainer m_ConnectionPointContainer = null;
        private OpcRcw.Comn.IConnectionPoint m_ConnectionPoint = null;
        private OpcRcw.Da.IOPCSyncIO m_SyncIO = null;
        private OpcRcw.Da.IOPCAsyncIO2 m_Async2IO = null;
        private OpcRcw.Da.IOPCItemMgt m_ItemManagement = null;
        private int m_connectionpoint_cookie = 0;

        public string Name { get; private set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int RequestUpdateRate { get; set; } = 1000;
        public int RevisedUpdateRate
        {
            get { return revisedUpdateRate; }
            set { revisedUpdateRate = value; }
        }
        public int ClientGroupHandle { get; private set; }
        public int ServerGroupHandle
        {
            get { return serverGroupHandle; }
            set { serverGroupHandle = value; }
        }
        public float DeadBand { get; set; } = 0.0f;
        public GCHandle TimeBias
        {
            get
            {
                return timeBias;
            }
            set
            {
                timeBias = value;
            }
        }
        public GCHandle PercendDeadBand
        {
            get
            {
                return percendDeadBand;
            }
            set
            {
                percendDeadBand = value;
            }
        }
        public int LCID
        {
            get
            {
                return lcid;
            }
            set
            {
                lcid = value;
            }
        }
        public object GroupPointer
        {
            get { return groupPointer; }
            set { groupPointer = value; }
        }

        private bool _bSubscribe = false;

        public bool ActiveSubscribe
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

        internal object groupPointer = null;
        internal int revisedUpdateRate = 0;
        internal int serverGroupHandle = 0;

        private static int _handle = 0;
        private int lcid = 0x0;
        private GCHandle timeBias = GCHandle.Alloc(0, GCHandleType.Pinned);
        private GCHandle percendDeadBand = GCHandle.Alloc(0, GCHandleType.Pinned);

        public List<OpcItem> OpcItems { get; private set; } = new List<OpcItem> { };
        private bool disposedValue;

        public event OnDataChangedHandler OnDataChanged;
        public event OnWriteCompletedHandler OnWriteCompleted;
        public event OnReadCompletedHandler OnReadCompleted;

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

        public OperResult AddOpcItem(OpcItem[] items)
        {
            IntPtr pResults = IntPtr.Zero;
            IntPtr pErrors = IntPtr.Zero;
            OpcRcw.Da.OPCITEMDEF[] itemDefyArray = new OpcRcw.Da.OPCITEMDEF[items.Length];
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
                IntPtr Pos = pResults;
                Marshal.Copy(pErrors, errors, 0, items.Length);
                StringBuilder stringBuilder = new();
                for (int j = 0; j < items.Length; j++)
                {
                    if (errors[j] == 0)
                    {
                        if (j != 0)
                        {
                            Pos = IntPtr.Add(Pos, Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMRESULT)));
                        }
                        object o = Marshal.PtrToStructure(Pos, typeof(OpcRcw.Da.OPCITEMRESULT));
                        if (o != null)
                        {
                            var result = (OpcRcw.Da.OPCITEMRESULT)o;

                            items[j].RunTimeDataType = result.vtCanonicalDataType;
                            itemServerHandle[j] = items[j].ServerHandle = result.hServer;
                            Marshal.DestroyStructure(Pos, typeof(OpcRcw.Da.OPCITEMRESULT));
                            OpcItems.Add(items[j]);
                        }
                    }
                    else
                    {
                        stringBuilder.AppendLine(items[j].ItemID + "添加失败,错误代码:" + errors[j]);
                    }
                }
                if (stringBuilder.Length > 0)
                {
                    return new OperResult(stringBuilder.ToString());
                }
                else
                {
                    return OperResult.CreateSuccessResult();
                }
            }
            catch (COMException ex)
            {
                return new OperResult(ex);
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

        public OperResult RemoveItem(OpcItem[] items)
        {
            IntPtr pErrors = IntPtr.Zero;
            bool[] result = new bool[items.Length];
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
            StringBuilder stringBuilder = new();
            for (int i = 0; i < errors.Length; i++)
            {
                if (errors[i] != 0)
                {
                    stringBuilder.AppendLine(items[i].ItemID + "添加失败,错误代码:" + errors[i]);
                }
            }
            if (stringBuilder.Length > 0)
            {
                return new OperResult(stringBuilder.ToString());
            }
            else
            {
                return OperResult.CreateSuccessResult();
            }
        }

        /// <summary>
        /// 组读取
        /// </summary>
        /// <exception cref="ExternalException"></exception>
        public OperResult ReadAsync()
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
                    int cancelId = 0;
                    m_Async2IO.Read(OpcItems.Count, serverHandle, 2, out cancelId, out pErrors);
                    Marshal.Copy(pErrors, PErrors, 0, OpcItems.Count);
                    return OperResult.CreateSuccessResult();
                }
                else
                    return new OperResult("连接无效");
            }
            catch (COMException ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                if (pErrors != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pErrors);
                }
            }
        }

        public OperResult WriteAsync(object[] values, int[] serverHandle, out int[] errors)
        {
            IntPtr pErrors = IntPtr.Zero;
            errors = new int[values.Length];
            if (m_Async2IO != null)
            {
                try
                {
                    int cancelId, transactionID = 0;
                    m_Async2IO.Write(values.Length, serverHandle, values, transactionID, out cancelId, out pErrors);
                    Marshal.Copy(pErrors, errors, 0, values.Length);
                    return OperResult.CreateSuccessResult();
                }
                catch (COMException ex)
                {
                    return new OperResult(ex);
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
                return new OperResult("连接无效");
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="handle"></param>
        internal void InitIoInterfaces(object handle)
        {
            groupPointer = handle;
            m_ItemManagement = (OpcRcw.Da.IOPCItemMgt)groupPointer;
            m_Async2IO = (OpcRcw.Da.IOPCAsyncIO2)groupPointer;
            m_SyncIO = (OpcRcw.Da.IOPCSyncIO)groupPointer;
            m_StateManagement = (OpcRcw.Da.IOPCGroupStateMgt)groupPointer;
            m_ConnectionPointContainer = (OpcRcw.Comn.IConnectionPointContainer)groupPointer;
            Guid iid = typeof(OpcRcw.Da.IOPCDataCallback).GUID;
            m_ConnectionPointContainer.FindConnectionPoint(ref iid, out m_ConnectionPoint);
            //创建客户端与服务端之间的连接
            m_ConnectionPoint.Advise(this, out m_connectionpoint_cookie);
        }


        private OperResult ActiveDataChanged(bool active)
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
                int nRevUpdateRate = 0;
                m_StateManagement?.SetState(pRequestedUpdateRate,
                                            out nRevUpdateRate,
                                            hActive.AddrOfPinnedObject(),
                                            pTimeBias,
                                            pDeadband,
                                            pLCID,
                                            hClientGroup);
                return OperResult.CreateSuccessResult();
            }
            catch (COMException ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                hActive.Free();
            }
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
            List<ItemReadResult> itemChanged = new List<ItemReadResult>();
            for (int i = 0; i < dwCount; i++)
            {
                int index = OpcItems.FindIndex(x => x.ClientHandle == phClientItems[i]);
                if (index >= 0)
                {
                    OpcItems[index].Value = pvValues[i];
                    OpcItems[index].Quality = pwQualities[i];
                    OpcItems[index].TimeStamp = OpcDaClient.Comn.Convert.FileTimeToDateTime(pftTimeStamps[i]);
                    itemChanged.Add(new ItemReadResult
                    {
                        Name = OpcItems[index].ItemID,
                        Value = pvValues[i],
                        Quality = pwQualities[i],
                        TimeStamp = OpcItems[index].TimeStamp
                    });
                }
            }
            OnDataChanged?.Invoke(itemChanged.ToArray());
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
            List<ItemReadResult> itemChanged = new List<ItemReadResult>();
            for (int i = 0; i < dwCount; i++)
            {
                int index = OpcItems.FindIndex(x => x.ClientHandle == phClientItems[i]);
                if (index >= 0)
                {
                    OpcItems[index].Value = pvValues[i];
                    OpcItems[index].Quality = pwQualities[i];
                    OpcItems[index].TimeStamp = OpcDaClient.Comn.Convert.FileTimeToDateTime(pftTimeStamps[i]);
                    itemChanged.Add(new ItemReadResult
                    {
                        Name = OpcItems[index].ItemID,
                        Value = pvValues[i],
                        Quality = pwQualities[i],
                        TimeStamp = OpcItems[index].TimeStamp
                    });
                }
            }
            OnReadCompleted?.Invoke(itemChanged.ToArray());
        }

        public void OnWriteComplete(int dwTransid,
                                    int hGroup,
                                    int hrMastererr,
                                    int dwCount,
                                    int[] pClienthandles,
                                    int[] pErrors)
        {
            List<ItemWriteResult> itemwrite = new List<ItemWriteResult>();
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
            OnWriteCompleted?.Invoke(itemwrite.ToArray());
        }

        public delegate void CancelCompletedHandler(int dwTransid, int hGroup);
        public event CancelCompletedHandler OnCancelCompleted;
        public void OnCancelComplete(int dwTransid, int hGroup)
        {
            OnCancelCompleted?.Invoke(dwTransid, hGroup);
        }

        protected virtual void Dispose(bool disposing)
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
#pragma warning disable CA1416 // 验证平台兼容性
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
#pragma warning restore CA1416 // 验证平台兼容性
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
