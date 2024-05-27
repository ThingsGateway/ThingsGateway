﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Newtonsoft.Json;

using ThingsGateway.Core.Extension;

namespace ThingsGateway.JsonSerialization;

/// <summary>
/// DateTimeOffset 类型序列化
/// </summary>
public class NewtonsoftJsonDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public NewtonsoftJsonDateTimeOffsetJsonConverter()
        : this(default)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="format"></param>
    public NewtonsoftJsonDateTimeOffsetJsonConverter(string format = "yyyy-MM-dd HH:mm:ss")
    {
        Format = format;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="format"></param>
    /// <param name="outputToLocalDateTime"></param>
    public NewtonsoftJsonDateTimeOffsetJsonConverter(string format = "yyyy-MM-dd HH:mm:ss", bool outputToLocalDateTime = false)
        : this(format)
    {
        Localized = outputToLocalDateTime;
    }

    /// <summary>
    /// 时间格式化格式
    /// </summary>
    public string Format { get; private set; }

    /// <summary>
    /// 是否输出为为当地时间
    /// </summary>
    public bool Localized { get; private set; } = false;

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override DateTimeOffset ReadJson(JsonReader reader, Type objectType, DateTimeOffset existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return DateTime.SpecifyKind(Penetrates.ConvertToDateTime(ref reader), Localized ? DateTimeKind.Local : DateTimeKind.Utc);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void WriteJson(JsonWriter writer, DateTimeOffset value, JsonSerializer serializer)
    {
        // 判断是否序列化成当地时间
        var formatDateTime = Localized ? value.ConvertToDateTime() : value;
        serializer.Serialize(writer, formatDateTime.ToString(Format));
    }
}

/// <summary>
/// DateTimeOffset 类型序列化
/// </summary>
public class NewtonsoftJsonNullableDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public NewtonsoftJsonNullableDateTimeOffsetJsonConverter()
        : this(default)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="format"></param>
    public NewtonsoftJsonNullableDateTimeOffsetJsonConverter(string format = "yyyy-MM-dd HH:mm:ss")
    {
        Format = format;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="format"></param>
    /// <param name="outputToLocalDateTime"></param>
    public NewtonsoftJsonNullableDateTimeOffsetJsonConverter(string format = "yyyy-MM-dd HH:mm:ss", bool outputToLocalDateTime = false)
        : this(format)
    {
        Localized = outputToLocalDateTime;
    }

    /// <summary>
    /// 时间格式化格式
    /// </summary>
    public string Format { get; private set; }

    /// <summary>
    /// 是否输出为为当地时间
    /// </summary>
    public bool Localized { get; private set; } = false;

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override DateTimeOffset? ReadJson(JsonReader reader, Type objectType, DateTimeOffset? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return DateTime.SpecifyKind(Penetrates.ConvertToDateTime(ref reader), Localized ? DateTimeKind.Local : DateTimeKind.Utc);
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void WriteJson(JsonWriter writer, DateTimeOffset? value, JsonSerializer serializer)
    {
        if (value == null) writer.WriteNull();
        else
        {
            // 判断是否序列化成当地时间
            var formatDateTime = Localized ? value.ConvertToDateTime() : value;
            serializer.Serialize(writer, formatDateTime!.Value.ToString(Format));
        }
    }
}
