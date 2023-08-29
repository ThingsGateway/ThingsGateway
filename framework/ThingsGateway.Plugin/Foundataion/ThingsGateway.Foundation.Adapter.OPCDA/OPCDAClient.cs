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

using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;

using ThingsGateway.Foundation.Adapter.OPCDA.Da;
using ThingsGateway.Foundation.Adapter.OPCDA.Rcw;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

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
public class OPCDAClient : DisposableObject
{
    private readonly ILog _logger;

    private readonly EasyLock checkLock = new();

    private Timer checkTimer;

    private bool FirstConnect;

    private int IsExit = 1;

    /// <summary>
    /// 当前保存的需订阅列表
    /// </summary>
    private Dictionary<string, List<OpcItem>> ItemDicts = new();

    private OpcServer m_server;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="logger"></param>
    public OPCDAClient(ILog logger)
    {
#if (NET6_0_OR_GREATER)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotSupportedException("不支持非windows系统");
        }
#endif
        _logger = logger;
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
    public OPCNode OPCNode { get; private set; }
    private List<OpcGroup> Groups => m_server.OpcGroups;
    /// <summary>
    /// 添加节点，需要在连接成功后执行
    /// </summary>
    /// <param name="items">组名称/变量节点，注意每次添加的组名称不能相同</param>
    public OperResult AddItems(Dictionary<string, List<OpcItem>> items)
    {
        try
        {
            if (IsExit == 1) return new("对象已释放");
            StringBuilder stringBuilder = new();
            foreach (var item in items)
            {
                if (IsExit == 1) return new("对象已释放");
                var subscription = m_server.AddGroup(item.Key, true, OPCNode.UpdateRate, OPCNode.DeadBand);
                if (subscription.IsSuccess)
                {
                    subscription.Content.ActiveSubscribe = OPCNode.ActiveSubscribe;
                    subscription.Content.OnDataChanged += Subscription_OnDataChanged;
                    subscription.Content.OnReadCompleted += Subscription_OnDataChanged;

                    var result = subscription.Content.AddOpcItem(item.Value.ToArray());
                    if (!result.IsSuccess)
                    {
                        stringBuilder.AppendLine("添加变量失败" + result.Message);
                    }
                    else
                    {
                        ItemDicts.AddOrUpdate(item.Key, item.Value);
                        _logger?.Debug($"添加成功");
                    }
                }
                else
                {
                    stringBuilder.AppendLine("添加组失败" + subscription.Message);
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
            if (stringBuilder.Length > 0)
            {
                return new(stringBuilder.ToString());
            }
            else
            {
                return OperResult.CreateSuccessResult();
            }
        }
        catch (Exception ex)
        {
            return new(ex);
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
        ItemDicts = items.ToList().ConvertAll(o => new OpcItem(o)
          ).ChunkTrivialBetter(OPCNode.GroupSize).ToDictionary(a => "default" + (i++));
        return ItemDicts;
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void Connect()
    {
        Interlocked.CompareExchange(ref IsExit, 0, 1);
        PrivateConnect();
        FirstConnect = true;
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        Interlocked.CompareExchange(ref IsExit, 1, 0);
        PrivateDisconnect();
    }

    /// <summary>
    /// 浏览节点
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public OperResult<List<BrowseElement>> GetBrowseElements(string itemId = null)
    {
        return this.m_server?.Browse(itemId);
    }

    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <returns></returns>
    public OperResult<ServerStatus> GetServerStatus()
    {
        return this.m_server?.GetServerStatus();
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="node"></param>
    public void Init(OPCNode node)
    {
        if (node != null)
            OPCNode = node;
        checkTimer?.Stop();
        checkTimer?.SafeDispose();
        checkTimer = new Timer(OPCNode.CheckRate * 60 * 1000);
        checkTimer.Elapsed += CheckTimer_Elapsed;
        checkTimer.Start();
        m_server?.SafeDispose();
        m_server = new OpcServer(OPCNode.OPCName, OPCNode.OPCIP);
    }

    /// <summary>
    /// 按OPC组读取组内变量，结果会在订阅事件中返回
    /// </summary>
    /// <param name="groupName">组名称，值为null时读取全部组</param>
    /// <returns></returns>
    public OperResult ReadItemsWithGroup(string groupName = null)
    {
        try
        {
            if (PrivateConnect())
            {
                var groups = groupName != null ? Groups.Where(a => a.Name == groupName) : Groups;
                foreach (var group in groups)
                {
                    if (group.OpcItems.Count > 0)
                    {
                        return group.ReadAsync();
                    }
                    else
                    {
                        return new OperResult("不存在任何变量");
                    }
                }
                return new OperResult("不存在任何变量");
            }
            return new OperResult("未初始化连接");
        }
        catch (Exception ex)
        {
            return new OperResult(ex);

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
            if (!opcGroups.Any())
            {
                _logger.Warning("找不到变量" + item);
                continue;
            }
            foreach (var opcGroup in opcGroups)
            {
                var tag = opcGroup.OpcItems.Where(a => item == a.ItemID);
                var result = opcGroup.RemoveItem(tag.ToArray());
                if (!result.IsSuccess)
                {
                    _logger.Warning($"移除变量{item}-" + result.Message);
                }
                else
                {
                    _logger?.Debug($"移除变量{item}成功");
                }
                if (opcGroup.OpcItems.Count == 0)
                {
                    opcGroup.OnDataChanged -= Subscription_OnDataChanged;
                    ItemDicts.Remove(opcGroup.Name);
                    m_server.RemoveGroup(opcGroup);
                }
                else
                {
                    ItemDicts[opcGroup.Name].RemoveWhere(a => tag.Contains(a));
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
    public Dictionary<string, OperResult> WriteItem(Dictionary<string, JToken> writeInfos)
    {
        Dictionary<string, OperResult> results = new();
        if (PrivateConnect())
        {
            try
            {
                var valueGroup = writeInfos.GroupBy(itemId =>
                  {
                      var group = Groups.FirstOrDefault(it => it.OpcItems.Any(a => a.ItemID == itemId.Key));
                      return group;
                  }).ToList();




                foreach (var item1 in valueGroup)
                {
                    if (item1.Key == null)
                    {
                        foreach (var item2 in item1)
                        {
                            results.Add(item2.Key, new OperResult("不存在该变量" + item2.Key));
                        }
                    }
                    else
                    {
                        List<int> ServerHandles = new();
                        List<object> Values = new();
                        foreach (var item2 in item1)
                        {
                            var opcItem = item1.Key.OpcItems.Where(it => it.ItemID == item2.Key).FirstOrDefault();
                            ServerHandles.Add(opcItem.ServerHandle);
                            var jtoken = item2.Value;
                            var rank = jtoken.CalculateActualValueRank();
                            object rawWriteValue;
                            switch (rank)
                            {
                                case -1:
                                    rawWriteValue = ((JValue)jtoken).Value;
                                    break;
                                default:
                                    var jarray = ((JArray)jtoken);
                                    rawWriteValue = jarray.Select(j => (object)j).ToArray();
                                    break;
                            }

                            Values.Add(rawWriteValue);

                        }

                        var result = item1.Key.Write(Values.ToArray(), ServerHandles.ToArray(), out var PErrors);
                        var data = item1.ToList();
                        for (int i = 0; i < data.Count; i++)
                        {
                            results.Add(data[i].Key, PErrors[i] == 0 ? OperResult.CreateSuccessResult() : new OperResult("错误代码：" + PErrors[i]));
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                var keys = writeInfos.Keys.ToList();
                foreach (var item in keys)
                {
                    results.AddOrUpdate(item, new(ex));
                }
                return results;

            }
        }
        else
        {
            var keys = writeInfos.Keys.ToList();
            foreach (var item in keys)
            {
                results.AddOrUpdate(item, new("未初始化连接"));
            }
            return results;
        }
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        PrivateDisconnect();
        checkLock.SafeDispose();
        base.Dispose(disposing);
    }

    private void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (checkLock.IsWaitting) return;
        checkLock.Wait();
        try
        {
            if (IsExit == 0)
            {
                var status = m_server.GetServerStatus();
                if (status.IsSuccess)
                {
                    _logger?.Trace(OPCNode.ToString() + "OPC状态检查正常!");
                }
                else
                {
                    if (IsExit == 0 && FirstConnect)
                    {
                        if (PrivateConnect())
                        {
                            _logger?.Warning(OPCNode.ToString() + "OPC重新链接成功!");
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
        finally { checkLock.Release(); }

    }

    private void PrivateAddItems()
    {
        var result = AddItems(ItemDicts);
        if (!result.IsSuccess)
        {
            _logger.Warning(result.Message);
        }
    }
    private bool PrivateConnect()
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
                                //disconnect();
                                Init(OPCNode);
                                _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 正在连接");
                                var result = m_server?.Connect();
                                if (result.IsSuccess)
                                {
                                    _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 连接成功");
                                    PrivateAddItems();
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
                    //disconnect();
                    Init(OPCNode);
                    _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 正在连接");
                    var result = m_server?.Connect();
                    if (result.IsSuccess)
                    {
                        _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 连接成功");
                        PrivateAddItems();
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
    private void PrivateDisconnect()
    {
        lock (this)
        {
            if (IsConnected)
                _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 断开连接");
            if (checkTimer != null)
            {
                checkTimer.Enabled = false;
                checkTimer.Stop();
            }

            try
            {
                m_server?.SafeDispose();
                m_server = null;
            }
            catch (Exception ex)
            {
                _logger?.Exception(ToString(), ex);
            }
        }
    }
    private void Subscription_OnDataChanged(List<ItemReadResult> values)
    {
        DataChangedHandler?.Invoke(values);
    }

}