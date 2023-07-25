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

using ThingsGateway.Core;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 变量运行状态表示
/// </summary>
public class DeviceVariableRunTime : DeviceVariable
{
    /// <summary>
    /// 设备名称
    /// </summary>
    [Description("设备名称")]
    [OrderTable(Order = 2)]
    public string DeviceName { get; set; }
    /// <summary>
    /// 运行态数据类型
    /// </summary>
    [Description("数据类型")]
    public Type DataType
    {
        get
        {
            if (Value != null && DataTypeEnum == DataTypeEnum.Object)
            {
                return Value?.GetType();
            }
            else
            {
                return DataTypeEnum.GetSystemType();
            }
        }
    }

    /// <summary>
    /// 所在采集设备
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [Description("采集设备")]
    public CollectDeviceRunTime CollectDeviceRunTime { get; set; }

    /// <summary>
    /// 原始值
    /// </summary>
    [Description("原始值")]
    [OrderTable(Order = 3)]
    public object RawValue { get; private set; }
    private object _value;
    /// <summary>
    /// 实时值
    /// </summary>
    [Description("实时值")]
    [OrderTable(Order = 3)]
    public object Value { get => _value; private set => _value = value; }
    /// <summary>
    /// 最近一次值
    /// </summary>
    [Description("最近一次值")]
    [OrderTable(Order = 3)]
    public object LastSetValue { get; private set; }

    /// <summary>
    /// 设置变量值与时间，设置为null时只更改质量戳状态
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    public OperResult SetValue(object value, DateTime dateTime = default)
    {
        try
        {

            if (value != null)
            {
                IsOnline = true;
            }
            else
            {
                IsOnline = false;
                RawValue = value;
                Set(value);
                return OperResult.CreateSuccessResult();
            }
            RawValue = value;
            if (!ReadExpressions.IsNullOrEmpty())
            {
                object data = null;
                try
                {
                    data = ReadExpressions.GetExpressionsResult(RawValue);
                    Set(data);
                }
                catch (Exception ex)
                {
                    Set(null);
                    return new(Name + " 转换表达式失败：" + ex.Message);
                }
            }
            else
            {
                Set(value);
            }
            return OperResult.CreateSuccessResult();

        }

        catch (Exception ex)
        {
            return new(ex);
        }


        void Set(object data)
        {
            DateTime time;
            if (dateTime == default)
            {
                time = DateTime.UtcNow;
            }
            else
            {
                time = dateTime;
            }
            CollectTime = time;
            if (data?.ToString() != _value?.ToString() && LastSetValue?.ToString() != data?.ToString() || isOnlineChanged)
            {
                ChangeTime = time;
                if (IsOnline)
                {
                    _value = data;
                }
                LastSetValue = data;
                VariableValueChange?.Invoke(this);
            }
            VariableCollectChange?.Invoke(this);
        }
    }

    /// <summary>
    /// 变化时间
    /// </summary>
    [Description("变化时间")]
    [OrderTable(Order = 4)]
    public DateTime ChangeTime { get; private set; }
    /// <summary>
    /// 采集时间
    /// </summary>
    [Description("采集时间")]
    [OrderTable(Order = 4)]
    public DateTime CollectTime { get; private set; }


    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    internal event VariableChangeEventHandler VariableCollectChange;

    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    public event VariableChangeEventHandler VariableValueChange;

    private bool isOnline;
    private bool isOnlineChanged;
    /// <summary>
    /// 是否在线
    /// </summary>
    [Description("是否在线")]
    [OrderTable(Order = 2)]
    public bool IsOnline
    {
        get
        {
            return isOnline;
        }
        private set
        {
            if (IsOnline != value)
            {
                isOnlineChanged = true;
            }
            else
            {
                isOnlineChanged = false;
            }
            isOnline = value;

        }
    }


    #region LoadSourceRead
    /// <summary>
    /// <see cref="CollectBase.ReadAsync(string, int, System.Threading.CancellationToken)"/>返回字节组中的索引位置
    /// 这个参数值由自动分包方法写入<see cref="CollectBase.LoadSourceRead(List{DeviceVariableRunTime})"/>
    /// </summary>
    [Description("分包索引")]
    [OrderTable(Order = 6)]
    public int Index { get; set; }
    /// <summary>
    /// 这个参数值由自动分包方法写入<see cref="CollectBase.LoadSourceRead(List{DeviceVariableRunTime})"/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    #endregion

    #region 报警
    /// <summary>
    /// 报警使能
    /// </summary>
    public bool AlarmEnable
    {
        get
        {
            return LAlarmEnable || LLAlarmEnable || HAlarmEnable || HHAlarmEnable || BoolOpenAlarmEnable || BoolCloseAlarmEnable;
        }
    }
    /// <summary>
    /// 报警时间
    /// </summary>
    public DateTime AlarmTime { get; set; }
    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; set; }
    /// <summary>
    /// 报警类型
    /// </summary>
    public AlarmEnum AlarmTypeEnum { get; set; } = AlarmEnum.None;
    /// <summary>
    /// 事件类型
    /// </summary>
    public EventEnum EventTypeEnum { get; set; } = EventEnum.None;
    /// <summary>
    /// 报警值
    /// </summary>
    public string AlarmCode { get; set; }
    /// <summary>
    /// 报警限值
    /// </summary>
    public string AlarmLimit { get; set; }
    /// <summary>
    /// 报警文本
    /// </summary>
    public string AlarmText { get; set; }
    #endregion
}

/// <summary>
/// 变量变化委托
/// </summary>
public delegate void VariableChangeEventHandler(DeviceVariableRunTime collectVariableRunTime);


