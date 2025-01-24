﻿#if NETCOREAPP
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

using ThingsGateway.NewLife.Reflection;

namespace ThingsGateway.NewLife.Serialization;

/// <summary>面向Type的Json序列化转换器</summary>
/// <remarks>借助字符串序列化Type.FullName</remarks>
public class TypeConverter : JsonConverter<Type>
{
    /// <summary>读取类型</summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
#if NETCOREAPP3_1
    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString()!.GetTypeEx()!;
#else
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString()?.GetTypeEx();
#endif

    /// <summary>写入类型</summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options) => writer.WriteStringValue(value.FullName);
}
#endif