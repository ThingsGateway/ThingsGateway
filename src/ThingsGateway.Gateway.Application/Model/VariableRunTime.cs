//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;


#if !Management
using ThingsGateway.Gateway.Application.Extensions;
#endif

#if !Management
using Riok.Mapperly.Abstractions;

using ThingsGateway.Foundation.Common.Json.Extension;

using System.Text.Json.Nodes;

using Newtonsoft.Json.Linq;

namespace ThingsGateway.Gateway.Application;
#else

namespace ThingsGateway.Management.Application;
#endif

/// <summary>
/// 变量运行态
/// </summary>
public partial class VariableRuntime : Variable
#if !Management
    ,
    IVariable,
    IDisposable
#endif
{

    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public bool ValueInited { get; set; }

    #region 属性

    /// <summary>
    /// 这个参数值由自动打包方法写入/>
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    [IgnoreExcel]
    public int Index { get; set; }

    /// <summary>
    /// 变化时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    [IgnoreExcel]
    public DateTime ChangeTime { get; set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 采集时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    [IgnoreExcel]
    public DateTime CollectTime { get; set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 上次值
    /// </summary>
    [AutoGenerateColumn(Visible = false, Order = 6)]
    [IgnoreExcel]
    public object LastSetValue { get; set; }

    /// <summary>
    /// 原始值
    /// </summary>
    [AutoGenerateColumn(Visible = false, Order = 6)]
    [IgnoreExcel]
    public object RawValue { get => rawValue; set => rawValue = value; }

#if !Management
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public TouchSocket.Core.ILog? LogMessage => DeviceRuntime?.Driver?.LogMessage;

    /// <summary>
    /// 所在采集设备
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public DeviceRuntime? DeviceRuntime { get; private set; }

    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public virtual bool IsMemory => DeviceRuntime?.IsMemory ?? false;

    /// <summary>
    /// VariableSource
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public IVariableSource? VariableSource { get; set; }

    /// <summary>
    /// VariableMethod
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public VariableMethod? VariableMethod { get; set; }

    /// <summary>
    /// 这个参数值由自动打包方法写入/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [MapperIgnore]
    [IgnoreExcel]
    public IThingsGatewayBitConverter? BitConverter { get; set; }

#endif


#if !Management
    private bool _isOnline;


    /// <summary>
    /// 是否在线
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    [IgnoreExcel]
    public bool IsOnline
    {
        get
        {
            return _isOnline;
        }
        protected set
        {
            if (IsOnline != value)
            {
                _isOnlineChanged = true;
            }
            else
            {
                _isOnlineChanged = false;
            }
            _isOnline = value;
        }
    }

    /// <summary>
    /// 设备名称
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 4)]
    [IgnoreExcel]
    public virtual string DeviceName => DeviceRuntime?.Name;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    [IgnoreExcel]
    public string LastErrorMessage
    {
        get
        {
            if (_isOnline == false)
                return _lastErrorMessage ?? VariableSource?.LastErrorMessage ?? VariableMethod?.LastErrorMessage;
            else
                return null;
        }
    }


    /// <summary>
    /// 实时值类型
    /// </summary>
    [AutoGenerateColumn(Visible = true, Order = 6)]
    [IgnoreExcel]
    public string RuntimeType => Value?.GetType()?.ToString();

#else



    /// <summary>
    /// 是否在线
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    [IgnoreExcel]
    public bool IsOnline { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 4)]
    [IgnoreExcel]
    public virtual string DeviceName { get; set; }

    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public virtual bool IsMemory { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    [IgnoreExcel]
    public string LastErrorMessage { get; set; }


    /// <summary>
    /// 实时值类型
    /// </summary>
    [AutoGenerateColumn(Visible = true, Order = 6)]
    [IgnoreExcel]
    public string RuntimeType { get; set; }

#endif


    /// <summary>
    /// 实时值
    /// </summary>
    [AutoGenerateColumn(Visible = true, Order = 6)]
    [IgnoreExcel]
    public object Value { get; set; }

    /// <summary>
    /// 报警使能
    /// </summary>
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    [IgnoreExcel]
    public bool AlarmEnable
    {
        get
        {
            return AlarmPropertys != null && (AlarmPropertys.LAlarmEnable || AlarmPropertys.LLAlarmEnable || AlarmPropertys.HAlarmEnable || AlarmPropertys.HHAlarmEnable || AlarmPropertys.BoolOpenAlarmEnable || AlarmPropertys.BoolCloseAlarmEnable || AlarmPropertys.CustomAlarmEnable);
        }
    }

    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public AlarmRuntimePropertys? AlarmRuntimePropertys { get; set; }

    #endregion

#pragma warning disable CS0414
#pragma warning disable CS0169
    private bool _isOnlineChanged;
    private object rawValue;
#if !Management
#pragma warning disable CS0649
    protected string _lastErrorMessage;


    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <param name="isOnline"></param>
    public virtual OperResult SetValue(object value, DateTime dateTime, bool isOnline = true)
    {
        IsOnline = isOnline;
        RawValue = value;
        if (IsOnline == false)
        {
            Set(value, dateTime);
            return new();
        }
        if (!string.IsNullOrEmpty(ReadExpressions))
        {
            try
            {
                var data = ReadExpressions.GetExpressionsResult(RawValue, LogMessage);
                Set(data, dateTime);
            }
            catch (Exception ex)
            {
                IsOnline = false;
                Set(null, dateTime);
                var oldMessage = _lastErrorMessage;
                if (ex.StackTrace != null)
                {
                    string stachTrace = string.Join(Environment.NewLine, ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Take(3));
                    _lastErrorMessage = $"{Name} Conversion expression failed：{ex.Message}{Environment.NewLine}{stachTrace}";
                }
                else
                {
                    _lastErrorMessage = $"{Name} Conversion expression failed：{ex.Message}{Environment.NewLine}";
                }
                if (oldMessage != _lastErrorMessage)
                {
                    LogMessage?.LogWarning(_lastErrorMessage);
                }
                return new($"{Name} Conversion expression failed", ex);
            }
        }
        else
        {
            Set(value, dateTime);
        }
        return new();
    }

    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="dateTime"></param>
    public void TimeChanged(DateTime dateTime)
    {
        DateTime time = dateTime != default ? dateTime : DateTime.Now;
        if (!IsOnline)
        {
            SetValue(RawValue, time, true);
        }
        else
        {
            CollectTime = time;
            GlobalData.VariableCollectChange(this);
        }
    }

    protected void Set(object data, DateTime dateTime)
    {
        DateTime time = dateTime != default ? dateTime : DateTime.Now;
        CollectTime = time;

        bool changed = false;
        if (data == null)
        {
            if (IsOnline)
            {
                changed = (Value != null);
            }
        }
        else
        {
            if (data is JsonNode jsonNode)
            {
                data = jsonNode.GetObjectFromJsonNode();
            }
            if (data is JToken jToken)
            {
                data = jToken.GetObjectFromJToken();
            }
            //判断变化，插件传入的Value可能是基础类型，也有可能是class，比较器无法识别是否变化，这里json处理序列化比较
            //检查IComparable
            if (!data.Equals(Value))
            {
                if (data is IComparable)
                {
                    changed = true;
                }
                else
                {
                    if (Value != null)
                        changed = data.ToSystemTextJsonString(false) != Value.ToSystemTextJsonString(false);
                    else
                        changed = true;
                }
            }
            else
            {
                changed = false;
            }
        }
        if (changed || _isOnlineChanged == true)
        {
            ChangeTime = time;

            LastSetValue = Value;

            ValueInited = true;

            if (_isOnline == true)
            {
                Value = data;
            }

            GlobalData.VariableValueChange(this);
        }
        GlobalData.VariableCollectChange(this);
    }

    public void Init(DeviceRuntime deviceRuntime)
    {
        GlobalData.AlarmEnableIdVariables.Remove(Id, out _);
        if (!AlarmEnable && GlobalData.RealAlarmIdVariables.TryRemove(Id, out var oldAlarm))
        {
            oldAlarm.EventType = EventTypeEnum.Restart;
            oldAlarm.EventTime = DateTime.Now;
            GlobalData.AlarmChange(this.AdaptAlarmVariable());
        }

        DeviceRuntime?.VariableRuntimes?.Remove(Name, out _);

        DeviceRuntime = deviceRuntime;



        DeviceRuntime?.VariableRuntimes?.TryAdd(Name, this);
        GlobalData.IdVariables.Remove(Id, out _);
        GlobalData.IdVariables.TryAdd(Id, this);
        if (AlarmEnable)
        {
            this.AlarmRuntimePropertys = new();
            GlobalData.AlarmEnableIdVariables.TryAdd(Id, this);
        }
    }

    public void Dispose()
    {
        DeviceRuntime?.VariableRuntimes?.Remove(Name, out _);

        GlobalData.IdVariables.Remove(Id, out _);

        GlobalData.AlarmEnableIdVariables.Remove(Id, out _);
        if (GlobalData.RealAlarmIdVariables.TryRemove(Id, out var oldAlarm))
        {
            oldAlarm.EventType = EventTypeEnum.Restart;
            oldAlarm.EventTime = DateTime.Now;
            GlobalData.AlarmChange(this.AdaptAlarmVariable());
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public virtual ValueTask<IOperResult> RpcAsync(string value, string executive = "brower", CancellationToken cancellationToken = default)
    {
        return RpcAsync(DeviceName, Name, value, executive, cancellationToken);

        static async PooledValueTask<IOperResult> RpcAsync(string deviceName, string name, string value, string executive, CancellationToken cancellationToken)
        {
            var data = await GlobalData.RpcService.InvokeDeviceMethodAsync(executive, new Dictionary<string, Dictionary<string, string>>()
        {
            { deviceName, new Dictionary<string, string>()  {   { name,value} }  }
        }, cancellationToken).ConfigureAwait(false);
            return data.FirstOrDefault().Value.FirstOrDefault().Value;
        }
    }

    public void SetErrorMessage(string lastErrorMessage)
    {
        _lastErrorMessage = lastErrorMessage;
    }


#endif



}
