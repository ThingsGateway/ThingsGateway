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
using System.Text;
using System.Timers;

using ThingsGateway.Foundation.OpcDa.Da;
using ThingsGateway.Foundation.OpcDa.Rcw;

using Timer = System.Timers.Timer;

//部分非托管交互代码来自https://gitee.com/Zer0Day/opc-client与OPC基金会opcnet库，更改部分逻辑

namespace ThingsGateway.Foundation.OpcDa;

/// <summary>
/// 日志输出
/// </summary>
public delegate void LogEventHandler(byte level, object sender, string message, Exception ex);

/// <summary>
/// OPCDAClient
/// </summary>
public class OpcDaMaster : IDisposable
{
    /// <summary>
    /// OPCDAClient
    /// </summary>
    ~OpcDaMaster()
    {
        Dispose();
    }

    #region 配置项

    /// <summary>
    /// 当前配置
    /// </summary>
    public OpcDaProperty OpcDaProperty { get; private set; }

    /// <summary>
    /// 数据变化事件
    /// </summary>
    public event DataChangedHandler DataChangedHandler;

    /// <summary>
    /// 日志输出
    /// </summary>
    public LogEventHandler LogEvent;

    #endregion 配置项

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
    public OpcDaMaster()
    {
#if (NETSTANDARD2_0_OR_GREATER)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotSupportedException("Non Windows systems are not supported");
        }
#endif
    }

    /// <summary>
    /// 获取变量说明
    /// </summary>
    /// <returns></returns>
    public string GetAddressDescription()
    {
        return "ItemName";
    }

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public bool IsConnected => m_server?.IsConnected == true;

    private List<OpcGroup> Groups => m_server.OpcGroups;

    /// <summary>
    /// 添加节点，需要在连接成功后执行
    /// </summary>
    /// <param name="items">组名称/变量节点，注意每次添加的组名称不能相同</param>
    public void AddItems(Dictionary<string, List<OpcItem>> items)
    {
        if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaMaster));
        foreach (var item in items)
        {
            if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaMaster));
            try
            {
                var subscription = m_server.AddGroup(item.Key, true, OpcDaProperty.UpdateRate, OpcDaProperty.DeadBand);
                subscription.ActiveSubscribe = OpcDaProperty.ActiveSubscribe;
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
                    LogEvent?.Invoke(3, this, $"Failed to add variable：{stringBuilder}", null);
                }
                else
                {
                    ItemDicts.AddOrUpdate(item.Key, item.Value.Where(a => !result.Select(b => b.Item1).Contains(a)).ToList());
                }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(3, this, $"Failed to add group：{ex.Message}", ex);
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
        ItemDicts = items.ConvertAll(o => new OpcItem(o)).ChunkTrivialBetter(OpcDaProperty.GroupSize).ToDictionary(a => "default" + (i++));
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
            LogEvent?.Invoke(3, this, $"Disconnect warn：{ex.Message}", ex);
        }
        checkTimer?.Dispose();
        Interlocked.CompareExchange(ref IsExit, 1, 0);
    }

    /// <summary>
    /// 浏览节点
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public List<BrowseElement> GetBrowseElements(string itemId = null)
    {
        return m_server?.Browse(itemId);
    }

    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <returns></returns>
    public ServerStatus GetServerStatus()
    {
        return m_server?.GetServerStatus();
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="config"></param>
    public void Init(OpcDaProperty config)
    {
        if (config != null)
            OpcDaProperty = config;
        checkTimer?.Stop();
        checkTimer?.Dispose();
        checkTimer = new Timer(Math.Max(OpcDaProperty.CheckRate, 1) * 60 * 1000);
        checkTimer.Elapsed += CheckTimer_Elapsed;
        checkTimer.Start();
        try
        {
            m_server?.Dispose();
        }
        catch (Exception ex)
        {
            LogEvent?.Invoke(3, this, $"Disconnect warn：{ex.Message}", ex);
        }
        m_server = new OpcServer(OpcDaProperty.OpcName, OpcDaProperty.OpcIP);
    }

    /// <summary>
    /// 按OPC组读取组内变量，结果会在订阅事件中返回
    /// </summary>
    /// <param name="groupName">组名称，值为null时读取全部组</param>
    /// <returns></returns>
    public void ReadItemsWithGroup(string groupName = null)
    {
        if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaMaster));
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
            foreach (var opcGroup in opcGroups.ToArray())
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
        return OpcDaProperty?.ToString();
    }

    /// <summary>
    /// 批量写入值，返回（名称，是否成功，错误描述）
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, Tuple<bool, string>> WriteItem(Dictionary<string, object> writeInfos)
    {
        if (IsExit == 1) throw new ObjectDisposedException(nameof(OpcDaMaster));
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
                        results.AddOrUpdate(item2.Key, Tuple.Create(false, $"The variable does not exist {item2.Key}"));
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
                        results.AddOrUpdate(handleItems[item2.Item1].ItemID, Tuple.Create(false, $"Error code{item2.Item2}"));
                    }
                }
                foreach (var item2 in item1)
                {
                    results.AddOrUpdate(item2.Key, Tuple.Create(true, $"Success"));
                }
            }
            catch (Exception ex)
            {
                var keys = writeInfos.Keys.ToList();
                foreach (var item in keys)
                {
                    results.AddOrUpdate(item, Tuple.Create(false, ex.Message));
                }
                return results;
            }
        }
        return results;
    }

    private void CheckTimer_Elapsed(object? sender, ElapsedEventArgs e)
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
                            LogEvent?.Invoke(1, this, $"Successfully reconnected", null);
                        }
                        catch (Exception ex)
                        {
                            LogEvent?.Invoke(3, this, $"Reconnect failed：{ex.Message}", ex);
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
            LogEvent?.Invoke(3, this, $"Add variable failed：{ex.Message}", ex);
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
                        Init(OpcDaProperty);
                        m_server?.Connect();
                        LogEvent?.Invoke(1, this, $"{m_server.Host} - {m_server.Name} - Connection successful", null);
                        PrivateAddItems();
                    }
                }
            }
            else
            {
                Init(OpcDaProperty);
                m_server?.Connect();
                LogEvent?.Invoke(1, this, $"{m_server.Host} - {m_server.Name} - Connection successful", null);
                PrivateAddItems();
            }
        }
    }

    private void PrivateDisconnect()
    {
        lock (this)
        {
            if (IsConnected)
                LogEvent?.Invoke(1, this, $"{m_server.Host} - {m_server.Name} - Disconnect", null);
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
                LogEvent?.Invoke(3, this, $"Connection dispose failed：{ex.Message}", ex);
            }
        }
    }

    private void Subscription_OnDataChanged(string name, int serverGroupHandle, List<ItemReadResult> values)
    {
        DataChangedHandler?.Invoke(name, serverGroupHandle, values);
    }
}
