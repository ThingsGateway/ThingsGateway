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

namespace ThingsGateway.Foundation;

/// <summary>
/// VariableObject
/// </summary>
public abstract class VariableObject
{
    private IProtocol protocol;
    private int maxPack;

    /// <summary>
    /// VariableObject
    /// </summary>
    public VariableObject(IProtocol protocol, int maxPack)
    {
        this.protocol = protocol;
        this.maxPack = maxPack;
    }

    private List<VariableSourceClass>? deviceVariableSourceReads;

    /// <summary>
    /// <see cref="VariableRuntimeAttribute"/>特性连读，反射赋值到继承类中的属性
    /// </summary>
    public async Task<OperResult> MulReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            //连读
            foreach (var item in deviceVariableSourceReads)
            {
                var result = await protocol.ReadAsync(item.RegisterAddress, item.Length);
                if (result.IsSuccess)
                {
                    item.VariableRunTimes.PraseStructContent(protocol, result.Content, item, exWhenAny: true);
                }
                else
                {
                    item.LastErrorMessage = result.ErrorMessage;
                    item.VariableRunTimes.ForEach(a => a.SetValue(null, isOnline: false));
                    return new(result);
                }
            }

            //结果反射赋值
            var dict1 = VariableObjectHelper.GetPairs(GetType());
            foreach (var pair in dict1)
            {
                pair.Value.Property.SetValue(this, pair.Value.VariableClass.Value);
            }
            return new();
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    private Dictionary<string, VariableRuntimeProperty>? dict;

    private void GetVariableSources()
    {
        if (deviceVariableSourceReads == null)
        {
            dict = VariableObjectHelper.GetPairs(GetType());
            List<VariableClass> variableClasss = new();
            foreach (var pair in dict)
            {
                var dataType = pair.Value.Attribute.DataType == DataTypeEnum.Object ? Type.GetTypeCode(pair.Value.Property.PropertyType).GetDataType() : pair.Value.Attribute.DataType;
                VariableClass variableClass = new VariableClass()
                {
                    DataType = dataType,
                    RegisterAddress = pair.Value.Attribute.RegisterAddress,
                    IntervalTime = 1000,
                };
                pair.Value.VariableClass = variableClass;
                variableClasss.Add(variableClass);
            }
            deviceVariableSourceReads = protocol.LoadSourceRead<VariableSourceClass>(variableClasss, maxPack, 1000);
        }
    }

    /// <summary>
    /// <see cref="VariableRuntimeAttribute"/>特性连读，反射赋值到继承类中的属性
    /// </summary>
    public OperResult MulRead(CancellationToken cancellationToken = default)
    {
        try
        {
            GetVariableSources();
            //连读
            foreach (var item in deviceVariableSourceReads)
            {
                var result = protocol.Read(item.RegisterAddress, item.Length);
                if (result.IsSuccess)
                {
                    item.VariableRunTimes.PraseStructContent(protocol, result.Content, item, exWhenAny: true);
                }
                else
                {
                    item.LastErrorMessage = result.ErrorMessage;
                    item.VariableRunTimes.ForEach(a => a.SetValue(null, isOnline: false));
                    return new(result);
                }
            }

            //结果反射赋值
            var dict1 = VariableObjectHelper.GetPairs(GetType());
            foreach (var pair in dict1)
            {
                pair.Value.Property.SetValue(this, pair.Value.VariableClass.Value);
            }
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
    public OperResult WriteValue(string propertyName, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return new($"propertyName不能为 null 或空。");
            }

            if (!dict.TryGetValue(propertyName, out var variableRuntimeProperty))
            {
                return new($"该属性未被识别，可能没有使用{typeof(VariableRuntimeAttribute)}特性标识");
            }
            var result = protocol.Write(variableRuntimeProperty.VariableClass.RegisterAddress, JToken.FromObject(value), variableRuntimeProperty.VariableClass.DataType, cancellationToken);
            return result;
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
    public async Task<OperResult> WriteValueAsync(string propertyName, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return new($"propertyName不能为 null 或空。");
            }

            if (!dict.TryGetValue(propertyName, out var variableRuntimeProperty))
            {
                return new($"该属性未被识别，可能没有使用{typeof(VariableRuntimeAttribute)}特性标识");
            }
            var result = await protocol.WriteAsync(variableRuntimeProperty.VariableClass.RegisterAddress, JToken.FromObject(value), variableRuntimeProperty.VariableClass.DataType, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }
}