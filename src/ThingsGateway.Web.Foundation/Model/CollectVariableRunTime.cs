using Furion.Logging.Extensions;

using ThingsGateway.Core;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

public class CollectVariableRunTime : CollectDeviceVariable
{
    /// <summary>
    /// 设备名称
    /// </summary>
    [Description("设备名称")]
    [OrderData(Order = 2)]
    public string DeviceName { get; set; }
    [Description("数据类型")]
    public Type DataType
    {
        get
        {
            if (Value != null && DataTypeEnum == DataTypeEnum.Object)
            {
                return Value.GetType();
            }
            else
            {
                return DataTypeEnum.GetNetType();
            }
        }
    }


    [Description("原始值")]
    [OrderData(Order = 3)]
    public object RawValue { get; set; }
    private object _value;
    [Description("实时值")]
    [OrderData(Order = 3)]
    public object Value { get => _value; private set => _value = value; }
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public object LastSetValue;

    public void SetValue(object value)
    {
        if (value != null)
        {
            Quality = 192;
        }
        else
        {
            Quality = 0;
            RawValue = value;
            Set(value);
            return;
        }
        RawValue = value;
        if (!ReadExpressions.IsNullOrEmpty() && value != null)
        {
            object data = null;
            try
            {
                data = ReadExpressions.GetExpressionsResult(RawValue);
            }
            catch (Exception ex)
            {
                (Name + " 转换表达式失败：" + ex.Message).LogError();
            }
            Set(data);
        }
        else
        {
            Set(value);
        }

        void Set(object data)
        {
            CollectTime = DateTime.Now;
            if (data?.ToString() != _value?.ToString() && LastSetValue != data)
            {
                ChangeTime = DateTime.Now;
                if (Quality == 192)
                    _value = data;
                VariableValueChange?.Invoke(this);
            }

            VariableCollectChange?.Invoke(this);
            LastSetValue = data;
        }
    }


    [Description("变化时间")]
    [OrderData(Order = 4)]
    public DateTime ChangeTime { get; set; }

    [Description("采集时间")]
    [OrderData(Order = 4)]
    public DateTime CollectTime { get; set; }


    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    internal VariableCahngeEventHandler VariableCollectChange { get; set; }

    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public VariableCahngeEventHandler VariableValueChange { get; set; }

    [Description("质量戳")]
    [OrderData(Order = 5)]
    public int Quality { get; private set; }


    #region LoadSourceRead
    /// <summary>
    /// <see cref="DriverBase.ReadAsync(string, ushort)"/>返回字节组中的索引位置
    /// 这个参数值由自动分包方法写入<see cref="DriverBase.LoadSourceRead(List{CollectVariableRunTime})"/>
    /// </summary>
    [Description("分包索引")]
    [OrderData(Order = 6)]
    public int Index { get; set; }
    /// <summary>
    /// 变量在属于字符串类型时的字符串长度
    /// 这个参数值由自动分包方法写入<see cref="DriverBase.LoadSourceRead(List{CollectVariableRunTime})"/>
    /// </summary>
    [Description("字符串长度")]
    [OrderData(Order = 6)]
    public int StringLength { get; set; }


    /// <summary>
    /// 变量在属于字符串类型时的BCD类型
    /// <br></br>
    /// 这个参数值由自动分包方法写入<see cref="DriverBase.LoadSourceRead(List{CollectVariableRunTime})"/>
    /// </summary>
    public BcdFormat StringBcdFormat { get; set; }

    /// <summary>
    /// 这个参数值由自动分包方法写入<see cref="DriverBase.LoadSourceRead(List{CollectVariableRunTime})"/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    #endregion



    #region 报警

    public bool AlarmEnable
    {
        get
        {
            return LAlarmEnable || LLAlarmEnable || HAlarmEnable || HHAlarmEnable || BoolOpenAlarmEnable || BoolCloseAlarmEnable;
        }
    }

    public DateTime AlarmTime { get; set; }
    public DateTime EventTime { get; set; }

    public AlarmEnum AlarmTypeEnum { get; set; }

    public EventEnum EventTypeEnum { get; set; }

    public string AlarmCode { get; set; }

    [SugarColumn(ColumnName = "AlamLimit", ColumnDescription = "报警限值", IsNullable = false)]
    public string AlarmLimit { get; set; }

    public string AlarmText { get; set; }
    #endregion
}

/// <summary>
/// 变量触发变化
/// </summary>
public delegate void VariableCahngeEventHandler(CollectVariableRunTime collectVariableRunTime);


