//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Mapster;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Gateway.Application.Extensions;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量运行态
/// </summary>
public class VariableRunTime : Variable, IVariable
{
    #region 重写

    [AutoGenerateColumn(Visible = false)]
    [NotNull]
    public override long? DeviceId { get; set; }

    [AutoGenerateColumn(Visible = false)]
    public override bool Enable { get; set; }

    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override int? IntervalTime { get; set; }

    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override string? ReadExpressions { get; set; }

    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true, Order = 3)]
    public override string? Unit { get; set; }

    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public override string? WriteExpressions { get; set; }

    #endregion 重写

    protected object? _value;
    private bool _isOnline;
    private bool? _isOnlineChanged;
    private string lastErrorMessage;

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
    [AutoGenerateColumn(Visible = false)]
    public CollectDeviceRunTime? CollectDeviceRunTime { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public DateTime? CollectTime { get; private set; } = DateTime.UnixEpoch.ToLocalTime();

    /// <summary>
    /// 设备名称
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 4)]
    public string? DeviceName { get; set; }

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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 5)]
    public string? LastErrorMessage
    {
        get
        {
            if (_isOnline == false)
                return VariableSource?.LastErrorMessage ?? VariableMethod?.LastErrorMessage ?? lastErrorMessage;
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
    public object? Value { get => _value; internal set => _value = value; }

    /// <summary>
    /// VariableMethod
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Visible = false)]
    public VariableMethod VariableMethod { get; set; }

    /// <summary>
    /// VariableSource
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    [AutoGenerateColumn(Visible = false)]
    public IVariableSource VariableSource { get; set; }

    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <param name="isOnline"></param>
    public OperResult SetValue(object? value, DateTime dateTime = default, bool isOnline = true)
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
                var data = ReadExpressions.GetExpressionsResult(RawValue);
                Set(data, dateTime);
            }
            catch (Exception ex)
            {
                IsOnline = false;
                Set(null, dateTime);
                lastErrorMessage = $"{Name} Conversion expression failed：{ex.Message}";
                return new($"{Name} Conversion expression failed", ex);
            }
        }
        else
        {
            Set(value, dateTime);
        }
        return new();
    }

    /// <inheritdoc/>
    public async ValueTask<OperResult> SetValueToDeviceAsync(string value, string? executive = "BLAZOR", CancellationToken cancellationToken = default)
    {
        var data = await GlobalData.RpcService.InvokeDeviceMethodAsync(executive, new Dictionary<string, string>() { { Name, value } }, cancellationToken).ConfigureAwait(false);
        return data.Values.FirstOrDefault();
    }

    internal void SetErrorMessage(string value)
    {
        if (VariableSource != null)
            VariableSource.LastErrorMessage = value;
    }

    private void Set(object data, DateTime dateTime)
    {
        DateTime time = dateTime != default ? dateTime : DateTime.Now;
        CollectTime = time;

        bool changed = false;
        if (data == null)
        {
            changed = (_value != null);
        }
        else
        {
            //判断变化，插件传入的Value可能是基础类型，也有可能是class，比较器无法识别是否变化，这里json处理序列化比较
            //检查IComparable
            if (data != _value)
            {
                Type type = data?.GetType();
                if (typeof(IComparable).IsAssignableFrom(type))
                {
                    changed = !(data.Equals(_value));
                }
                else
                {
                    changed = data?.ToSystemTextJsonString(new()) != _value?.ToSystemTextJsonString(new());
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
    /// 这个参数值由自动打包方法写入<see cref="通用.LoadSourceRead{T, T2}(List{T2}, int)"/>
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public int Index { get; set; }

    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="通用.LoadSourceRead{T, T2}(List{T2}, int)"/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AutoGenerateColumn(Visible = false)]
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
    /// 事件类型
    /// </summary>
    [AutoGenerateColumn(Visible = false)]
    public EventTypeEnum? EventType { get; set; }

    #endregion 报警
}
