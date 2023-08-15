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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Opc.Ua;

using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;

namespace ThingsGateway.Foundation.Adapter.OPCUA;
/// <summary>
/// 扩展方法
/// </summary>
public static class JsonUtils
{
    #region Decode

    /// <summary>
    /// 解析获取DataValue
    /// </summary>
    /// <returns></returns>
    public static DataValue Decode(
        IServiceMessageContext Context,
        NodeId dataTypeId,
        BuiltInType builtInType,
        int valueRank,
        JToken json
    )
    {

        var data = DecoderObject(Context, dataTypeId, builtInType, valueRank, json);
        var dataValue = new DataValue(new Variant(data));
        return dataValue;
    }


    /// <summary>
    /// 解析获取object
    /// </summary>
    /// <returns></returns>
    public static object DecoderObject(
        IServiceMessageContext Context,
        NodeId dataTypeId,
        BuiltInType builtInType,
        int valueRank,
        JToken json
    )
    {
        object newData;
        switch (builtInType)
        {
            case BuiltInType.ExtensionObject:
                newData = new
                {
                    Value = new
                    {
                        TypeId = new { Id = dataTypeId.Identifier, Namespace = dataTypeId.NamespaceIndex },
                        Body = json
                    }
                }.ToJson();
                break;
            case BuiltInType.Variant:
                var type = TypeInfo.GetDataTypeId(GetSystemType(json.Type));
                newData = new
                {
                    Value = new
                    {
                        Type = type.Identifier,
                        Body = json
                    }

                }.ToJson();
                break;
            default:
                newData = new
                {
                    Value = json
                }.ToJson();
                break;
        }

        using var decoder = new JsonDecoder(newData.ToString(), Context);
        var data = DecodeRawData(decoder, builtInType, valueRank, "Value");
        return data;
    }



    /// <summary>
    /// DecodeRawData
    /// </summary>
    /// <returns></returns>
    private static object DecodeRawData(JsonDecoder decoder, BuiltInType builtInType, int ValueRank, string fieldName)
    {
        if (builtInType != 0)
        {
            if (ValueRank == ValueRanks.Scalar)
            {
                Type type = TypeInfo.GetSystemType(builtInType, ValueRank);
                switch (builtInType)
                {
                    case BuiltInType.Null: { var variant = decoder.ReadVariant(fieldName); return variant.Value; }
                    case BuiltInType.Boolean: { return decoder.ReadBoolean(fieldName); }
                    case BuiltInType.SByte: { return decoder.ReadSByte(fieldName); }
                    case BuiltInType.Byte: { return decoder.ReadByte(fieldName); }
                    case BuiltInType.Int16: { return decoder.ReadInt16(fieldName); }
                    case BuiltInType.UInt16: { return decoder.ReadUInt16(fieldName); }
                    case BuiltInType.Int32: { return decoder.ReadInt32(fieldName); }
                    case BuiltInType.UInt32: { return decoder.ReadUInt32(fieldName); }
                    case BuiltInType.Int64: { return decoder.ReadInt64(fieldName); }
                    case BuiltInType.UInt64: { return decoder.ReadUInt64(fieldName); }
                    case BuiltInType.Float: { return decoder.ReadFloat(fieldName); }
                    case BuiltInType.Double: { return decoder.ReadDouble(fieldName); }
                    case BuiltInType.String: { return decoder.ReadField(fieldName, out var token) ? token?.ToString() : null; }
                    case BuiltInType.DateTime: { return decoder.ReadDateTime(fieldName); }
                    case BuiltInType.Guid: { return decoder.ReadGuid(fieldName); }
                    case BuiltInType.ByteString: { return decoder.ReadByteString(fieldName); }
                    case BuiltInType.XmlElement: { return decoder.ReadXmlElement(fieldName); }
                    case BuiltInType.NodeId: { return decoder.ReadNodeId(fieldName); }
                    case BuiltInType.ExpandedNodeId: { return decoder.ReadExpandedNodeId(fieldName); }
                    case BuiltInType.StatusCode: { return decoder.ReadStatusCode(fieldName); }
                    case BuiltInType.QualifiedName: { return decoder.ReadQualifiedName(fieldName); }
                    case BuiltInType.LocalizedText: { return decoder.ReadLocalizedText(fieldName); }
                    case BuiltInType.ExtensionObject: { return decoder.ReadExtensionObject(fieldName); }
                    case BuiltInType.DataValue: { return decoder.ReadDataValue(fieldName); }
                    case BuiltInType.Enumeration:
                        {
                            return type.IsEnum ? decoder.ReadEnumerated(fieldName, type) : (object)decoder.ReadInt32(fieldName);
                        }
                    case BuiltInType.DiagnosticInfo: { return decoder.ReadDiagnosticInfo(fieldName); }
                    case BuiltInType.Variant: { return decoder.ReadVariant(fieldName); }
                }
            }
            if (ValueRank >= ValueRanks.OneDimension)
            {
                return decoder.ReadArray(fieldName, ValueRank, builtInType);
            }
        }
        return null;
    }


    #endregion

    #region Encode
    /// <summary>
    /// OPCUAValue解析为Jtoken
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static JToken Encode(
        IServiceMessageContext Context,
        BuiltInType type,
        object value
        )
    {
        //对于Integer，Int64，Number等会转化为string JValue！

        using var encoder = CreateEncoder(Context, null, false);
        Encode(encoder, type, "Value", value, null);
        var textbuffer = encoder.CloseAndReturnText();
        using var stringReader = new StringReader(textbuffer);
        using var jsonReader = new JsonTextReader(stringReader);
        var jToken = JToken.Load(jsonReader);
        return jToken["Value"];
    }

    private static void Encode(JsonEncoder encoder, BuiltInType builtInType, string fieldName, object value, ByteBlock byteBlock)
    {
        bool isArray = (value?.GetType().IsArray ?? false) && (builtInType != BuiltInType.ByteString);
        bool isCollection = (value is IList) && (builtInType != BuiltInType.ByteString);
        if (!isArray && !isCollection)
        {
            switch (builtInType)
            {
                case BuiltInType.Null: { encoder.WriteVariant(fieldName, new Variant(value)); return; }
                //case BuiltInType.Boolean: { encoder.WriteBoolean(fieldName, (bool)value); return; }
                //case BuiltInType.SByte: { encoder.WriteSByte(fieldName, (sbyte)value); return; }
                //case BuiltInType.Byte: { encoder.WriteByte(fieldName, (byte)value); return; }
                //case BuiltInType.Int16: { encoder.WriteInt16(fieldName, (short)value); return; }
                //case BuiltInType.UInt16: { encoder.WriteUInt16(fieldName, (ushort)value); return; }
                //case BuiltInType.Int32: { encoder.WriteInt32(fieldName, (int)value); return; }
                //case BuiltInType.UInt32: { encoder.WriteUInt32(fieldName, (uint)value); return; }
                //case BuiltInType.Int64: { encoder.WriteInt64(fieldName, (long)value); return; }
                //case BuiltInType.UInt64: { encoder.WriteUInt64(fieldName, (ulong)value); return; }
                //case BuiltInType.Float: { encoder.WriteFloat(fieldName, (float)value); return; }
                //case BuiltInType.Double: { encoder.WriteDouble(fieldName, (double)value); return; }

                //case BuiltInType.Integer: { encoder.WriteInt64(fieldName, (long)value); return; }
                //case BuiltInType.Number: { encoder.WriteInt64(fieldName, (long)value); return; }
                //case BuiltInType.UInteger: { encoder.WriteUInt64(fieldName, (ulong)value); return; }
                //case BuiltInType.String: { encoder.WriteString(fieldName, value?.ToString()); return; }
                //case BuiltInType.DateTime: { encoder.WriteDateTime(fieldName, (DateTime)value); return; }
                case BuiltInType.Boolean:
                    {
                        encoder.WriteBoolean(fieldName, Convert.ToBoolean(value));
                        return;
                    }
                case BuiltInType.SByte:
                    {
                        encoder.WriteSByte(fieldName, Convert.ToSByte(value));
                        return;
                    }
                case BuiltInType.Byte:
                    {
                        encoder.WriteByte(fieldName, Convert.ToByte(value));
                        return;
                    }
                case BuiltInType.Int16:
                    {
                        encoder.WriteInt16(fieldName, Convert.ToInt16(value));
                        return;
                    }
                case BuiltInType.UInt16:
                    {
                        encoder.WriteUInt16(fieldName, Convert.ToUInt16(value));
                        return;
                    }
                case BuiltInType.Int32:
                    {
                        encoder.WriteInt32(fieldName, Convert.ToInt32(value));
                        return;
                    }
                case BuiltInType.UInt32:
                    {
                        encoder.WriteUInt32(fieldName, Convert.ToUInt32(value));
                        return;
                    }
                case BuiltInType.Int64:
                    {
                        encoder.WriteInt64(fieldName, Convert.ToInt64(value));
                        return;
                    }
                case BuiltInType.UInt64:
                    {
                        encoder.WriteUInt64(fieldName, Convert.ToUInt64(value));
                        return;
                    }
                case BuiltInType.Float:
                    {
                        encoder.WriteFloat(fieldName, Convert.ToSingle(value));
                        return;
                    }
                case BuiltInType.Double:
                    {
                        encoder.WriteDouble(fieldName, Convert.ToDouble(value));
                        return;
                    }
                case BuiltInType.Integer:
                    {
                        encoder.WriteInt64(fieldName, Convert.ToInt64(value));
                        return;
                    }
                case BuiltInType.Number:
                    {
                        encoder.WriteInt64(fieldName, Convert.ToInt64(value));
                        return;
                    }
                case BuiltInType.UInteger:
                    {
                        encoder.WriteUInt64(fieldName, Convert.ToUInt64(value));
                        return;
                    }
                case BuiltInType.String:
                    {
                        encoder.WriteString(fieldName, Convert.ToString(value));
                        return;
                    }
                case BuiltInType.DateTime:
                    {
                        encoder.WriteDateTime(fieldName, Convert.ToDateTime(value));
                        return;
                    }


                case BuiltInType.Guid: { encoder.WriteGuid(fieldName, (Uuid)value); return; }
                case BuiltInType.ByteString: { encoder.WriteByteString(fieldName, (byte[])value); return; }
                case BuiltInType.XmlElement: { encoder.WriteXmlElement(fieldName, (XmlElement)value); return; }
                case BuiltInType.NodeId: { encoder.WriteNodeId(fieldName, (NodeId)value); return; }
                case BuiltInType.ExpandedNodeId: { encoder.WriteExpandedNodeId(fieldName, (ExpandedNodeId)value); return; }
                case BuiltInType.StatusCode: { encoder.WriteStatusCode(fieldName, (StatusCode)value); return; }
                case BuiltInType.QualifiedName: { encoder.WriteQualifiedName(fieldName, (QualifiedName)value); return; }
                case BuiltInType.LocalizedText: { encoder.WriteLocalizedText(fieldName, (LocalizedText)value); return; }
                case BuiltInType.ExtensionObject: { encoder.WriteExtensionObject(fieldName, (ExtensionObject)value); return; }
                case BuiltInType.DataValue: { encoder.WriteDataValue(fieldName, (DataValue)value); return; }
                case BuiltInType.Enumeration:
                    {
                        if (value?.GetType().IsEnum == true)
                        {
                            encoder.WriteEnumerated(fieldName, (Enum)value);
                        }
                        else
                        {
                            encoder.WriteEnumerated(fieldName, (Enumeration)value);
                        }
                        return;
                    }
                case BuiltInType.Variant: { encoder.WriteVariant(fieldName, new Variant(value)); return; }
                case BuiltInType.DiagnosticInfo: { encoder.WriteDiagnosticInfo(fieldName, (DiagnosticInfo)value); return; }

                    //case BuiltInType.Boolean:
                    //case BuiltInType.SByte:
                    //case BuiltInType.Byte:
                    //case BuiltInType.Int16:
                    //case BuiltInType.UInt16:
                    //case BuiltInType.Int32:
                    //case BuiltInType.UInt32:
                    //case BuiltInType.Int64:
                    //case BuiltInType.UInt64:
                    //case BuiltInType.Float:
                    //case BuiltInType.Double:
                    //case BuiltInType.String:
                    //case BuiltInType.Number:
                    //case BuiltInType.Integer:
                    //case BuiltInType.UInteger:
                    //    { encoder.WriteString(fieldName, value?.ToString()); return; }
            }
        }
        else
        {
            Array c = value as Array;
            encoder.WriteArray(fieldName, c, c.Rank, builtInType);
        }
    }
    /// <summary>
    /// CreateEncoder
    /// </summary>
    /// <returns></returns>
    private static JsonEncoder CreateEncoder(
        IServiceMessageContext context,
        Stream stream,
        bool useReversibleEncoding = false,
        bool topLevelIsArray = false,
        bool includeDefaultValues = true,
        bool includeDefaultNumbers = true
        )
    {

        return new JsonEncoder(context, useReversibleEncoding, topLevelIsArray, stream)
        {
            IncludeDefaultValues = includeDefaultValues,
            IncludeDefaultNumberValues = includeDefaultNumbers
        };
    }
    #endregion

    #region json

    /// <summary>
    /// 维度
    /// </summary>
    /// <param name="jToken"></param>
    /// <returns></returns>
    public static int CalculateActualValueRank(this JToken jToken)
    {
        if (jToken.Type != JTokenType.Array)
            return -1;

        var jArray = jToken.ToArray();
        int numDimensions = 1;

        while (jArray.GetElementsType() == JTokenType.Array)
        {
            jArray = jArray.Children().ToArray();
            numDimensions++;
        }
        return numDimensions;
    }
    private static JTokenType GetElementsType(this JToken[] jTokens)
    {
        if (!jTokens.ElementsHasSameType())
            throw new Exception("The array sent must have the same type of element in each dimension");
        return jTokens.First().Type;
    }
    private static bool ElementsHasSameType(this JToken[] jTokens)
    {
        var checkType = jTokens[0].Type == JTokenType.Integer ? JTokenType.Float : jTokens[0].Type;
        return jTokens
            .Select(x => (x.Type == JTokenType.Integer) ? JTokenType.Float : x.Type)
            .All(t => t == checkType);
    }
    private static Type GetSystemType(JTokenType jsonType)
    {
        return jsonType switch
        {
            JTokenType.None => typeof(string),
            JTokenType.Object => typeof(string),
            JTokenType.Array => typeof(Array),
            JTokenType.Constructor => typeof(string),
            JTokenType.Property => typeof(string),
            JTokenType.Comment => typeof(string),
            JTokenType.Integer => typeof(long),
            JTokenType.Float => typeof(float),
            JTokenType.String => typeof(string),
            JTokenType.Boolean => typeof(bool),
            JTokenType.Null => typeof(string),
            JTokenType.Undefined => typeof(string),
            JTokenType.Date => typeof(DateTime),
            JTokenType.Raw => typeof(string),
            JTokenType.Bytes => typeof(byte[]),
            JTokenType.Guid => typeof(Guid),
            JTokenType.Uri => typeof(Uri),
            JTokenType.TimeSpan => typeof(TimeSpan),
            _ => null,
        };
    }

    #endregion
}