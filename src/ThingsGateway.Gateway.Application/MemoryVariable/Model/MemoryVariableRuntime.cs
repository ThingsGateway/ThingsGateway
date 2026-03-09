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
using Riok.Mapperly.Abstractions;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using ThingsGateway.Foundation.Common.Json.Extension;
using ThingsGateway.Gateway.Application.Extensions;

#if !Management

namespace ThingsGateway.Gateway.Application;
#else

namespace ThingsGateway.Management.Application;
#endif

/// <summary>
/// 变量运行态
/// </summary>
public partial class MemoryVariableRuntime : VariableRuntime
#if !Management
    , IMemoryVariableRpc
#endif
{
    /// <summary>
    /// 设备
    /// </summary>
    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override long DeviceId { get; set; }

    /// <summary>
    /// 写入后再次读取检查值是否一致
    /// </summary>
    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override bool RpcWriteCheck { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时RegisterAddress为方法参数
    /// </summary>
    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override string OtherMethod { get; set; }

    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override DataTypeEnum DataType { get; set; } = DataTypeEnum.Object;

    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override string RegisterAddress { get; set; } = MemoryConst.MemoryName;


    [OrmColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override int? ArrayLength { get; set; }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    [OrmColumn(IsIgnore = true)]
    public override ValidateForm? AlarmPropertysValidateForm { get; set; }

    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    [OrmColumn(IsIgnore = true)]
    public override NonBlockingDictionary<long, ModelValueValidateForm>? VariablePropertyModels { get; set; }



    [OrmColumn(ColumnDescription = "触发方式", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual BusinessUpdateEnum BusinessUpdate { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
#if !Management
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override bool IsMemory => true;

    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override string DeviceName => base.DeviceName;

#else
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override bool IsMemory { get; set; } = true;

    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public override string DeviceName { get; set; }

#endif
#if !Management
    public OperResult<object> MemoryVariableRpc(JsonNode value, CancellationToken cancellationToken = default)
    {

        var data = value.GetObjectFromJsonNode();
        return new(WriteValue(data, DateTime.Now, true));

    }

    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    public override OperResult SetValue(object value, DateTime dateTime, bool isOnline = true, bool setChanged = false)
    {
        IsOnline = isOnline;
        RawValue = value;
        if (IsOnline == false)
        {
            Set(value, dateTime, setChanged);
            return new();
        }
        if (!string.IsNullOrEmpty(ReadExpressions))
        {
            try
            {
                var data = ReadExpressions.GetMemoryExpressionsResult(LogMessage);
                Set(data, dateTime, setChanged);
            }
            catch (Exception ex)
            {
                IsOnline = false;
                Set(null, dateTime, setChanged);
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
            Set(value, dateTime, setChanged);
        }
        return new();
    }


    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <param name="isOnline"></param>
    public OperResult WriteValue(object value, DateTime dateTime, bool isOnline = true)
    {
        IsOnline = isOnline;
        RawValue = value;
        if (IsOnline == false)
        {
            Set(value, dateTime, false);
            return new();
        }
        if (!string.IsNullOrEmpty(WriteExpressions))
        {
            try
            {
                var data = WriteExpressions.GetMemoryExpressionsResult(LogMessage);
                Set(data, dateTime, false);
            }
            catch (Exception ex)
            {
                IsOnline = false;
                Set(null, dateTime, false);
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
            Set(value, dateTime, false);
        }
        return new();
    }
#endif


}
