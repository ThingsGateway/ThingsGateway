//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using System.ComponentModel;

using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Gateway.Application.Extensions;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量运行态
/// </summary>
public class VariableRunTime : Variable, IVariable
{
    private bool _isOnline;
    private bool? _isOnlineChanged;
    private object? _value;

    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    public event VariableChangeEventHandler? VariableValueChange;

    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    internal event VariableCollectEventHandler? VariableCollectChange;

    /// <summary>
    /// 变化时间
    /// </summary>
    [Description("变化时间")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public DateTime? ChangeTime { get; private set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 所在采集设备
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [Description("采集设备")]
    public CollectDeviceRunTime? CollectDeviceRunTime { get; set; }

    /// <summary>
    /// VariableSource
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public IVariableSource VariableSource { get; set; }

    /// <summary>
    /// VariableMethod
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public VariableMethod VariableMethod { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [Description("采集时间")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public DateTime? CollectTime { get; private set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 设备名称
    /// </summary>
    [Description("设备名称")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public string? DeviceName { get; set; }

    /// <summary>
    /// 是否在线
    /// </summary>
    [Description("是否在线")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Description("离线原因")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public string? LastErrorMessage
    {
        get
        {
            if (_isOnline == false)
                return VariableSource?.LastErrorMessage ?? VariableMethod?.LastErrorMessage;
            else
                return null;
        }
    }

    /// <summary>
    /// 上次值
    /// </summary>
    [Description("上次值")]
    [DataTable(Order = 3, IsShow = true, Sortable = false, CellClass = " table-text-truncate ")]
    public object? LastSetValue { get; internal set; }

    /// <summary>
    /// 原始值
    /// </summary>
    [Description("原始值")]
    [DataTable(Order = 3, IsShow = true, Sortable = false, CellClass = " table-text-truncate ")]
    public object? RawValue { get; internal set; }

    /// <summary>
    /// 实时值
    /// </summary>
    [Description("实时值")]
    [DataTable(Order = 3, IsShow = true, Sortable = false, CellClass = " table-text-truncate ")]
    public object? Value { get => _value; internal set => _value = value; }

    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <param name="isOnline"></param>
    public OperResult SetValue(object value, DateTime dateTime = default, bool isOnline = true)
    {
        IsOnline = isOnline;
        RawValue = value;
        if (IsOnline == false)
        {
            Set(value);
            return new();
        }
        if (!string.IsNullOrEmpty(ReadExpressions))
        {
            try
            {
                var data = ReadExpressions.GetExpressionsResult(RawValue);
                Set(data);
            }
            catch (Exception ex)
            {
                IsOnline = false;
                Set(null);
                VariableSource.LastErrorMessage = $"{Name} 转换表达式失败：{ex.Message}";
                return new($"{Name} 转换表达式失败：{ex.Message}");
            }
        }
        else
        {
            Set(value);
        }
        return new();

        void Set(object data)
        {
            DateTime time = dateTime != default ? dateTime : DateTimeUtil.Now;
            CollectTime = time;

            if ((data is Array array ? array?.ToJsonString() != _value?.ToJsonString() : (data?.ToString() != _value?.ToString()))
                || _isOnlineChanged == true)
            {
                ChangeTime = time;

                LastSetValue = _value;

                if (_isOnline == true)
                {
                    _value = data;
                }

                VariableValueChange?.Invoke(this);
            }

            VariableCollectChange?.Invoke(this);
        }
    }

    internal void SetErrorMessage(string value)
    {
        if (VariableSource != null)
            VariableSource.LastErrorMessage = value;
    }

    private IRpcService? _rpcService { get; set; }

    /// <inheritdoc/>
    public async Task<OperResult> SetValueToDeviceAsync(string value, string? executive = null, CancellationToken cancellationToken = default)
    {
        _rpcService ??= App.RootServices.GetService<IRpcService>();
        var data = await _rpcService.InvokeDeviceMethodAsync(executive, new Dictionary<string, string>() { { Name, value } });
        return data.Values.FirstOrDefault();
    }

    #region LoadSourceRead

    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="通用.LoadSourceRead{T, T2}(List{T2}, int)"/>
    /// </summary>
    [Description("打包索引")]
    [DataTable(Order = 6, IsShow = true, Sortable = true)]
    public int Index { get; set; }

    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="通用.LoadSourceRead{T, T2}(List{T2}, int)"/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    #endregion LoadSourceRead

    #region 报警

    /// <summary>
    /// 报警值
    /// </summary>
    public string? AlarmCode { get; set; }

    /// <summary>
    /// 报警使能
    /// </summary>
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
    public string? AlarmLimit { get; set; }

    /// <summary>
    /// 报警文本
    /// </summary>
    public string? AlarmText { get; set; }

    /// <summary>
    /// 报警时间
    /// </summary>
    public DateTime? AlarmTime { get; set; }

    /// <summary>
    /// 报警类型
    /// </summary>
    public AlarmTypeEnum? AlarmType { get; set; }

    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime? EventTime { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public EventTypeEnum? EventType { get; set; }

    #endregion 报警
}

/// <summary>
/// 变量采集事件委托
/// </summary>
public delegate void VariableCollectEventHandler(VariableRunTime collectVariableRunTime);

/// <summary>
/// 变量改变事件委托
/// </summary>
public delegate void VariableChangeEventHandler(VariableRunTime collectVariableRunTime);