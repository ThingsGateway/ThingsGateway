//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ThingsGateway.Gateway.Application.Extensions;
using ThingsGateway.NewLife.Reflection;

namespace ThingsGateway.Foundation;

/// <summary>
/// VariableObject
/// </summary>
public abstract class VariableObject
{
    /// <summary>
    /// 协议对象
    /// </summary>
    [JsonIgnore]
    public IProtocol Protocol;

    /// <summary>
    /// VariableRuntimePropertyDict
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, VariableRuntimeProperty>? VariableRuntimePropertyDict;

    /// <summary>
    /// DeviceVariableSourceReads
    /// </summary>
    [JsonIgnore]
    protected List<VariableSourceClass>? DeviceVariableSourceReads;

    /// <summary>
    /// MaxPack
    /// </summary>
    protected int MaxPack;

    /// <summary>
    /// VariableObject
    /// </summary>
    public VariableObject(IProtocol protocol, int maxPack)
    {
        Protocol = protocol;
        MaxPack = maxPack;
    }

    /// <summary>
    /// ReadTime
    /// </summary>
    public DateTime ReadTime { get; set; }

    /// <summary>
    /// GetExpressionsValue
    /// </summary>
    /// <param name="value"></param>
    /// <param name="variableRuntimeProperty"></param>
    /// <returns></returns>
    public virtual JToken GetExpressionsValue(object value, VariableRuntimeProperty variableRuntimeProperty)
    {
        var jToken = JToken.FromObject(value);
        if (!string.IsNullOrEmpty(variableRuntimeProperty.Attribute.WriteExpressions))
        {
            object rawdata = jToken is JValue jValue ? jValue.Value : jToken is JArray jArray ? jArray : jToken.ToString();

            object data = variableRuntimeProperty.Attribute.WriteExpressions.GetExpressionsResult(rawdata);
            jToken = JToken.FromObject(data);
        }

        return jToken;
    }

    /// <summary>
    /// GetVariableClass
    /// </summary>
    /// <returns></returns>
    public virtual List<VariableClass> GetVariableClass()
    {
        VariableRuntimePropertyDict ??= VariableObjectHelper.GetVariableRuntimePropertyDict(GetType());
        List<VariableClass> variableClasss = new();
        foreach (var pair in VariableRuntimePropertyDict)
        {
            var dataType = pair.Value.Attribute.DataType == DataTypeEnum.Object ? Type.GetTypeCode(pair.Value.Property.PropertyType.IsArray ? pair.Value.Property.PropertyType.GetElementType() : pair.Value.Property.PropertyType).GetDataType() : pair.Value.Attribute.DataType;
            VariableClass variableClass = new VariableClass()
            {
                DataType = dataType,
                RegisterAddress = pair.Value.Attribute.RegisterAddress,
                IntervalTime = 1000,
            };
            pair.Value.VariableClass = variableClass;
            variableClasss.Add(variableClass);
        }

        return variableClasss;
    }

    /// <summary>
    /// <see cref="VariableRuntimeAttribute"/>特性连读，反射赋值到继承类中的属性
    /// </summary>
    public virtual async ValueTask<OperResult> MultiReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            //连读
            foreach (var item in DeviceVariableSourceReads)
            {
                var result = await Protocol.ReadAsync(item.RegisterAddress, item.Length, cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    var result1 = item.VariableRunTimes.PraseStructContent(Protocol, result.Content, exWhenAny: true);
                    if (!result1.IsSuccess)
                    {
                        item.LastErrorMessage = result1.ErrorMessage;
                        var time = DateTime.Now;
                        item.VariableRunTimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                        return new OperResult(result1);
                    }
                }
                else
                {
                    item.LastErrorMessage = result.ErrorMessage;
                    var time = DateTime.Now;
                    item.VariableRunTimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                    return new OperResult(result);
                }
            }

            SetValue();
            return OperResult.Success;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <summary>
    /// 结果反射赋值
    /// </summary>
    public virtual void SetValue()
    {
        //结果反射赋值
        foreach (var pair in VariableRuntimePropertyDict)
        {
            if (!string.IsNullOrEmpty(pair.Value.Attribute.ReadExpressions))
            {
                var data = pair.Value.Attribute.ReadExpressions.GetExpressionsResult(pair.Value.VariableClass.Value);
                pair.Value.Property.SetValue(this, data.ChangeType(pair.Value.Property.PropertyType));
            }
            else
            {
                pair.Value.Property.SetValue(this, pair.Value.VariableClass.Value.ChangeType(pair.Value.Property.PropertyType));
            }
        }

        ReadTime = DateTime.Now;
    }

    /// <summary>
    /// 写入值到设备中
    /// </summary>
    /// <param name="propertyName">属性名称，必须使用<see cref="VariableRuntimeAttribute"/>特性</param>
    /// <param name="value">写入值</param>
    /// <param name="cancellationToken">取消令箭</param>
    public virtual async ValueTask<OperResult> WriteValueAsync(string propertyName, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            if (string.IsNullOrEmpty(propertyName))
            {
                return new OperResult($"PropertyName cannot be null or empty.");
            }

            if (!VariableRuntimePropertyDict.TryGetValue(propertyName, out var variableRuntimeProperty))
            {
                return new OperResult($"This attribute is not recognized and may not have been identified using the {typeof(VariableRuntimeAttribute)} attribute");
            }

            JToken jToken = GetExpressionsValue(value, variableRuntimeProperty);

            var result = await Protocol.WriteAsync(variableRuntimeProperty.VariableClass.RegisterAddress, jToken, variableRuntimeProperty.VariableClass.DataType, cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <summary>
    /// GetVariableSources
    /// </summary>
    protected virtual void GetVariableSources()
    {
        if (DeviceVariableSourceReads == null)
        {
            List<VariableClass> variableClasss = GetVariableClass();
            DeviceVariableSourceReads = Protocol.LoadSourceRead<VariableSourceClass>(variableClasss, MaxPack, 1000);
        }
    }
}
