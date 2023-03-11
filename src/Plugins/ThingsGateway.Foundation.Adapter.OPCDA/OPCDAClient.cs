using OpcDaClient.Da;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Json;

using Timer = System.Timers.Timer;

//部分非托管交互代码来自https://gitee.com/Zer0Day/opc-client与OPC基金会opcnet库，更改部分逻辑

namespace ThingsGateway.Foundation.Adapter.OPCDA
{
    public delegate void DataChangedEventHandler(List<ItemReadResult> values);

    public class OPCDAClient : DisposableObject
    {
        public OPCNode OPCNode;
        private ILog _logger;
        private EasyLock checkLock = new();
        private Timer checkTimer;
        private bool FirstConnect;
        private List<OpcGroup> Groups = new();
        private int IsQuit = 1;
        private OpcServer m_server;
        private IntelligentConcurrentQueue<ItemReadResult> results = new IntelligentConcurrentQueue<ItemReadResult>(100000);
        private Dictionary<string, List<OpcItem>> Tags = new();
        //定义组对象（订阅者）
        public OPCDAClient(ILog logger)
        {
            _logger = logger;
            EasyTask.Run(dataChangedHandlerInvoke);
        }

        public event DataChangedEventHandler DataChangedHandler;
        public bool IsConnected => m_server?.IsConnected == true;
        public void Connect()
        {
            Interlocked.CompareExchange(ref IsQuit, 0, 1);
            connect();
            FirstConnect = true;
            _logger.Trace("主动连接");
        }
        public void Disconnect()
        {
            Interlocked.CompareExchange(ref IsQuit, 1, 0);
            disconnect();
            _logger.Info(ToString(), "主动断开连接");
        }

        public string GetStatus()
        {
            return this.m_server.GetServerStatus().ToJson().FormatJson();
        }
        public OperResult<List<BrowseElement>> GetBrowse(string itemId=null)
        {
            return this.m_server.Browse(itemId);
        }
        public void Init(OPCNode node = null)
        {
            if (node != null)
                OPCNode = node;
            checkTimer?.Stop();
            checkTimer?.Dispose();
            checkTimer = new Timer(OPCNode.CheckRate);
            checkTimer.Elapsed += checkTimer_Elapsed;
            checkTimer.Start();
            m_server?.Dispose();
            m_server = new OpcDaClient.Da.OpcServer(OPCNode.OPCName, OPCNode.OPCIP);
        }

        public OperResult ReadSub(string groupName = null)
        {
            if (connect())
            {
                var groups = groupName != null ? Groups.Where(a => a.Name == groupName) : Groups;
                foreach (var group in Groups)
                {
                    var result = group.ReadAsync();
                    if (!result.IsSuccess)
                    {
                        return result;
                    }
                }
                return OperResult.CreateSuccessResult();
            }
            return new OperResult("未初始化连接");
        }

        /// <summary>
        /// 设置Tags,只需执行一次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tags"></param>
        /// <returns></returns>
        public Dictionary<string, List<OpcItem>> SetTags(List<string> tags)
        {
            int i = 0;
            Dictionary<string, List<OpcItem>> tagDicts = tags.ToList().ConvertAll(o => new OpcItem(o)
              ).ChunkTrivialBetter(OPCNode.GroupSize).ToDictionary(a => "default" + (i++));
            Tags = tagDicts;
            return tagDicts;
        }

        public OperResult Write(string valueName, object value)
        {
            if (connect())
            {
                try
                {
                    var group = Groups.FirstOrDefault(it => it.OpcItems.Any(a => a.ItemID == valueName));
                    if (group == null)
                        return new OperResult("不存在该变量" + valueName);
                    var item = group.OpcItems.Where(it => it.ItemID == valueName).FirstOrDefault();
                    int[] serverHandle = new int[1] { item.ServerHandle };
                    object[] Value = new object[1] { value };
                    int[] PErrors = new int[1];
                    group.WriteAsync(Value, serverHandle, out PErrors);
                    //itemvalue.Value = Convert.ChangeType(value, item.RunTimeDataType);
                    if (PErrors != null && PErrors.First() == 0)
                    {
                        return OperResult.CreateSuccessResult();
                    }
                    else
                    {
                        return new OperResult();
                    }
                }
                catch (Exception ex)
                {
                    return new OperResult(ex.Message);
                }
            }
            return new OperResult();
        }

        internal void AddTags()
        {
            Groups = new();
            if (IsQuit == 1 && connect()) return;
            foreach (var item in Tags)
            {
                if (IsQuit == 1) return;
                var subscription = m_server.AddGroup(item.Key, true, OPCNode.UpdateRate, OPCNode.DeadBand);
                if (subscription.IsSuccess)
                {
                    subscription.Content.ActiveSubscribe = OPCNode.ActiveSubscribe;
                    subscription.Content.OnDataChanged += Subscription_OnDataChanged;
                    subscription.Content.OnReadCompleted += Subscription_OnDataChanged;
                    Groups.Add(subscription.Content);

                    var result = subscription.Content.AddOpcItem(item.Value.ToArray());
                    if (!result.IsSuccess)
                    {
                        _logger?.Error(ToString(), result.Message);
                    }
                }

            }

        }

        protected override void Dispose(bool disposing)
        {
            disconnect();
            base.Dispose(disposing);
        }

        private void checkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (checkLock.IsWaitting) return;
            checkLock.Lock();
            try
            {
                if (IsQuit == 0)
                {
                    var status = m_server.GetServerStatus();
                    if (status.IsSuccess)
                    {
                        //_logger?.Trace(OPCNode.ToString() + "OPC状态检查正常！");
                    }
                    else
                    {
                        if (IsQuit == 0 && FirstConnect)
                        {
                            if (connect())
                            {
                                _logger?.Info(OPCNode.ToString() + "OPC重新链接成功！");
                            }
                            else
                            {

                            }
                        }

                    }
                }
                else
                {
                    var timeer = sender as Timer;
                    timeer.Enabled = false;
                    timeer.Stop();
                }
            }
            finally { checkLock.UnLock(); }

        }

        private bool connect()
        {
            lock (this)
            {
                try
                {
                    if (m_server?.IsConnected == true)
                    {
                        var status = m_server.GetServerStatus();
                        if (!status.IsSuccess)
                        {
                            var status1 = m_server.GetServerStatus();
                            if (!status1.IsSuccess)
                            {
                                _logger?.Error(status1.Message);
                                //失败重新连接
                                try
                                {
                                    disconnect();
                                    Init(OPCNode);
                                    var result = m_server?.Connect();
                                    if (result.IsSuccess)
                                    {
                                        AddTags();
                                    }
                                    else
                                    {
                                        _logger?.Error(result.Message);
                                        return IsConnected;
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    _logger?.Exception(ex2);
                                    return IsConnected;
                                }
                            }
                        }

                    }
                    else
                    {
                        disconnect();
                        Init(OPCNode);
                        var result = m_server?.Connect();
                        if (result.IsSuccess)
                        {
                            AddTags();
                        }
                        else
                        {
                            _logger?.Error(result.Message);
                            return IsConnected;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Exception(OPCNode.ToString(), ex);
                    return false;
                }
                return IsConnected;
            }
        }
        private async void dataChangedHandlerInvoke()
        {
            while (!DisposedValue)
            {
                if (results.Count > 0)
                    DataChangedHandler?.Invoke(results.ToListWithDequeue(results.Count));
                if (OPCNode == null)
                    await Task.Delay(1000);
                else
                    await Task.Delay(OPCNode.UpdateRate == 0 ? 1000 : OPCNode.UpdateRate);
            }
        }

        private void disconnect()
        {
            checkTimer.Enabled = false;
            checkTimer.Stop();
            try
            {
                m_server?.Dispose();
                m_server = null;
            }
            catch (Exception ex)
            {
                _logger?.Exception(OPCNode.ToString(), ex);
            }
        }
        private void Subscription_OnDataChanged(ItemReadResult[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                results.Enqueue(values[i]);
            }
        }

    }
}