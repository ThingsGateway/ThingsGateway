//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Foundation.Variable;

namespace ThingsGateway.Foundation;

/// <summary>
/// VariableObject
/// </summary>
public abstract class VariableObject
{
    /// <summary>
    /// MaxPack
    /// </summary>
    protected int MaxPack;

    /// <summary>
    /// DeviceVariableSourceReads
    /// </summary>
    protected List<VariableSourceClass>? DeviceVariableSourceReads;

    /// <summary>
    /// VariableRuntimePropertyDict
    /// </summary>
    public Dictionary<string, VariableRuntimeProperty>? VariableRuntimePropertyDict;

    /// <summary>
    /// VariableObject
    /// </summary>
    public VariableObject(IProtocol protocol, int maxPack)
    {
        this.Protocol = protocol;
        this.MaxPack = maxPack;
    }

    /// <summary>
    /// 协议对象
    /// </summary>
    public IProtocol Protocol { get; set; }

    /// <summary>
    /// <see cref="VariableRuntimeAttribute"/>特性连读，反射赋值到继承类中的属性
    /// </summary>
    public virtual async Task<OperResult> MulReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            //连读
            foreach (var item in DeviceVariableSourceReads)
            {
                var result = await Protocol.ReadAsync(item.RegisterAddress, item.Length, cancellationToken);
                if (result.IsSuccess)
                {
                    item.VariableRunTimes.PraseStructContent(Protocol, result.Content, item, exWhenAny: true);
                }
                else
                {
                    item.LastErrorMessage = result.ErrorMessage;
                    item.VariableRunTimes.ForEach(a => a.SetValue(null, isOnline: false));
                    return new(result);
                }
            }

            SetValue();
            return new();
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <summary>
    /// ReadTime
    /// </summary>
    public DateTime ReadTime { get; set; }

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
                pair.Value.Property.PropertyType.GetTypeValue(data?.ToString(), out var objValue);
                pair.Value.Property.SetValue(this, objValue);
            }
            else
            {
                pair.Value.Property.PropertyType.GetTypeValue(pair.Value.VariableClass.Value?.ToString(), out var objValue);
                pair.Value.Property.SetValue(this, objValue);
            }
        }

        ReadTime = DateTime.Now;
    }

    /// <summary>
    /// <see cref="VariableRuntimeAttribute"/>特性连读，反射赋值到继承类中的属性
    /// </summary>
    public virtual OperResult MulRead(CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            //连读
            foreach (var item in DeviceVariableSourceReads)
            {
                var result = Protocol.Read(item.RegisterAddress, item.Length, cancellationToken);
                if (result.IsSuccess)
                {
                    item.VariableRunTimes.PraseStructContent(Protocol, result.Content, item, exWhenAny: true);
                }
                else
                {
                    item.LastErrorMessage = result.ErrorMessage;
                    item.VariableRunTimes.ForEach(a => a.SetValue(null, isOnline: false));
                    return new(result);
                }
            }
            SetValue();
            return new();
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <summary>
    /// 写入值到设备中
    /// </summary>
    /// <param name="propertyName">属性名称，必须使用<see cref="VariableRuntimeAttribute"/>特性</param>
    /// <param name="value">写入值</param>
    /// <param name="cancellationToken">取消令箭</param>
    public virtual OperResult WriteValue(string propertyName, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            if (string.IsNullOrEmpty(propertyName))
            {
                return new($"propertyName不能为 null 或空。");
            }

            if (!VariableRuntimePropertyDict.TryGetValue(propertyName, out var variableRuntimeProperty))
            {
                return new($"该属性未被识别，可能没有使用{typeof(VariableRuntimeAttribute)}特性标识");
            }

            JToken jToken = GetExpressionsValue(value, variableRuntimeProperty);

            var result = Protocol.Write(variableRuntimeProperty.VariableClass.RegisterAddress, jToken, variableRuntimeProperty.VariableClass.DataType, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

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
    /// 写入值到设备中
    /// </summary>
    /// <param name="propertyName">属性名称，必须使用<see cref="VariableRuntimeAttribute"/>特性</param>
    /// <param name="value">写入值</param>
    /// <param name="cancellationToken">取消令箭</param>
    public virtual async Task<OperResult> WriteValueAsync(string propertyName, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            if (string.IsNullOrEmpty(propertyName))
            {
                return new($"propertyName不能为 null 或空。");
            }

            if (!VariableRuntimePropertyDict.TryGetValue(propertyName, out var variableRuntimeProperty))
            {
                return new($"该属性未被识别，可能没有使用{typeof(VariableRuntimeAttribute)}特性标识");
            }

            JToken jToken = GetExpressionsValue(value, variableRuntimeProperty);

            var result = await Protocol.WriteAsync(variableRuntimeProperty.VariableClass.RegisterAddress, jToken, variableRuntimeProperty.VariableClass.DataType, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return new(ex);
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
}