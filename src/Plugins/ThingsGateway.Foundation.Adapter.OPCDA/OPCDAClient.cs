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
using System.Text;
using System.Threading;
using System.Timers;

using ThingsGateway.Foundation.Adapter.OPCDA.Da;
using ThingsGateway.Foundation.Adapter.OPCDA.Rcw;
using ThingsGateway.Foundation.Extension.Enumerator;
using ThingsGateway.Foundation.Extension.Generic;

using Timer = System.Timers.Timer;

//部分非托管交互代码来自https://gitee.com/Zer0Day/opc-client与OPC基金会opcnet库，更改部分逻辑

namespace ThingsGateway.Foundation.Adapter.OPCDA;

public delegate void DataChangedEventHandler(List<ItemReadResult> values);

public class OPCDAClient : DisposableObject
{
    /// <summary>
    /// 当前配置
    /// </summary>
    public OPCNode OPCNode;
    private ILog _logger;
    private EasyLock checkLock = new();
    private Timer checkTimer;
    private bool FirstConnect;
    private int IsExit = 1;
    /// <summary>
    /// 当前保存的需订阅列表
    /// </summary>
    private Dictionary<string, List<OpcItem>> ItemDicts = new();

    private OpcServer m_server;
    //定义组对象（订阅者）
    public OPCDAClient(ILog logger)
    {
        _logger = logger;
    }

    public event DataChangedEventHandler DataChangedHandler;
    public bool IsConnected => m_server?.IsConnected == true;
    private List<OpcGroup> Groups => m_server.OpcGroups;
    /// <summary>
    /// 添加节点，需要在连接成功后执行
    /// </summary>
    /// <param name="tags">组名称/变量节点，注意每次添加的组名称不能相同</param>
    public OperResult AddTags(Dictionary<string, List<OpcItem>> tags)
    {
        try
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (IsExit == 1) return new("对象已释放");
            foreach (var item in tags)
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
                        _logger?.Debug($"添加变量{item.Value.Select(a => a.ItemID).ToList().ToJson()}成功");
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
    /// 连接服务器
    /// </summary>
    public void Connect()
    {
        Interlocked.CompareExchange(ref IsExit, 0, 1);
        connect();
        FirstConnect = true;
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        Interlocked.CompareExchange(ref IsExit, 1, 0);
        disconnect();
    }

    /// <summary>
    /// 浏览节点
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public OperResult<List<BrowseElement>> GetBrowse(string itemId = null)
    {
        return this.m_server?.Browse(itemId);
    }

    /// <summary>
    /// 获取服务状态
    /// </summary>
    /// <returns></returns>
    public OperResult<ServerStatus> GetStatus()
    {
        return this.m_server?.GetServerStatus();
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="node"></param>
    public void Init(OPCNode node = null)
    {
        if (node != null)
            OPCNode = node;
        checkTimer?.Stop();
        checkTimer?.SafeDispose();
        checkTimer = new Timer(OPCNode.CheckRate);
        checkTimer.Elapsed += checkTimer_Elapsed;
        checkTimer.Start();
        m_server?.SafeDispose();
        m_server = new OpcServer(OPCNode.OPCName, OPCNode.OPCIP);
    }

    /// <summary>
    /// 手动读取变量，结果会在订阅事件中返回
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public OperResult ReadGroup(string groupName = null)
    {
        try
        {
            if (connect())
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
    /// <param name="tags"></param>
    public void RemoveTags(List<string> tags)
    {
        foreach (var item in tags)
        {
            if (IsExit == 1) return;
            var opcGroup = Groups.FirstOrDefault(it => it.OpcItems.Any(a => a.ItemID == item));
            if (opcGroup == null)
            {
                _logger.Warning("找不到变量" + item);
                continue;
            }
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
                ItemDicts.Remove(opcGroup.Name);
                m_server.RemoveGroup(opcGroup);
            }
            else
            {
                ItemDicts[opcGroup.Name].RemoveWhere(a => tag.Contains(a));
            }

        }
    }

    /// <summary>
    /// 设置节点并保存
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public Dictionary<string, List<OpcItem>> SetTags(List<string> tags)
    {
        int i = 0;
        ItemDicts = tags.ToList().ConvertAll(o => new OpcItem(o)
          ).ChunkTrivialBetter(OPCNode.GroupSize).ToDictionary(a => "default" + (i++));
        return ItemDicts;
    }
    public override string ToString()
    {
        return OPCNode.ToString();
    }

    /// <summary>
    /// 写入值
    /// </summary>
    /// <param name="valueName">写入</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public OperResult Write(string valueName, string value)
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
                int[] PErrors = new int[1];
                var jtoken = JToken.Parse(value);
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

                object[] Value = new object[1] { rawWriteValue };
                var result = group.Write(Value, serverHandle, out PErrors);
                //itemvalue.Value = Convert.ChangeType(value, item.RunTimeDataType);
                return result;
            }
            catch (Exception ex)
            {
                return new OperResult(ex.Message);
            }
        }
        return new OperResult();
    }

    protected override void Dispose(bool disposing)
    {
        disconnect();
        base.Dispose(disposing);
    }

    private void AddTags()
    {
        var result = AddTags(ItemDicts);
        if (!result.IsSuccess)
        {
            _logger.Warning(result.Message);
        }
    }

    private void checkTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (checkLock.IsWaitting) return;
        checkLock.Lock();
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
                        if (connect())
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
                                //disconnect();
                                Init(OPCNode);
                                _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 正在连接");
                                var result = m_server?.Connect();
                                if (result.IsSuccess)
                                {
                                    _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 连接成功");
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
                    //disconnect();
                    Init(OPCNode);
                    _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 正在连接");
                    var result = m_server?.Connect();
                    if (result.IsSuccess)
                    {
                        _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 连接成功");
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



    private void disconnect()
    {
        if (IsConnected)
            _logger?.Trace($"{m_server.Host + " - " + m_server.Name} - 断开连接");
        checkTimer.Enabled = false;
        checkTimer.Stop();
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
    private void Subscription_OnDataChanged(ItemReadResult[] values)
    {
        DataChangedHandler?.Invoke(values.ToList());

    }

}