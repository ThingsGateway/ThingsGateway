//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

using System.Text;

using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

/// <summary>
/// String类型数据转换器
/// </summary>
public class ThingsGatewayStringConverter : StringSerializerConverter
{
    public static ThingsGatewayStringConverter Default = new ThingsGatewayStringConverter();

    /// <summary>
    /// 构造函数
    /// </summary>
    public ThingsGatewayStringConverter(params ISerializerFormatter<string, object>[] converters) : base(converters)
    {
        this.Add(new StringToClassConverter<object>());
        this.Add(new JsonStringToClassSerializerFormatter<object>());
    }
}

/// <summary>
/// String值转换为基础类型。
/// </summary>
public class StringToClassConverter<TState> : ISerializerFormatter<string, TState>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int Order { get; set; } = -100;

    /// <inheritdoc/>
    public bool TryDeserialize(TState state, in string source, Type targetType, out object target)
    {
        return targetType.GetTypeValue(source, out target!);
    }

    /// <inheritdoc/>
    public bool TrySerialize(TState state, in object target, out string source)
    {
        if (target != null)
        {
            var targetType = target.GetType();
            return targetType.GetTypeStringValue(target, out source!);
        }
        else
        {
            source = null;
            return true;
        }
    }
}

/// <summary>
/// Json字符串转到对应类
/// </summary>
public class JsonStringToClassSerializerFormatter<TState> : ISerializerFormatter<string, TState>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int Order { get; set; } = -99;

    /// <summary>
    /// JsonSettings
    /// </summary>
    public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

    /// <inheritdoc/>
    public bool TryDeserialize(TState state, in string source, Type targetType, out object target)
    {
        try
        {
            target = JsonConvert.DeserializeObject(source, targetType, this.JsonSettings)!;
            return true;
        }
        catch
        {
            target = default;
            return false;
        }
    }

    /// <inheritdoc/>
    public bool TrySerialize(TState state, in object target, out string source)
    {
        try
        {
            source = JsonConvert.SerializeObject(target, JsonSettings);
            return true;
        }
        catch (Exception)
        {
            source = null;
            return false;
        }
    }
}

public class StringToEncodingConverter : ISerializerFormatter<string, object>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="targetType"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryDeserialize(object state, in string source, Type targetType, out object target)
    {
        try
        {
            target = Encoding.Default;
            if (targetType == typeof(Encoding))
            {
                target = Encoding.GetEncoding(source);
                return true;
            }
        }
        catch
        {
            target = default;
            return false;
        }
        return false;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public bool TrySerialize(object state, in object target, out string source)
    {
        try
        {
            if (target?.GetType() == typeof(Encoding))
            {
                source = (target as Encoding).WebName;
                return true;
            }
            source = target.ToJsonString();
            return true;
        }
        catch (Exception)
        {
            source = null;
            return false;
        }
    }
}
