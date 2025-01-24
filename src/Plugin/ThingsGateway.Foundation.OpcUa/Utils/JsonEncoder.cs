//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Globalization;
using System.Text;
using System.Xml;


namespace Opc.Ua;

public class OPCUAJsonEncoder : IJsonEncoder, IEncoder, IDisposable
{
    private const int kStreamWriterBufferSize = 1024;

    private static readonly char[] m_specialChars = ['"', '\\', '\n', '\r', '\t', '\b', '\f'];
    private static readonly char[] m_substitution = ['"', '\\', 'n', 'r', 't', 'b', 'f'];
    private bool m_commaRequired;
    private IServiceMessageContext m_context;
    private bool m_dontWriteClosing;
    private bool m_inVariantWithEncoding;
    private bool m_leaveOpen;
    private bool m_levelOneSkipped;
    private MemoryStream m_memoryStream;
    private ushort[] m_namespaceMappings;
    private Stack<string> m_namespaces;
    private uint m_nestingLevel;
    private ushort[] m_serverMappings;
    private Stream m_stream;
    private bool m_topLevelIsArray;
    private StreamWriter m_writer;
    public OPCUAJsonEncoder(IServiceMessageContext context, bool useReversibleEncoding)
        : this(context, useReversibleEncoding, topLevelIsArray: false, null, leaveOpen: false, 1024)
    {
    }

    public OPCUAJsonEncoder(IServiceMessageContext context, bool useReversibleEncoding, bool topLevelIsArray = false, Stream stream = null, bool leaveOpen = false, int streamSize = 1024)
    {
        Initialize();
        m_context = context;
        m_stream = stream;
        m_leaveOpen = leaveOpen;
        UseReversibleEncoding = useReversibleEncoding;
        m_topLevelIsArray = topLevelIsArray;
        if (m_stream == null)
        {
            m_memoryStream = new MemoryStream();
            m_writer = new StreamWriter(m_memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), streamSize, leaveOpen: false);
            m_leaveOpen = false;
        }
        else
        {
            m_writer = new StreamWriter(m_stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), streamSize, m_leaveOpen);
        }
        InitializeWriter();
    }

    public OPCUAJsonEncoder(IServiceMessageContext context, bool useReversibleEncoding, StreamWriter writer, bool topLevelIsArray = false)
    {
        Initialize();
        m_context = context;
        m_writer = writer;
        UseReversibleEncoding = useReversibleEncoding;
        m_topLevelIsArray = topLevelIsArray;
        if (m_writer == null)
        {
            m_stream = new MemoryStream();
            m_writer = new StreamWriter(m_stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024);
        }
        InitializeWriter();
    }

    public IServiceMessageContext Context => m_context;
    public EncodingType EncodingType => EncodingType.Json;
    public bool ForceNamespaceUri { get; set; }
    public bool ForceNamespaceUriForIndex1 { get; set; }
    public bool IncludeDefaultNumberValues { get; set; }
    public bool IncludeDefaultValues { get; set; }
    public bool UseReversibleEncoding { get; private set; }

    /// <inheritdoc/>
    public JsonEncodingType EncodingToUse { get; private set; }

    public static ArraySegment<byte> EncodeMessage(IEncodeable message, byte[] buffer, IServiceMessageContext context)
    {

        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        using MemoryStream stream = new MemoryStream(buffer, writable: true);
        using OPCUAJsonEncoder jsonEncoder = new OPCUAJsonEncoder(context, useReversibleEncoding: true, topLevelIsArray: false, stream);
        jsonEncoder.EncodeMessage(message);
        int count = jsonEncoder.Close();
        return new ArraySegment<byte>(buffer, 0, count);
    }

    public static void EncodeSessionLessMessage(IEncodeable message, Stream stream, IServiceMessageContext context, bool leaveOpen)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        OPCUAJsonEncoder jsonEncoder = new OPCUAJsonEncoder(context, useReversibleEncoding: true, topLevelIsArray: false, stream, leaveOpen);
        try
        {
            long position = stream.Position;
            SessionLessServiceMessage sessionLessServiceMessage = new SessionLessServiceMessage();
            sessionLessServiceMessage.NamespaceUris = context.NamespaceUris;
            sessionLessServiceMessage.ServerUris = context.ServerUris;
            sessionLessServiceMessage.Message = message;
            sessionLessServiceMessage.Encode(jsonEncoder);
            if (context.MaxMessageSize > 0 && context.MaxMessageSize < (int)(stream.Position - position))
            {
                throw ServiceResultException.Create(2148007936u, "MaxMessageSize {0} < {1}", context.MaxMessageSize, (int)(stream.Position - position));
            }
            jsonEncoder.Close();
        }
        finally
        {
            if (leaveOpen)
            {
                stream.Position = 0L;
            }
            jsonEncoder.Dispose();
        }
    }

    public int Close()
    {
        if (!m_dontWriteClosing)
        {
            if (m_topLevelIsArray)
            {
                m_writer.Write("]");
            }
            else
            {
                m_writer.Write("}");
            }
        }
        m_writer.Flush();
        int result = (int)m_writer.BaseStream.Position;
        m_writer.Dispose();
        m_writer = null;
        return result;
    }

    public string CloseAndReturnText()
    {
        Close();
        if (m_memoryStream == null)
        {
            MemoryStream memoryStream = m_stream as MemoryStream;
            if (memoryStream != null)
            {
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            throw new NotSupportedException("Cannot get text from external stream. Use Close or MemoryStream instead.");
        }
        return Encoding.UTF8.GetString(m_memoryStream.ToArray());
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void EncodeMessage(IEncodeable message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }
        NodeId value = ExpandedNodeId.ToNodeId(message.TypeId, m_context.NamespaceUris);
        WriteNodeId("TypeId", value);
        WriteEncodeable("Body", message, message.GetType());
    }

    public void PopArray()
    {
        if (m_nestingLevel > 1 || m_topLevelIsArray || (m_nestingLevel == 1 && !m_levelOneSkipped))
        {
            m_writer.Write("]");
            m_commaRequired = true;
        }
        m_nestingLevel--;
    }

    public void PopNamespace()
    {
        m_namespaces.Pop();
    }

    public void PopStructure()
    {
        if (m_nestingLevel > 1 || m_topLevelIsArray || (m_nestingLevel == 1 && !m_levelOneSkipped))
        {
            m_writer.Write("}");
            m_commaRequired = true;
        }
        m_nestingLevel--;
    }

    public void PushArray(string fieldName)
    {
        m_nestingLevel++;
        if (m_commaRequired)
        {
            m_writer.Write(",");
        }
        if (!string.IsNullOrEmpty(fieldName))
        {
            m_writer.Write("\"");
            EscapeString(fieldName);
            m_writer.Write("\":");
        }
        else if (!m_commaRequired && m_nestingLevel == 1 && !m_topLevelIsArray)
        {
            m_levelOneSkipped = true;
            return;
        }
        m_commaRequired = false;
        m_writer.Write("[");
    }

    public void PushNamespace(string namespaceUri)
    {
        m_namespaces.Push(namespaceUri);
    }

    public void PushStructure(string fieldName)
    {
        m_nestingLevel++;
        if (m_commaRequired)
        {
            m_writer.Write(",");
        }
        if (!string.IsNullOrEmpty(fieldName))
        {
            m_writer.Write("\"");
            EscapeString(fieldName);
            m_writer.Write("\":");
        }
        else if (!m_commaRequired && m_nestingLevel == 1 && !m_topLevelIsArray)
        {
            m_levelOneSkipped = true;
            return;
        }
        m_commaRequired = false;
        m_writer.Write("{");
    }

    public void SetMappingTables(NamespaceTable namespaceUris, StringTable serverUris)
    {
        m_namespaceMappings = null;
        if (namespaceUris != null && m_context.NamespaceUris != null)
        {
            m_namespaceMappings = namespaceUris.CreateMapping(m_context.NamespaceUris, updateTable: false);
        }
        m_serverMappings = null;
        if (serverUris != null && m_context.ServerUris != null)
        {
            m_serverMappings = serverUris.CreateMapping(m_context.ServerUris, updateTable: false);
        }
    }

    public void UsingReversibleEncoding<T>(Action<string, T> action, string fieldName, T value, bool useReversibleEncoding)
    {
        bool useReversibleEncoding2 = UseReversibleEncoding;
        try
        {
            UseReversibleEncoding = useReversibleEncoding;
            action(fieldName, value);
        }
        finally
        {
            UseReversibleEncoding = useReversibleEncoding2;
        }
    }

    public void WriteArray(string fieldName, object array, int valueRank, BuiltInType builtInType)
    {
        if (valueRank == 1)
        {
            switch (builtInType)
            {
                case BuiltInType.Boolean:
                    WriteBooleanArray(fieldName, (bool[])array);
                    break;

                case BuiltInType.SByte:
                    WriteSByteArray(fieldName, (sbyte[])array);
                    break;

                case BuiltInType.Byte:
                    WriteByteArray(fieldName, (byte[])array);
                    break;

                case BuiltInType.Int16:
                    WriteInt16Array(fieldName, (short[])array);
                    break;

                case BuiltInType.UInt16:
                    WriteUInt16Array(fieldName, (ushort[])array);
                    break;

                case BuiltInType.Int32:
                    WriteInt32Array(fieldName, (int[])array);
                    break;

                case BuiltInType.UInt32:
                    WriteUInt32Array(fieldName, (uint[])array);
                    break;

                case BuiltInType.Int64:
                    WriteInt64Array(fieldName, (long[])array);
                    break;

                case BuiltInType.UInt64:
                    WriteUInt64Array(fieldName, (ulong[])array);
                    break;

                case BuiltInType.Float:
                    WriteFloatArray(fieldName, (float[])array);
                    break;

                case BuiltInType.Double:
                    WriteDoubleArray(fieldName, (double[])array);
                    break;

                case BuiltInType.String:
                    WriteStringArray(fieldName, (string[])array);
                    break;

                case BuiltInType.DateTime:
                    WriteDateTimeArray(fieldName, (DateTime[])array);
                    break;

                case BuiltInType.Guid:
                    WriteGuidArray(fieldName, (Uuid[])array);
                    break;

                case BuiltInType.ByteString:
                    WriteByteStringArray(fieldName, (byte[][])array);
                    break;

                case BuiltInType.XmlElement:
                    WriteXmlElementArray(fieldName, (XmlElement[])array);
                    break;

                case BuiltInType.NodeId:
                    WriteNodeIdArray(fieldName, (NodeId[])array);
                    break;

                case BuiltInType.ExpandedNodeId:
                    WriteExpandedNodeIdArray(fieldName, (ExpandedNodeId[])array);
                    break;

                case BuiltInType.StatusCode:
                    WriteStatusCodeArray(fieldName, (StatusCode[])array);
                    break;

                case BuiltInType.QualifiedName:
                    WriteQualifiedNameArray(fieldName, (QualifiedName[])array);
                    break;

                case BuiltInType.LocalizedText:
                    WriteLocalizedTextArray(fieldName, (LocalizedText[])array);
                    break;

                case BuiltInType.ExtensionObject:
                    WriteExtensionObjectArray(fieldName, (ExtensionObject[])array);
                    break;

                case BuiltInType.DataValue:
                    WriteDataValueArray(fieldName, (DataValue[])array);
                    break;

                case BuiltInType.DiagnosticInfo:
                    WriteDiagnosticInfoArray(fieldName, (DiagnosticInfo[])array);
                    break;

                case BuiltInType.Enumeration:
                    {
                        Array array6 = array as Array;
                        if (array6 == null)
                        {
                            throw ServiceResultException.Create(2147876864u, "Unexpected non Array type encountered while encoding an array of enumeration.");
                        }
                        WriteEnumeratedArray(fieldName, array6, array6.GetType().GetElementType());
                        break;
                    }
                case BuiltInType.Variant:
                    {
                        Variant[] array3 = array as Variant[];
                        if (array3 != null)
                        {
                            WriteVariantArray(fieldName, array3);
                            break;
                        }
                        IEncodeable[] array4 = array as IEncodeable[];
                        if (array4 != null)
                        {
                            WriteEncodeableArray(fieldName, array4, array.GetType().GetElementType());
                            break;
                        }
                        object[] array5 = array as object[];
                        if (array5 != null)
                        {
                            WriteObjectArray(fieldName, array5);
                            break;
                        }
                        throw ServiceResultException.Create(2147876864u, "Unexpected type encountered while encoding an array of Variants: {0}", array.GetType());
                    }
                default:
                    {
                        IEncodeable[] array2 = array as IEncodeable[];
                        if (array2 != null)
                        {
                            WriteEncodeableArray(fieldName, array2, array.GetType().GetElementType());
                            break;
                        }
                        if (array == null)
                        {
                            WriteSimpleField(fieldName, null, quotes: false);
                            break;
                        }
                        throw ServiceResultException.Create(2147876864u, "Unexpected BuiltInType encountered while encoding an array: {0}", builtInType);
                    }
            }
        }
        else
        {
            if (valueRank <= 1)
            {
                return;
            }
            Matrix matrix = array as Matrix;
            if (matrix == null)
            {
                Array array7 = array as Array;
                if (array7 == null || array7.Rank != valueRank)
                {
                    throw ServiceResultException.Create(2147876864u, "Unexpected array type encountered while encoding array: {0}", array.GetType().Name);
                }
                matrix = new Matrix(array7, builtInType);
            }
            if (matrix != null)
            {
                int index = 0;
                WriteStructureMatrix(fieldName, matrix, 0, ref index, matrix.TypeInfo);
            }
        }
    }

    public void WriteBoolean(string fieldName, bool value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && !value)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else if (value)
        {
            WriteSimpleField(fieldName, "true", quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, "false", quotes: false);
        }
    }

    public void WriteBooleanArray(string fieldName, IList<bool> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteBoolean(null, values[i]);
        }
        PopArray();
    }

    public void WriteByte(string fieldName, byte value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteByteArray(string fieldName, IList<byte> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteByte(null, values[i]);
        }
        PopArray();
    }

    public void WriteByteString(string fieldName, byte[] value)
    {
        if (value == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        if (m_context.MaxByteStringLength > 0 && m_context.MaxByteStringLength < value.Length)
        {
            throw new ServiceResultException(2148007936u);
        }
        WriteSimpleField(fieldName, Convert.ToBase64String(value), quotes: true);
    }

    /// <summary>
    /// Writes a byte string to the stream with a given index and count.
    /// </summary>
    public void WriteByteString(string fieldName, byte[] value, int index, int count)
    {
        if (value == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }

        // check the length.
        if (m_context.MaxByteStringLength > 0 && m_context.MaxByteStringLength < count)
        {
            throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded);
        }

        WriteSimpleField(fieldName, Convert.ToBase64String(value), quotes: true);

    }

    /// <summary>
    /// Writes a byte string to the stream.
    /// </summary>
    public void WriteByteString(string fieldName, ReadOnlySpan<byte> value)
    {
        if (value == ReadOnlySpan<byte>.Empty)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }

        // check the length.
        if (m_context.MaxByteStringLength > 0 && m_context.MaxByteStringLength < value.Length)
        {
            throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded);
        }
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        WriteSimpleField(fieldName, Convert.ToBase64String(value), quotes: true);
#else
        WriteSimpleField(fieldName, Convert.ToBase64String(value.ToArray()), quotes: true);
#endif

    }

    public void WriteByteStringArray(string fieldName, IList<byte[]> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteByteString(null, values[i]);
        }
        PopArray();
    }

    public void WriteDataValue(string fieldName, DataValue value)
    {
        if (value == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushStructure(fieldName);
        if (value != null)
        {
            if (value.WrappedValue.TypeInfo != null && value.WrappedValue.TypeInfo.BuiltInType != 0)
            {
                WriteVariant("Value", value.WrappedValue);
            }
            if (value.StatusCode != 0u)
            {
                WriteStatusCode("StatusCode", value.StatusCode);
            }
            if (value.SourceTimestamp != DateTime.MinValue)
            {
                WriteDateTime("SourceTimestamp", value.SourceTimestamp);
                if (value.SourcePicoseconds != 0)
                {
                    WriteUInt16("SourcePicoseconds", value.SourcePicoseconds);
                }
            }
            if (value.ServerTimestamp != DateTime.MinValue)
            {
                WriteDateTime("ServerTimestamp", value.ServerTimestamp);
                if (value.ServerPicoseconds != 0)
                {
                    WriteUInt16("ServerPicoseconds", value.ServerPicoseconds);
                }
            }
        }
        PopStructure();
    }

    public void WriteDataValueArray(string fieldName, IList<DataValue> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteDataValue(null, values[i]);
        }
        PopArray();
    }

    public void WriteDateTime(string fieldName, DateTime value)
    {
        if (fieldName != null && !IncludeDefaultValues && value == DateTime.MinValue)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else if (value <= DateTime.MinValue)
        {
            WriteSimpleField(fieldName, "0001-01-01T00:00:00Z", quotes: true);
        }
        else if (value >= DateTime.MaxValue)
        {
            WriteSimpleField(fieldName, "9999-12-31T23:59:59Z", quotes: true);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture), quotes: true);
        }
    }

    public void WriteDateTimeArray(string fieldName, IList<DateTime> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] <= DateTime.MinValue)
            {
                WriteSimpleField(null, null, quotes: false);
            }
            else
            {
                WriteDateTime(null, values[i]);
            }
        }
        PopArray();
    }

    public void WriteDiagnosticInfo(string fieldName, DiagnosticInfo value)
    {
        WriteDiagnosticInfo(fieldName, value, 0);
    }

    public void WriteDiagnosticInfoArray(string fieldName, IList<DiagnosticInfo> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteDiagnosticInfo(null, values[i]);
        }
        PopArray();
    }

    public void WriteDouble(string fieldName, double value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value > -4.94065645841247E-324 && value < double.Epsilon)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else if (double.IsNaN(value))
        {
            WriteSimpleField(fieldName, "NaN", quotes: true);
        }
        else if (double.IsPositiveInfinity(value))
        {
            WriteSimpleField(fieldName, "Infinity", quotes: true);
        }
        else if (double.IsNegativeInfinity(value))
        {
            WriteSimpleField(fieldName, "-Infinity", quotes: true);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString("R", CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteDoubleArray(string fieldName, IList<double> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteDouble(null, values[i]);
        }
        PopArray();
    }

    public void WriteEncodeable(string fieldName, IEncodeable value, Type systemType)
    {
        if (value == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        if (m_nestingLevel == 0 && (m_commaRequired || m_topLevelIsArray) && (string.IsNullOrWhiteSpace(fieldName) ^ m_topLevelIsArray))
        {
            throw ServiceResultException.Create(2147876864u, "With Array as top level, encodeables with fieldname will create invalid json");
        }
        if (m_nestingLevel == 0 && !m_commaRequired && string.IsNullOrWhiteSpace(fieldName) && !m_topLevelIsArray)
        {
            m_writer.Flush();
            if (m_writer.BaseStream.Length == 1)
            {
                m_writer.BaseStream.Seek(0L, SeekOrigin.Begin);
            }
            m_dontWriteClosing = true;
        }
        CheckAndIncrementNestingLevel();
        try
        {
            PushStructure(fieldName);
            value?.Encode(this);
            PopStructure();
        }
        finally
        {
            m_nestingLevel--;
        }
    }

    public void WriteEncodeableArray(string fieldName, IList<IEncodeable> values, Type systemType)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else if (string.IsNullOrWhiteSpace(fieldName) && m_nestingLevel == 0 && !m_topLevelIsArray)
        {
            m_writer.Flush();
            if (m_writer.BaseStream.Length == 1)
            {
                m_writer.BaseStream.Seek(0L, SeekOrigin.Begin);
            }
            m_nestingLevel++;
            PushArray(fieldName);
            for (int i = 0; i < values.Count; i++)
            {
                WriteEncodeable(null, values[i], systemType);
            }
            PopArray();
            m_dontWriteClosing = true;
            m_nestingLevel--;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(fieldName) && m_nestingLevel == 0 && m_topLevelIsArray)
            {
                throw ServiceResultException.Create(2147876864u, "With Array as top level, encodeables array with filename will create invalid json");
            }
            PushArray(fieldName);
            if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
            {
                throw new ServiceResultException(2148007936u);
            }
            for (int j = 0; j < values.Count; j++)
            {
                WriteEncodeable(null, values[j], systemType);
            }
            PopArray();
        }
    }

    public void WriteEnumerated(string fieldName, Enum value)
    {
        int num = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        string text = num.ToString();
        if (UseReversibleEncoding)
        {
            WriteSimpleField(fieldName, text, quotes: false);
            return;
        }
        if (value.ToString() == text)
        {
            WriteSimpleField(fieldName, text, quotes: true);
            return;
        }
        WriteSimpleField(fieldName, Utils.Format("{0}_{1}", value.ToString(), num), quotes: true);
    }

    public void WriteEnumerated(string fieldName, int numeric)
    {
        string value = numeric.ToString(CultureInfo.InvariantCulture);
        WriteSimpleField(fieldName, value, !UseReversibleEncoding);
    }

    public void WriteEnumeratedArray(string fieldName, Array values, Type systemType)
    {
        if (values == null || values.Length == 0)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Length)
        {
            throw new ServiceResultException(2148007936u);
        }
        Type elementType = values.GetType().GetElementType();
        if (elementType.IsEnum)
        {
            foreach (Enum value in values)
            {
                WriteEnumerated(null, value);
            }
        }
        else
        {
            if (elementType != typeof(int))
            {
                throw new ServiceResultException(2147876864u, Utils.Format("Type '{0}' is not allowed in an Enumeration.", elementType.FullName));
            }
            foreach (int value2 in values)
            {
                WriteEnumerated(null, value2);
            }
        }
        PopArray();
    }

    public void WriteExpandedNodeId(string fieldName, ExpandedNodeId value)
    {
        var nodeid = (((NodeId)value));
        if (value == null || nodeid == null || (!UseReversibleEncoding && NodeId.IsNull(value)))
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushStructure(fieldName);
        string text = value.NamespaceUri;
        ushort namespaceIndex = value.NamespaceIndex;
        if (ForceNamespaceUri && text == null && namespaceIndex > ((!ForceNamespaceUriForIndex1) ? 1 : 0))
        {
            text = Context.NamespaceUris.GetString(namespaceIndex);
        }
        WriteNodeIdContents(nodeid, text);
        uint num = value.ServerIndex;
        if (num >= 1)
        {
            string @string = m_context.ServerUris.GetString(num);
            if (!string.IsNullOrEmpty(@string))
            {
                WriteSimpleField("ServerUri", @string, quotes: true);
                PopStructure();
                return;
            }
            if (m_serverMappings != null && m_serverMappings.Length > num)
            {
                num = m_serverMappings[num];
            }
            if (num != 0)
            {
                WriteUInt32("ServerUri", num);
            }
        }
        PopStructure();
    }

    public void WriteExpandedNodeIdArray(string fieldName, IList<ExpandedNodeId> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteExpandedNodeId(null, values[i]);
        }
        PopArray();
    }

    public void WriteExtensionObject(string fieldName, ExtensionObject value)
    {
        if (value == null || value.Encoding == ExtensionObjectEncoding.None)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        IEncodeable encodeable = value.Body as IEncodeable;
        if (!UseReversibleEncoding && encodeable != null)
        {
            IStructureTypeInfo structureTypeInfo = value.Body as IStructureTypeInfo;
            if (structureTypeInfo != null && structureTypeInfo.StructureType == StructureType.Union)
            {
                encodeable.Encode(this);
                return;
            }
            PushStructure(fieldName);
            encodeable.Encode(this);
            PopStructure();
            return;
        }
        PushStructure(fieldName);
        ExpandedNodeId expandedNodeId = value.TypeId;
        if (encodeable != null)
        {
            expandedNodeId = value.Encoding switch
            {
                ExtensionObjectEncoding.Binary => encodeable.BinaryEncodingId,
                ExtensionObjectEncoding.Xml => encodeable.XmlEncodingId,
                _ => encodeable.TypeId,
            };
        }
        NodeId value2 = ExpandedNodeId.ToNodeId(expandedNodeId, Context.NamespaceUris);
        if (UseReversibleEncoding)
        {
            WriteNodeId("TypeId", value2);
        }
        else
        {
            WriteExpandedNodeId("TypeId", expandedNodeId);
        }
        if (encodeable != null)
        {
            WriteEncodeable("Body", encodeable, null);
        }
        else if (value.Body != null)
        {
            if (value.Encoding == ExtensionObjectEncoding.Json)
            {
                WriteSimpleField("Body", value.Body as string, quotes: true);
            }
            else
            {
                WriteByte("Encoding", (byte)value.Encoding);
                if (value.Encoding == ExtensionObjectEncoding.Binary)
                {
                    WriteByteString("Body", value.Body as byte[]);
                }
                else if (value.Encoding == ExtensionObjectEncoding.Xml)
                {
                    WriteXmlElement("Body", value.Body as XmlElement);
                }
            }
        }
        PopStructure();
    }

    public void WriteExtensionObjectArray(string fieldName, IList<ExtensionObject> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteExtensionObject(null, values[i]);
        }
        PopArray();
    }

    public void WriteFloat(string fieldName, float value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value > -1.401298E-45f && value < float.Epsilon)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else if (float.IsNaN(value))
        {
            WriteSimpleField(fieldName, "NaN", quotes: true);
        }
        else if (float.IsPositiveInfinity(value))
        {
            WriteSimpleField(fieldName, "Infinity", quotes: true);
        }
        else if (float.IsNegativeInfinity(value))
        {
            WriteSimpleField(fieldName, "-Infinity", quotes: true);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString("R", CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteFloatArray(string fieldName, IList<float> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteFloat(null, values[i]);
        }
        PopArray();
    }

    public void WriteGuid(string fieldName, Uuid value)
    {
        if (fieldName != null && !IncludeDefaultValues && value == Uuid.Empty)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(), quotes: true);
        }
    }

    public void WriteGuid(string fieldName, Guid value)
    {
        if (fieldName != null && !IncludeDefaultValues && value == Guid.Empty)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(), quotes: true);
        }
    }

    public void WriteGuidArray(string fieldName, IList<Uuid> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteGuid(null, values[i]);
        }
        PopArray();
    }

    public void WriteGuidArray(string fieldName, IList<Guid> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteGuid(null, values[i]);
        }
        PopArray();
    }

    public void WriteInt16(string fieldName, short value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteInt16Array(string fieldName, IList<short> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteInt16(null, values[i]);
        }
        PopArray();
    }

    public void WriteInt32(string fieldName, int value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteInt32Array(string fieldName, IList<int> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteInt32(null, values[i]);
        }
        PopArray();
    }

    public void WriteInt64(string fieldName, long value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0L)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: true);
        }
    }

    public void WriteInt64Array(string fieldName, IList<long> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteInt64(null, values[i]);
        }
        PopArray();
    }

    public void WriteLocalizedText(string fieldName, LocalizedText value)
    {
        if (LocalizedText.IsNullOrEmpty(value))
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else if (UseReversibleEncoding)
        {
            PushStructure(fieldName);
            WriteSimpleField("Text", value.Text, quotes: true);
            if (!string.IsNullOrEmpty(value.Locale))
            {
                WriteSimpleField("Locale", value.Locale, quotes: true);
            }
            PopStructure();
        }
        else
        {
            WriteSimpleField(fieldName, value.Text, quotes: true);
        }
    }

    public void WriteLocalizedTextArray(string fieldName, IList<LocalizedText> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteLocalizedText(null, values[i]);
        }
        PopArray();
    }

    public void WriteNodeId(string fieldName, NodeId value)
    {
        if (value == null || (NodeId.IsNull(value) && value.IdType == IdType.Numeric))
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushStructure(fieldName);
        ushort namespaceIndex = value.NamespaceIndex;
        if (ForceNamespaceUri && namespaceIndex > ((!ForceNamespaceUriForIndex1) ? 1 : 0))
        {
            string @string = Context.NamespaceUris.GetString(namespaceIndex);
            WriteNodeIdContents(value, @string);
        }
        else
        {
            WriteNodeIdContents(value);
        }
        PopStructure();
    }

    public void WriteNodeIdArray(string fieldName, IList<NodeId> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteNodeId(null, values[i]);
        }
        PopArray();
    }

    public void WriteObjectArray(string fieldName, IList<object> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (values != null && m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        if (values != null)
        {
            for (int i = 0; i < values.Count; i++)
            {
                WriteVariant("Variant", new Variant(values[i]));
            }
        }
        PopArray();
    }

    public void WriteQualifiedName(string fieldName, QualifiedName value)
    {
        if (QualifiedName.IsNull(value))
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushStructure(fieldName);
        WriteString("Name", value.Name);
        WriteNamespaceIndex("Uri", value.NamespaceIndex);
        PopStructure();
    }

    public void WriteQualifiedNameArray(string fieldName, IList<QualifiedName> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteQualifiedName(null, values[i]);
        }
        PopArray();
    }

    public void WriteSByte(string fieldName, sbyte value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteSByteArray(string fieldName, IList<sbyte> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteSByte(null, values[i]);
        }
        PopArray();
    }

    public void WriteStatusCode(string fieldName, StatusCode value)
    {
        if (fieldName != null && !IncludeDefaultValues && value == 0u)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else if (UseReversibleEncoding)
        {
            WriteUInt32(fieldName, value.Code);
        }
        else if (value != 0u)
        {
            PushStructure(fieldName);
            WriteSimpleField("Code", value.Code.ToString(CultureInfo.InvariantCulture), quotes: false);
            WriteSimpleField("Symbol", StatusCode.LookupSymbolicId(value.CodeBits), quotes: true);
            PopStructure();
        }
    }

    public void WriteStatusCodeArray(string fieldName, IList<StatusCode> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            if (!UseReversibleEncoding && values[i] == 0u)
            {
                WriteSimpleField(null, null, quotes: false);
            }
            else
            {
                WriteStatusCode(null, values[i]);
            }
        }
        PopArray();
    }

    public void WriteString(string fieldName, string value)
    {
        if (fieldName != null && !IncludeDefaultValues && value == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value, quotes: true);
        }
    }

    public void WriteStringArray(string fieldName, IList<string> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteString(null, values[i]);
        }
        PopArray();
    }

    public void WriteUInt16(string fieldName, ushort value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteUInt16Array(string fieldName, IList<ushort> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteUInt16(null, values[i]);
        }
        PopArray();
    }

    public void WriteUInt32(string fieldName, uint value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: false);
        }
    }

    public void WriteUInt32Array(string fieldName, IList<uint> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteUInt32(null, values[i]);
        }
        PopArray();
    }

    public void WriteUInt64(string fieldName, ulong value)
    {
        if (fieldName != null && !IncludeDefaultNumberValues && value == 0L)
        {
            WriteSimpleField(fieldName, null, quotes: false);
        }
        else
        {
            WriteSimpleField(fieldName, value.ToString(CultureInfo.InvariantCulture), quotes: true);
        }
    }

    public void WriteUInt64Array(string fieldName, IList<ulong> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteUInt64(null, values[i]);
        }
        PopArray();
    }

    public void WriteVariant(string fieldName, Variant value)
    {
        if (Variant.Null == value)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        CheckAndIncrementNestingLevel();
        try
        {
            bool flag = value.TypeInfo == null || value.TypeInfo.BuiltInType == BuiltInType.Null || value.Value == null;
            if (UseReversibleEncoding && !flag)
            {
                PushStructure(fieldName);
                byte value2 = (byte)value.TypeInfo.BuiltInType;
                if (value.TypeInfo.BuiltInType == BuiltInType.Enumeration)
                {
                    value2 = 6;
                }
                WriteByte("Type", value2);
                fieldName = "Body";
            }
            if (m_commaRequired)
            {
                m_writer.Write(",");
            }
            if (!string.IsNullOrEmpty(fieldName))
            {
                m_writer.Write("\"");
                EscapeString(fieldName);
                m_writer.Write("\":");
            }
            WriteVariantContents(value.Value, value.TypeInfo);
            if (UseReversibleEncoding && !flag)
            {
                Matrix matrix = value.Value as Matrix;
                if (matrix != null)
                {
                    WriteInt32Array("Dimensions", matrix.Dimensions);
                }
                PopStructure();
            }
        }
        finally
        {
            m_nestingLevel--;
        }
    }

    public void WriteVariantArray(string fieldName, IList<Variant> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] == Variant.Null)
            {
                WriteSimpleField(null, null, quotes: false);
            }
            else
            {
                WriteVariant(null, values[i]);
            }
        }
        PopArray();
    }

    public void WriteVariantContents(object value, TypeInfo typeInfo)
    {
        try
        {
            m_inVariantWithEncoding = UseReversibleEncoding;
            if (value == null)
            {
                return;
            }
            m_commaRequired = false;
            if (typeInfo.ValueRank < 0)
            {
                switch (typeInfo.BuiltInType)
                {
                    case BuiltInType.Boolean:
                        WriteBoolean(null, (bool)value);
                        break;

                    case BuiltInType.SByte:
                        WriteSByte(null, (sbyte)value);
                        break;

                    case BuiltInType.Byte:
                        WriteByte(null, (byte)value);
                        break;

                    case BuiltInType.Int16:
                        WriteInt16(null, (short)value);
                        break;

                    case BuiltInType.UInt16:
                        WriteUInt16(null, (ushort)value);
                        break;

                    case BuiltInType.Int32:
                        WriteInt32(null, (int)value);
                        break;

                    case BuiltInType.UInt32:
                        WriteUInt32(null, (uint)value);
                        break;

                    case BuiltInType.Int64:
                        WriteInt64(null, (long)value);
                        break;

                    case BuiltInType.UInt64:
                        WriteUInt64(null, (ulong)value);
                        break;

                    case BuiltInType.Float:
                        WriteFloat(null, (float)value);
                        break;

                    case BuiltInType.Double:
                        WriteDouble(null, (double)value);
                        break;

                    case BuiltInType.String:
                        WriteString(null, (string)value);
                        break;

                    case BuiltInType.DateTime:
                        WriteDateTime(null, (DateTime)value);
                        break;

                    case BuiltInType.Guid:
                        WriteGuid(null, (Uuid)value);
                        break;

                    case BuiltInType.ByteString:
                        WriteByteString(null, (byte[])value);
                        break;

                    case BuiltInType.XmlElement:
                        WriteXmlElement(null, (XmlElement)value);
                        break;

                    case BuiltInType.NodeId:
                        WriteNodeId(null, (NodeId)value);
                        break;

                    case BuiltInType.ExpandedNodeId:
                        WriteExpandedNodeId(null, (ExpandedNodeId)value);
                        break;

                    case BuiltInType.StatusCode:
                        WriteStatusCode(null, (StatusCode)value);
                        break;

                    case BuiltInType.QualifiedName:
                        WriteQualifiedName(null, (QualifiedName)value);
                        break;

                    case BuiltInType.LocalizedText:
                        WriteLocalizedText(null, (LocalizedText)value);
                        break;

                    case BuiltInType.ExtensionObject:
                        WriteExtensionObject(null, (ExtensionObject)value);
                        break;

                    case BuiltInType.DataValue:
                        WriteDataValue(null, (DataValue)value);
                        break;

                    case BuiltInType.Enumeration:
                        WriteEnumerated(null, (Enum)value);
                        break;

                    case BuiltInType.DiagnosticInfo:
                        WriteDiagnosticInfo(null, (DiagnosticInfo)value);
                        break;

                    case BuiltInType.Variant:
                    case BuiltInType.Number:
                    case BuiltInType.Integer:
                    case BuiltInType.UInteger:
                        break;
                }
            }
            else
            {
                if (typeInfo.ValueRank < 1)
                {
                    return;
                }
                int valueRank = typeInfo.ValueRank;
                if (UseReversibleEncoding)
                {
                    Matrix matrix = value as Matrix;
                    if (matrix != null)
                    {
                        value = matrix.Elements;
                        valueRank = 1;
                    }
                }
                WriteArray(null, value, valueRank, typeInfo.BuiltInType);
                return;
            }
        }
        finally
        {
            m_inVariantWithEncoding = false;
        }
    }

    public void WriteXmlElement(string fieldName, XmlElement value)
    {
        if (value == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        string outerXml = value.OuterXml;
        byte[] bytes = Encoding.UTF8.GetBytes(outerXml);
        WriteSimpleField(fieldName, Convert.ToBase64String(bytes), quotes: true);
    }

    public void WriteXmlElementArray(string fieldName, IList<XmlElement> values)
    {
        if (values == null)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        PushArray(fieldName);
        if (m_context.MaxArrayLength > 0 && m_context.MaxArrayLength < values.Count)
        {
            throw new ServiceResultException(2148007936u);
        }
        for (int i = 0; i < values.Count; i++)
        {
            WriteXmlElement(null, values[i]);
        }
        PopArray();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (m_writer != null)
            {
                Close();
            }
            if (!m_leaveOpen)
            {
                Utils.SilentDispose(m_memoryStream);
                Utils.SilentDispose(m_stream);
                m_memoryStream = null;
                m_stream = null;
            }
        }
    }

    private void CheckAndIncrementNestingLevel()
    {
        if (m_nestingLevel > m_context.MaxEncodingNestingLevels)
        {
            throw ServiceResultException.Create(2148007936u, "Maximum nesting level of {0} was exceeded", m_context.MaxEncodingNestingLevels);
        }
        m_nestingLevel++;
    }

    private void EscapeString(string value)
    {
        foreach (char c in value)
        {
            bool flag = false;
            for (int j = 0; j < m_specialChars.Length; j++)
            {
                if (m_specialChars[j] == c)
                {
                    m_writer.Write('\\');
                    m_writer.Write(m_substitution[j]);
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                if (c < ' ')
                {
                    m_writer.Write("\\u");
                    m_writer.Write("{0:X4}", (int)c);
                }
                else
                {
                    m_writer.Write(c);
                }
            }
        }
    }

    private void Initialize()
    {
        m_stream = null;
        m_writer = null;
        m_namespaces = new Stack<string>();
        m_commaRequired = false;
        m_leaveOpen = false;
        m_nestingLevel = 0u;
        m_levelOneSkipped = false;
        ForceNamespaceUri = false;
        IncludeDefaultValues = false;
        IncludeDefaultNumberValues = true;
    }

    private void InitializeWriter()
    {
        if (m_topLevelIsArray)
        {
            m_writer.Write("[");
        }
        else
        {
            m_writer.Write("{");
        }
    }
    private void WriteDiagnosticInfo(string fieldName, DiagnosticInfo value, int depth)
    {
        if (value == null || value.IsNullDiagnosticInfo)
        {
            WriteSimpleField(fieldName, null, quotes: false);
            return;
        }
        CheckAndIncrementNestingLevel();
        try
        {
            PushStructure(fieldName);
            if (value.SymbolicId >= 0)
            {
                WriteSimpleField("SymbolicId", value.SymbolicId.ToString(CultureInfo.InvariantCulture), quotes: false);
            }
            if (value.NamespaceUri >= 0)
            {
                WriteSimpleField("NamespaceUri", value.NamespaceUri.ToString(CultureInfo.InvariantCulture), quotes: false);
            }
            if (value.Locale >= 0)
            {
                WriteSimpleField("Locale", value.Locale.ToString(CultureInfo.InvariantCulture), quotes: false);
            }
            if (value.LocalizedText >= 0)
            {
                WriteSimpleField("LocalizedText", value.LocalizedText.ToString(CultureInfo.InvariantCulture), quotes: false);
            }
            if (value.AdditionalInfo != null)
            {
                WriteSimpleField("AdditionalInfo", value.AdditionalInfo, quotes: true);
            }
            if (value.InnerStatusCode != 0u)
            {
                WriteStatusCode("InnerStatusCode", value.InnerStatusCode);
            }
            if (value.InnerDiagnosticInfo != null)
            {
                if (depth < DiagnosticInfo.MaxInnerDepth)
                {
                    WriteDiagnosticInfo("InnerDiagnosticInfo", value.InnerDiagnosticInfo, depth + 1);
                }
                else
                {
                    Utils.LogWarning("InnerDiagnosticInfo dropped because nesting exceeds maximum of {0}.", DiagnosticInfo.MaxInnerDepth);
                }
            }
            PopStructure();
        }
        finally
        {
            m_nestingLevel--;
        }
    }

    private void WriteNamespaceIndex(string fieldName, ushort namespaceIndex)
    {
        if (namespaceIndex == 0)
        {
            return;
        }
        if ((!UseReversibleEncoding || ForceNamespaceUri) && namespaceIndex > ((!ForceNamespaceUriForIndex1) ? 1 : 0))
        {
            string @string = m_context.NamespaceUris.GetString(namespaceIndex);
            if (!string.IsNullOrEmpty(@string))
            {
                WriteSimpleField(fieldName, @string, quotes: true);
                return;
            }
        }
        if (m_namespaceMappings != null && m_namespaceMappings.Length > namespaceIndex)
        {
            namespaceIndex = m_namespaceMappings[namespaceIndex];
        }
        if (namespaceIndex != 0)
        {
            WriteUInt16(fieldName, namespaceIndex);
        }
    }

    private void WriteNodeIdContents(NodeId value, string namespaceUri = null)
    {
        if (value.IdType > IdType.Numeric)
        {
            WriteInt32("IdType", (int)value.IdType);
        }
        switch (value.IdType)
        {
            case IdType.Numeric:
                WriteUInt32("Id", (uint)value.Identifier);
                break;

            case IdType.String:
                WriteString("Id", (string)value.Identifier);
                break;

            case IdType.Guid:
                WriteGuid("Id", (Guid)value.Identifier);
                break;

            case IdType.Opaque:
                WriteByteString("Id", (byte[])value.Identifier);
                break;
        }
        if (namespaceUri != null)
        {
            WriteString("Namespace", namespaceUri);
        }
        else
        {
            WriteNamespaceIndex("Namespace", value.NamespaceIndex);
        }
    }

    private void WriteSimpleField(string fieldName, string value, bool quotes)
    {
        if (!string.IsNullOrEmpty(fieldName))
        {
            if (value == null)
            {
                return;
            }
            if (m_commaRequired)
            {
                m_writer.Write(",");
            }
            m_writer.Write("\"");
            EscapeString(fieldName);
            m_writer.Write("\":");
        }
        else if (m_commaRequired)
        {
            m_writer.Write(",");
        }
        if (value != null)
        {
            if (quotes)
            {
                m_writer.Write("\"");
                EscapeString(value);
                m_writer.Write("\"");
            }
            else
            {
                m_writer.Write(value);
            }
        }
        else
        {
            m_writer.Write("null");
        }
        m_commaRequired = true;
    }
    private void WriteStructureMatrix(string fieldName, Matrix matrix, int dim, ref int index, TypeInfo typeInfo)
    {
        var (flag, num) = Matrix.ValidateDimensions(allowZeroDimension: true, matrix.Dimensions, Context.MaxArrayLength);
        if (!flag || num != matrix.Elements.Length)
        {
            throw new ArgumentException("The number of elements in the matrix does not match the dimensions.");
        }
        CheckAndIncrementNestingLevel();
        try
        {
            int num2 = matrix.Dimensions[dim];
            if (dim == matrix.Dimensions.Length - 1)
            {
                Array array = Array.CreateInstance(matrix.Elements.GetType().GetElementType(), num2);
                Array.Copy(matrix.Elements, index, array, 0, num2);
                if (m_commaRequired)
                {
                    m_writer.Write(",");
                }
                WriteVariantContents(array, new TypeInfo(typeInfo.BuiltInType, 1));
                index += num2;
            }
            else
            {
                PushArray(fieldName);
                for (int i = 0; i < num2; i++)
                {
                    WriteStructureMatrix(null, matrix, dim + 1, ref index, typeInfo);
                }
                PopArray();
            }
        }
        finally
        {
            m_nestingLevel--;
        }
    }

    public void UsingAlternateEncoding<T>(Action<string, T> action, string fieldName, T value, JsonEncodingType useEncoding)
    {
        JsonEncodingType currentValue = EncodingToUse;
        try
        {
            EncodingToUse = useEncoding;
            action(fieldName, value);
        }
        finally
        {
            EncodingToUse = currentValue;
        }
    }
}
