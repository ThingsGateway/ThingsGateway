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

using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

using ThingsGateway.Foundation.Adapter.OPCDA.Da;
using ThingsGateway.Foundation.Adapter.OPCDA.Rcw;

using Timer = System.Timers.Timer;

//部分非托管交互代码来自https://gitee.com/Zer0Day/opc-client与OPC基金会opcnet库，更改部分逻辑

namespace ThingsGateway.Foundation.Adapter.OPCDA;
/// <summary>
/// 订阅变化项
/// </summary>
/// <param name="values"></param>
public delegate void DataChangedEventHandler(List<ItemReadResult> values);
/// <summary>
/// OPCDAClient
/// </summary>
public class OPCDAClient : IDisposable
{
    /// <summary>
    /// LogAction
    /// </summary>
    private readonly Action<byte, object, string, Exception> _logAction;

    private readonly object checkLock = new();

    private Timer checkTimer;

    private int IsExit = 1;
    /// <summary>
    /// 当前保存的需订阅列表
    /// </summary>
    private Dictionary<string, List<OpcItem>> ItemDicts = new();

    private OpcServer m_server;
    private bool publicConnect;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public OPCDAClient(Action<byte, object, string, Exception> logAction)
    {
#if (NET6_0_OR_GREATER)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotSupportedException("不支持非windows系统");
        }
#endif
        _logAction = logAction;
    }

    /// <summary>
    /// 数据变化事件
    /// </summary>
    public event DataChangedEventHandler DataChangedHandler;

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public bool IsConnected => m_server?.IsConnected == true;

    /// <summary>
    /// 当前配置
    /// </summary>
    public OPCDANode OPCNode { get; private set; }
    private List<OpcGroup> Groups => m_server.OpcGroups;

    /// <summary>
    /// 添加节点，需要在连接成功后执行
    /// </summary>
    /// <param name="items">组名称/变量节点，注意每次添加的组名称不能相同</param>
    public void AddItems(Dictionary<string, List<OpcItem>> items)
    {
        if (IsExit == 1) throw new("对象已释放");
        foreach (var item in items)
        {
            if (IsExit == 1) throw new("对象已释放");
            try
            {
                var subscription = m_server.AddGroup(item.Key, true, OPCNode.UpdateRate, OPCNode.DeadBand);
                subscription.ActiveSubscribe = OPCNode.ActiveSubscribe;
                subscription.OnDataChanged += Subscription_OnDataChanged;
                subscription.OnReadCompleted += Subscription_OnDataChanged;

                var result = subscription.AddOpcItem(item.Value.ToArray());
                StringBuilder stringBuilder = new StringBuilder();
                if (result.Count > 0)
                {
                    foreach (var item1 in result)
                    {
                        stringBuilder.Append($"{item1.Item1.ItemID}：{item1.Item2}");
                    }
                    _logAction?.Invoke(3, this, $"添加变量失败：{stringBuilder}", null);
                }
                else
                {
                    ItemDicts.AddOrUpdate(item.Key, item.Value.Where(a => !result.Select(b => b.Item1).Contains(a)).ToList());
                }
            }
            catch (Exception ex)
            {
                _logAction?.Invoke(3, this, $"添加组失败：{ex.Message}", ex);
            }
        }
        for (int i = 0; i < Groups?.Count; i++)
        {
            var group = Groups[i];
            if (group != null)
            {
                if (group.OpcItems.Count == 0)
                {
                    ItemDicts.Remove(group.Name);
                    m_server.RemoveGroup(group);
                }
            }
        }
    }

    /// <summary>
    /// 设置节点并保存，每次重连会自动添加节点
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public Dictionary<string, List<OpcItem>> AddItemsWithSave(List<string> items)
    {
        int i = 0;
        ItemDicts = items.ToList().ConvertAll(o => new OpcItem(o)).ChunkTrivialBetter(OPCNode.GroupSize).ToDictionary(a => "default" + (i++));
        return ItemDicts;
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void Connect()
    {
        publicConnect = true;
        Interlocked.CompareExchange(ref IsExit, 0, 1);
        PrivateConnect();
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        Interlocked.CompareExchange(ref IsExit, 1, 0);
        PrivateDisconnect();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            PrivateDisconnect();
        }
        catch (Exception ex)
        {
            _logAction?.Invoke(3, this, $"连接释放失败：{ex.Message}", ex);
        }
        Interlocked.CompareExchange(ref IsExit, 1, 0);
    }

    /// <summary>
    /// 浏览节点
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public List<BrowseElement> GetBrowseElements(string itemId = null)
    {
        return this.m_server?.Browse(itemId);
    }

    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <returns></returns>
    public ServerStatus GetServerStatus()
    {
        return this.m_server?.GetServerStatus();
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="node"></param>
    public void Init(OPCDANode node)
    {
        if (node != null)
            OPCNode = node;
        checkTimer?.Stop();
        checkTimer?.Dispose();
        checkTimer = new Timer(OPCNode.CheckRate * 60 * 1000);
        checkTimer.Elapsed += CheckTimer_Elapsed;
        checkTimer.Start();
        try
        {
            m_server?.Dispose();
        }
        catch (Exception ex)
        {
            _logAction?.Invoke(3, this, $"连接释放失败：{ex.Message}", ex);
        }
        m_server = new OpcServer(OPCNode.OPCName, OPCNode.OPCIP);
    }

    /// <summary>
    /// 按OPC组读取组内变量，结果会在订阅事件中返回
    /// </summary>
    /// <param name="groupName">组名称，值为null时读取全部组</param>
    /// <returns></returns>
    public void ReadItemsWithGroup(string groupName = null)
    {
        PrivateConnect();
        {
            var groups = groupName != null ? Groups.Where(a => a.Name == groupName) : Groups;
            foreach (var group in groups)
            {
                if (group.OpcItems.Count > 0)
                {
                    group.ReadAsync();
                }
            }
        }
    }

    /// <summary>
    /// 移除节点
    /// </summary>
    /// <param name="items"></param>
    public void RemoveItems(List<string> items)
    {
        foreach (var item in items)
        {
            if (IsExit == 1) return;
            var opcGroups = Groups.Where(it => it.OpcItems.Any(a => a.ItemID == item));
            foreach (var opcGroup in opcGroups)
            {
                var tag = opcGroup.OpcItems.Where(a => item == a.ItemID);
                var result = opcGroup.RemoveItem(tag.ToArray());

                if (opcGroup.OpcItems.Count == 0)
                {
                    opcGroup.OnDataChanged -= Subscription_OnDataChanged;
                    ItemDicts.Remove(opcGroup.Name);
                    m_server.RemoveGroup(opcGroup);
                }
                else
                {
                    ItemDicts[opcGroup.Name].RemoveWhere(a => tag.Contains(a) && !result.Select(b => b.Item1).Contains(a));
                }
            }


        }
    }
    /// <inheritdoc/>
    public override string ToString()
    {
        return OPCNode?.ToString();
    }

    /// <summary>
    /// 批量写入值
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Tuple<bool, string>> WriteItem(Dictionary<string, object> writeInfos)
    {
        PrivateConnect();
        Dictionary<string, Tuple<bool, string>> results = new();

        var valueGroup = writeInfos.GroupBy(itemId =>
          {
              var group = Groups.FirstOrDefault(it => it.OpcItems.Any(a => a.ItemID == itemId.Key));
              return group;
          }).ToList();

        foreach (var item1 in valueGroup)
        {
            try
            {
                if (item1.Key == null)
                {
                    foreach (var item2 in item1)
                    {
                        results.AddOrUpdate(item2.Key, Tuple.Create(true, $"不存在该变量{item2.Key}"));
                    }
                }
                else
                {
                    List<int> serverHandles = new();
                    Dictionary<int, OpcItem> handleItems = new();
                    List<object> values = new();
                    foreach (var item2 in item1)
                    {
                        var opcItem = item1.Key.OpcItems.Where(it => it.ItemID == item2.Key).FirstOrDefault();
                        serverHandles.Add(opcItem.ServerHandle);
                        handleItems.AddOrUpdate(opcItem.ServerHandle, opcItem);
                        var rawWriteValue = item2.Value;
                        values.Add(rawWriteValue);
                    }

                    var result = item1.Key.Write(values.ToArray(), serverHandles.ToArray());
                    var data = item1.ToList();
                    foreach (var item2 in result)
                    {
                        results.AddOrUpdate(handleItems[item2.Item1].ItemID, Tuple.Create(true, $"错误代码{item2.Item2}"));
                    }
                }
                foreach (var item2 in item1)
                {
                    results.AddOrUpdate(item2.Key, Tuple.Create(false, $"成功"));
                }
            }
            catch (Exception ex)
            {
                var keys = writeInfos.Keys.ToList();
                foreach (var item in keys)
                {
                    results.AddOrUpdate(item, Tuple.Create(true, ex.Message));
                }
                return results;
            }
        }
        return results;
    }
    private void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        lock (checkLock)
        {

            if (IsExit == 0)
            {
                try
                {
                    var status = m_server.GetServerStatus();
                }
                catch
                {
                    if (IsExit == 0 && publicConnect)
                    {
                        try
                        {
                            PrivateConnect();
                            _logAction?.Invoke(1, this, $"重新链接成功", null);
                        }
                        catch (Exception ex)
                        {
                            _logAction?.Invoke(3, this, $"重新链接失败：{ex.Message}", ex);
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
    }

    private void PrivateAddItems()
    {
        try
        {
            AddItems(ItemDicts);
        }
        catch (Exception ex)
        {
            _logAction?.Invoke(3, this, $"添加点位失败：{ex.Message}", ex);
        }
    }
    private void PrivateConnect()
    {
        lock (this)
        {

            if (m_server?.IsConnected == true)
            {
                try
                {
                    var status = m_server.GetServerStatus();
                }
                catch
                {
                    try
                    {
                        var status1 = m_server.GetServerStatus();
                    }
                    catch
                    {

                        Init(OPCNode);
                        m_server?.Connect();
                        _logAction?.Invoke(1, this, $"{m_server.Host} - {m_server.Name} - 连接成功", null);
                        PrivateAddItems();
                    }
                }


            }
            else
            {
                Init(OPCNode);
                m_server?.Connect();
                _logAction?.Invoke(1, this, $"{m_server.Host} - {m_server.Name} - 连接成功", null);
                PrivateAddItems();
            }

        }
    }

    private void PrivateDisconnect()
    {
        lock (this)
        {
            if (IsConnected)
                _logAction?.Invoke(1, this, $"{m_server.Host} - {m_server.Name} - 断开连接", null);
            if (checkTimer != null)
            {
                checkTimer.Enabled = false;
                checkTimer.Stop();
            }

            try
            {
                m_server?.Dispose();
                m_server = null;
            }
            catch (Exception ex)
            {
                _logAction?.Invoke(3, this, $"连接释放失败：{ex.Message}", ex);
            }
        }
    }
    private void Subscription_OnDataChanged(List<ItemReadResult> values)
    {
        DataChangedHandler?.Invoke(values);
    }

}