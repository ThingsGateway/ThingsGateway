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

using Mapster;

using ThingsGateway.Gateway.Application.Extensions;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量运行态
/// </summary>
public class VariableRuntime : Variable, IVariable, IDisposable
{
    private bool _isOnline;
    private bool? _isOnlineChanged;
    protected object? _value;

    /// <summary>
    /// 变化时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public DateTime? ChangeTime { get; private set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 所在采集设备
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public DeviceRuntime? DeviceRuntime { get; set; }

    /// <summary>
    /// VariableSource
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IVariableSource VariableSource { get; set; }

    /// <summary>
    /// VariableMethod
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public VariableMethod VariableMethod { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public DateTime? CollectTime { get; private set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 设备名称
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 4)]
    public string? DeviceName => DeviceRuntime?.Name;

    /// <summary>
    /// 是否在线
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public bool IsOnline
    {
        get
        {
            return _isOnline;
        }
        private set
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

    private string _lastErrorMessage;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public string? LastErrorMessage
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
    /// 上次值
    /// </summary>
    [AutoGenerateColumn(Visible = false, Order = 6)]
    public object? LastSetValue { get; internal set; }

    /// <summary>
    /// 原始值
    /// </summary>
    [AutoGenerateColumn(Visible = false, Order = 6)]
    public object? RawValue { get; internal set; }

    /// <summary>
    /// 实时值
    /// </summary>
    [AutoGenerateColumn(Visible = true, Order = 6)]
    public override object? Value { get => _value; set => _value = value; }

    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <param name="isOnline"></param>
    public OperResult SetValue(object? value, DateTime dateTime, bool isOnline = true)
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
                var data = ReadExpressions.GetExpressionsResult(RawValue, DeviceRuntime?.Driver?.LogMessage);
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
                    DeviceRuntime?.Driver?.LogMessage?.LogWarning(_lastErrorMessage);
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

    private void Set(object data, DateTime dateTime)
    {
        DateTime time = dateTime != default ? dateTime : DateTime.Now;
        CollectTime = time;

        bool changed = false;
        if (data == null)
        {
            if (IsOnline)
            {
                changed = (_value != null);
            }
        }
        else
        {
            //判断变化，插件传入的Value可能是基础类型，也有可能是class，比较器无法识别是否变化，这里json处理序列化比较
            //检查IComparable
            if (!data.Equals(_value))
            {
                if (data is IComparable)
                {
                    changed = true;
                }
                else
                {
                    if (_value != null)
                        changed = data.ToJsonNetString() != _value.ToJsonNetString();
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

            LastSetValue = _value;

            if (_isOnline == true)
            {
                _value = data;
            }

            GlobalData.VariableValueChange(this);
        }
        GlobalData.VariableCollectChange(this);
    }


    #region LoadSourceRead

    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="IDevice.LoadSourceRead{T}(IEnumerable{IVariable}, int, string)"/>
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public int Index { get; set; }

    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="IDevice.LoadSourceRead{T}(IEnumerable{IVariable}, int, string)"/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    #endregion LoadSourceRead

    #region 报警

    /// <summary>
    /// 报警值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string? AlarmCode { get; set; }

    /// <summary>
    /// 报警使能
    /// </summary>
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool AlarmEnable
    {
        get
        {
            return LAlarmEnable || LLAlarmEnable || HAlarmEnable || HHAlarmEnable || BoolOpenAlarmEnable || BoolCloseAlarmEnable || CustomAlarmEnable;
        }
    }

    /// <summary>
    /// 报警限值
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string? AlarmLimit { get; set; }

    /// <summary>
    /// 报警文本
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public string? AlarmText { get; set; }

    /// <summary>
    /// 报警时间
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public DateTime? AlarmTime { get; set; }

    /// <summary>
    /// 报警类型
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public AlarmTypeEnum? AlarmType { get; set; }

    /// <summary>
    /// 事件时间
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public DateTime? EventTime { get; set; }
    /// <summary>
    /// 事件时间
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    internal DateTime? PrepareEventTime { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public EventTypeEnum? EventType { get; set; }

    #endregion 报警
    public void Init(DeviceRuntime deviceRuntime)
    {
        DeviceRuntime?.VariableRuntimes?.TryRemove(Name, out _);

        DeviceRuntime = deviceRuntime;

        DeviceRuntime.VariableRuntimes.TryAdd(Name, this);
        GlobalData.IdVariables.TryRemove(Id, out _);
        GlobalData.IdVariables.TryAdd(Id, this);
        GlobalData.Variables.TryRemove(Name, out _);
        GlobalData.Variables.TryAdd(Name, this);
        if (AlarmEnable)
        {
            GlobalData.AlarmEnableVariables.TryRemove(Name, out _);
            GlobalData.AlarmEnableVariables.TryAdd(Name, this);
        }
    }


    public void Dispose()
    {
        DeviceRuntime?.VariableRuntimes?.TryRemove(Name, out _);

        GlobalData.IdVariables.TryRemove(Id, out _);
        GlobalData.Variables.TryRemove(Name, out _);

        GlobalData.AlarmEnableVariables.TryRemove(Name, out _);

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask<OperResult> RpcAsync(string value, string? executive = "brower", CancellationToken cancellationToken = default)
    {
        var data = await GlobalData.RpcService.InvokeDeviceMethodAsync(executive, new Dictionary<string, string>() { { Name, value } }, cancellationToken).ConfigureAwait(false);
        return data.FirstOrDefault().Value;
    }

    public void SetErrorMessage(string? lastErrorMessage)
    {
        _lastErrorMessage = lastErrorMessage;
    }
}

