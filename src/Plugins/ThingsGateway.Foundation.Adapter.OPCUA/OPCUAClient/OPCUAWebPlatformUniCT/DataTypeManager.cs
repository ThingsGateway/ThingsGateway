#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Opc.Ua;
using Opc.Ua.Client;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TypeInfo = Opc.Ua.TypeInfo;

namespace ThingsGateway.Foundation.Adapter.OPCUA;

public class DataTypeManager
{
    private readonly ISession _session;

    public DataTypeManager(ISession session)
    {
        _session = session;
    }

    #region Read UA Value


    public JToken GetJToken(VariableNode variableNode, DataValue dataValue)
    {
        var value = new Variant(dataValue.Value);
        BuiltInType type = TypeInfo.GetBuiltInType(variableNode.DataType, _session.SystemContext.TypeTable);

        switch (type)
        {
            case BuiltInType.Boolean:
                return SerializeBoolean(variableNode, value);
            case BuiltInType.SByte:
                return SerializeSByte(variableNode, value);
            case BuiltInType.Byte:
                return SerializeByte(variableNode, value);
            case BuiltInType.Int16:
            case BuiltInType.UInt16:
            case BuiltInType.Int32:
            case BuiltInType.UInt32:
            case BuiltInType.Int64:
            case BuiltInType.UInt64:
                return SerializeInteger(variableNode, value);
            case BuiltInType.Float:
                return SerializeFloat(variableNode, value);
            case BuiltInType.Double:
                return SerializeDouble(variableNode, value);
            case BuiltInType.String:
            case BuiltInType.DateTime:
            case BuiltInType.Guid:
                return SerializeString(variableNode, value);
            case BuiltInType.DiagnosticInfo:
                return SerializeDiagnosticInfo(variableNode, value);
            case BuiltInType.LocalizedText:
                return SerializeLocalizedText(variableNode, value);
            case BuiltInType.NodeId:
                return SerializeNodeId(variableNode, value);
            case BuiltInType.ExpandedNodeId:
                return SerializeExpandedNodeId(variableNode, value);
            case BuiltInType.StatusCode:
                return SerializeStatusCode(variableNode, value);
            case BuiltInType.QualifiedName:
                return SerializeQualifiedName(variableNode, value);
            case BuiltInType.ByteString:
                return SerializeByteString(variableNode, value);
            case BuiltInType.Enumeration:
                return SerializeEnumeration(variableNode, value);
            case BuiltInType.ExtensionObject:
                return SerializeExtensionObject(variableNode, value);
        }

        return JValue.CreateNull();
    }

    private static dynamic GetEnumValue(Variant value, int enstrreturn, LocalizedText[] enumString,
        EnumValueType[] enumValues)
    {
        dynamic valueOut;
        int index = (int)value.Value;
        if (enstrreturn < 0)
        {
            var jsonResultEnumerationCustom = new
            {
                EnumIndex = index,
                EnumValue = ""
            };
            valueOut = JObject.FromObject(jsonResultEnumerationCustom);
        }
        else if (enstrreturn == 1)
        {
            var jsonResultEnumerationCustom = new
            {
                EnumIndex = index,
                EnumValue = enumString[index].Text
            };
            valueOut = JObject.FromObject(jsonResultEnumerationCustom);
        }
        else
        {

            var jsonResultEnumerationCustom = new
            {
                EnumIndex = index,
                EnumValue = enumValues.Single(s => s.Value == index).DisplayName.Text
            };
            valueOut = JObject.FromObject(jsonResultEnumerationCustom);
        }

        return valueOut;
    }

    private static Array IterativeCopy<TInput, TOutput>(Array source, int[] dimensions, Func<TInput, TOutput> mutate)
    {
        var array = Array.CreateInstance(typeof(TOutput), dimensions);
        var flatSource = Utils.FlattenArray(source);
        var indexes = new int[dimensions.Length];

        for (var ii = 0; ii < flatSource.Length; ii++)
        {
            var mutated = mutate((TInput)flatSource.GetValue(ii));
            array.SetValue(mutated, indexes);

            for (var jj = indexes.Length - 1; jj >= 0; jj--)
            {
                indexes[jj]++;

                if (indexes[jj] < dimensions[jj])
                {
                    break;
                }

                indexes[jj] = 0;
            }
        }

        return array;
    }


    private int GetEnumStrings(NodeId dataTypeNodeId, out LocalizedText[] enumStrings, out EnumValueType[] enumValues)
    {
        _session.Browse(
            null,
            null,
            dataTypeNodeId,
            0u,
            BrowseDirection.Forward,
            ReferenceTypeIds.HasProperty,  //HasProperty reference
            true,
            (uint)NodeClass.Variable, //looking for Variable
            out _,
            out var refDescriptionCollection);

        //Because it is enum it will reference Property (variabile) EnumStrings.

        bool enumstr = refDescriptionCollection.Exists(referenceDescription => referenceDescription.BrowseName.Name.Equals("EnumStrings"));
        bool enumval = refDescriptionCollection.Exists(referenceDescription => referenceDescription.BrowseName.Name.Equals("EnumValues"));

        if (enumstr)
        {
            ReferenceDescription enumStringsReferenceDescription = refDescriptionCollection.First(referenceDescription => referenceDescription.BrowseName.Name.Equals("EnumStrings"));
            NodeId enumStringNodeId = ExpandedNodeId.ToNodeId(enumStringsReferenceDescription.NodeId, _session.MessageContext.NamespaceUris);
            enumStrings = (LocalizedText[])ReadService(enumStringNodeId, Attributes.Value)[0].Value;
            enumValues = null;

            return 1;
        }

        if (enumval)
        {
            ReferenceDescription enumValuesReferenceDescription = refDescriptionCollection.First(referenceDescription => referenceDescription.BrowseName.Name.Equals("EnumValues"));
            NodeId enumStringNodeId = (NodeId)enumValuesReferenceDescription.NodeId;
            ExtensionObject[] enVal = (ExtensionObject[])ReadService(enumStringNodeId, Attributes.Value)[0].Value;
            enumValues = new EnumValueType[enVal.Length];
            for (int ind = 0; ind < enVal.Length; ind++)
                enumValues[ind] = (EnumValueType)enVal[ind].Body;

            enumStrings = null;

            return 2;
        }

        enumValues = null;
        enumStrings = null;
        return -1;

    }

    private dynamic GetInnerDiagnosticInfo(DiagnosticInfo diagnosticInfo)
    {
        if (diagnosticInfo == null)
            return new
            {

            };

        string code = new PlatformStatusCode(diagnosticInfo.InnerStatusCode).code;
        return new
        {
            diagnosticInfo.SymbolicId,
            diagnosticInfo.NamespaceUri,
            diagnosticInfo.Locale,
            diagnosticInfo.LocalizedText,
            diagnosticInfo.AdditionalInfo,
            diagnosticInfo.InnerStatusCode,
            InnerDiagnosticInfo = GetInnerDiagnosticInfo(diagnosticInfo)
        };
    }

    private DataValueCollection ReadService(NodeId nodeId, uint attributeId)
    {
        ReadValueIdCollection nodeToRead = new ReadValueIdCollection(1);

        ReadValueId vId = new ReadValueId()
        {
            NodeId = nodeId,
            AttributeId = attributeId
        };

        nodeToRead.Add(vId);

        DataValueCollection dataValueCollection;
        DiagnosticInfoCollection diagnCollection;

        var responseRead = _session.Read(null,
                     0,
                     TimestampsToReturn.Both,
                     nodeToRead,
                     out dataValueCollection,
                     out diagnCollection
                     );

        return dataValueCollection;
    }

    private JToken SerializeBoolean(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jBoolVal = new JValue(value.Value);
            return jBoolVal;
        }
        else if (variableNode.ValueRank == 1)
        {

            var arr = (Array)value.Value;
            var jArray = new JArray(arr);
            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArray = JArray.Parse(arrStr);

            return jArray;

        }
    }

    private JToken SerializeByte(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jIntVal = new JValue(value.Value);
            return jIntVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var bytes = (Byte[])value.Value;
            int[] byteRepresentations = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                byteRepresentations[i] = Convert.ToInt32(bytes[i]);
            }
            var jArray = new JArray(byteRepresentations);

            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);


            return jArr;
        }
    }

    private JToken SerializeByteString(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jStringVal = new JValue(Convert.ToBase64String((byte[])value.Value));
            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Array)value.Value;
            var newArr = new List<string>();
            foreach (var byteString in arr)
            {
                newArr.Add(Convert.ToBase64String((byte[])byteString));
            }
            var jArray = new JArray(newArr);


            return jArray;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private JToken SerializeDiagnosticInfo(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var diagnosticInfo = (DiagnosticInfo)value.Value;
            var code = new PlatformStatusCode(diagnosticInfo.InnerStatusCode);
            var diagnosticInfoValue = new
            {
                diagnosticInfo.SymbolicId,
                diagnosticInfo.NamespaceUri,
                diagnosticInfo.Locale,
                diagnosticInfo.LocalizedText,
                diagnosticInfo.AdditionalInfo,
                InnerStatusCode = new
                {
                    code.code,
                    code.structureChanged
                },
                InnerDiagnosticInfo = GetInnerDiagnosticInfo(diagnosticInfo)
            };
            var jStringVal = JObject.Parse(JsonConvert.SerializeObject(diagnosticInfoValue));

            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var jArray = new JArray();

            var diagnosticInfos = (DiagnosticInfo[])value.Value;
            foreach (var diagnInfo in diagnosticInfos)
            {
                var code = new PlatformStatusCode(diagnInfo.InnerStatusCode);
                jArray.Add(JObject.Parse(JsonConvert.SerializeObject(new
                {
                    diagnInfo.SymbolicId,
                    diagnInfo.NamespaceUri,
                    diagnInfo.Locale,
                    diagnInfo.LocalizedText,
                    diagnInfo.AdditionalInfo,
                    InnerStatusCode = new
                    {
                        code.code,
                        code.structureChanged
                    },
                    InnerDiagnosticInfo = GetInnerDiagnosticInfo(diagnInfo)
                })));
            }


            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var diagnInfos = (DiagnosticInfo[])matrix.Elements;
            var diagnInfoRepresentation = new dynamic[matrix.Elements.Length];
            for (var i = 0; i < diagnInfos.Length; i++)
            {
                var code = new PlatformStatusCode(diagnInfos[i].InnerStatusCode);
                diagnInfoRepresentation[i] = new
                {
                    diagnInfos[i].SymbolicId,
                    diagnInfos[i].NamespaceUri,
                    diagnInfos[i].Locale,
                    diagnInfos[i].LocalizedText,
                    diagnInfos[i].AdditionalInfo,
                    InnerStatusCode = new
                    {
                        code.code,
                        code.structureChanged
                    },
                    InnerDiagnosticInfo = GetInnerDiagnosticInfo(diagnInfos[i])
                };
            }

            var arr = (new Matrix(diagnInfoRepresentation, BuiltInType.DiagnosticInfo, matrix.Dimensions)).ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);


            return jArr;
        }
    }

    private JToken SerializeDouble(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jDoubleVal = new JValue(value.Value);
            return jDoubleVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Array)value.Value;
            var jArray = new JArray(arr);

            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);

            return jArr;
        }
    }

    private JToken SerializeEnumeration(VariableNode variableNode, Variant value)
    {
        int enstrreturn = GetEnumStrings(variableNode.DataType, out var enumString, out var enumValues);


        if (enstrreturn == 1)
        {
            List<JToken> enumIndexList = new List<JToken>();
            for (int i = 0; i < enumString.Length; i++)
            {
                enumIndexList.Add(new JValue(i));
            }
            List<JToken> enumValueList = enumString.Select(val => new JValue(val.Text)).Cast<JToken>().ToList();
        }

        if (enstrreturn == 2)
        {
            List<JToken> enumIndexList = new List<JToken>();
            List<JToken> enumValueList = new List<JToken>();
            foreach (var val in enumValues)
            {
                enumIndexList.Add(new JValue(val.Value));
                enumValueList.Add(new JValue(val.DisplayName.Text));
            }
        }

        if (variableNode.ValueRank == -1)
        {
            var valueOut = GetEnumValue(value, enstrreturn, enumString, enumValues);

            return valueOut;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Int32[])value.Value;
            var values = arr.Select(s => GetEnumValue(s, enstrreturn, enumString, enumValues));
            var jArray = new JArray(values);

            return jArray;
        }
        else
        {
            throw new NotImplementedException("Read Matrix of Emuneration not implemented");
        }
    }

    private JToken SerializeExpandedNodeId(VariableNode variableNode, Variant value)
    {

        if (variableNode.ValueRank == -1)
        {
            ExpandedNodeId expandedNodeId = (ExpandedNodeId)value.Value;
            string NodeId = "";
            if (expandedNodeId.IdType == IdType.Opaque)
                NodeId = expandedNodeId.NamespaceIndex + "-" + Convert.ToBase64String((byte[])expandedNodeId.Identifier);
            else
                NodeId = expandedNodeId.NamespaceIndex + "-" + expandedNodeId.Identifier;
            string NamespaceUri = "";
            if (expandedNodeId.NamespaceUri != null)
                NamespaceUri = expandedNodeId.NamespaceUri;
            var expNodeId = new
            {
                NodeId,
                NamespaceUri,
                expandedNodeId.ServerIndex
            };
            var jStringVal = JObject.Parse(JsonConvert.SerializeObject(expNodeId));

            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            JArray jArray = new JArray();
            var expandedNodeIds = (ExpandedNodeId[])value.Value;

            string NodeId;
            string NamespaceUri = "";
            for (int i = 0; i < expandedNodeIds.Length; i++)
            {
                if (expandedNodeIds[i].IdType == IdType.Opaque)
                    NodeId = expandedNodeIds[i].NamespaceIndex + "-" + Convert.ToBase64String((byte[])expandedNodeIds[i].Identifier, 0, ((byte[])expandedNodeIds[i].Identifier).Length);
                else
                    NodeId = expandedNodeIds[i].NamespaceIndex + "-" + expandedNodeIds[i].Identifier;
                NamespaceUri = "";
                if (expandedNodeIds[i].NamespaceUri != null)
                    NamespaceUri = expandedNodeIds[i].NamespaceUri;
                var expNodeId = new
                {
                    NodeId,
                    NamespaceUri,
                    expandedNodeIds[i].ServerIndex
                };
                jArray.Add(JObject.Parse(JsonConvert.SerializeObject(expNodeId)));
            }
            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var expandedNodeIds = (ExpandedNodeId[])matrix.Elements;
            var expNodeIdRepresentation = new dynamic[matrix.Elements.Length];
            string NodeId;
            string NamespaceUri = "";
            for (int i = 0; i < expandedNodeIds.Length; i++)
            {
                if (expandedNodeIds[i].IdType == IdType.Opaque)
                    NodeId = expandedNodeIds[i].NamespaceIndex + "-" + Convert.ToBase64String((byte[])expandedNodeIds[i].Identifier, 0, ((byte[])expandedNodeIds[i].Identifier).Length);
                else
                    NodeId = expandedNodeIds[i].NamespaceIndex + "-" + expandedNodeIds[i].Identifier.ToString();

                NamespaceUri = "";
                if (expandedNodeIds[i].NamespaceUri != null)
                    NamespaceUri = expandedNodeIds[i].NamespaceUri;
                var expNodeId = new
                {
                    NodeId,
                    NamespaceUri,
                    expandedNodeIds[i].ServerIndex
                };
                expNodeIdRepresentation[i] = JObject.Parse(JsonConvert.SerializeObject(expNodeId));
            }

            var arr = (new Matrix(expNodeIdRepresentation, BuiltInType.ExpandedNodeId, matrix.Dimensions)).ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);
            return jArr;
        }
    }

    private JToken SerializeExtensionObject(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            //Check if it is not a Type of the standard information model
            if (variableNode.DataType.NamespaceIndex != 0)
            {
                var analyzer = new DataTypeAnalyzer(_session);
                var encodingNodeId = analyzer.GetDataTypeEncodingNodeId(variableNode.DataType);
                var descriptionNodeId = analyzer.GetDataTypeDescriptionNodeId(encodingNodeId);
                string dictionary = analyzer.GetDictionary(descriptionNodeId);

                //Retrieve a key that will be used by the Parser. As explained in the specification Part 3, 
                //the value of DataTypeDescription variable contains the description identifier in the 
                //DataTypeDictionary value which describe the data structure.
                string descriptionId = ReadService(descriptionNodeId, Attributes.Value)[0].Value.ToString();

                //Start parsing
                var parser = new ParserXPath(dictionary);

                return parser.Parse(descriptionId, (ExtensionObject)value.Value, _session.MessageContext);
            }

            var structStandard = ((ExtensionObject)value.Value).Body;
            var jValue = JObject.FromObject(structStandard);
            return jValue;
        }
        else if (variableNode.ValueRank == 1)
        {
            if (variableNode.DataType.NamespaceIndex != 0)
            {
                var analyzer = new DataTypeAnalyzer(_session);
                var encodingNodeId = analyzer.GetDataTypeEncodingNodeId(variableNode.DataType);
                var descriptionNodeId = analyzer.GetDataTypeDescriptionNodeId(encodingNodeId);
                //TODO: 字典可以实现的缓存以提高性能
                string dictionary = analyzer.GetDictionary(descriptionNodeId);

                string descriptionId = ReadService(descriptionNodeId, Attributes.Value)[0].Value.ToString();

                var parser = new ParserXPath(dictionary);
                var jArray = new JArray();
                var arrayValue = (Array)value.Value;

                JToken uaValue;

                foreach (var x in arrayValue)
                {
                    uaValue = parser.Parse(descriptionId, (ExtensionObject)x, _session.MessageContext);
                    jArray.Add(uaValue);
                }
                return jArray;
            }
            else
            {
                var structArray = ((ExtensionObject[])value.Value).Select(s => s.Body);
                var jArray = JArray.FromObject(structArray);
                return jArray;
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private JToken SerializeFloat(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jFloatVal = new JValue(value.Value);

            return jFloatVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Array)value.Value;
            var jArray = new JArray(arr);


            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);

            return jArr;
        }
    }

    private JToken SerializeInteger(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jIntVal = new JValue(value.Value);
            return jIntVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Array)value.Value;
            var jArray = new JArray(arr);
            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);

            return jArr;
        }
    }
    private JToken SerializeLocalizedText(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            LocalizedText locText = (LocalizedText)value.Value;
            var loctext = new
            {
                locText.Locale,
                locText.Text
            };
            var jStringVal = JObject.Parse(JsonConvert.SerializeObject(loctext));

            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var jArray = new JArray();

            var locText = (LocalizedText[])value.Value;
            for (int i = 0; i < locText.Length; i++)
            {
                jArray.Add(JObject.Parse(JsonConvert.SerializeObject(new
                {
                    locText[i].Locale,
                    locText[i].Text
                })));
            }


            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var locTexts = (LocalizedText[])matrix.Elements;
            var locTextsRepresentation = new dynamic[matrix.Elements.Length];
            for (int i = 0; i < locTexts.Length; i++)
            {
                locTextsRepresentation[i] = new
                {
                    locTexts[i].Locale,
                    locTexts[i].Text
                };
            }

            var arr = (new Matrix(locTextsRepresentation, BuiltInType.LocalizedText, matrix.Dimensions)).ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);


            return jArr;
        }
    }

    private JToken SerializeNodeId(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            NodeId nodeId = (NodeId)value.Value;
            string nodeIdRepresentation = "";
            if (nodeId.IdType == IdType.Opaque)
                nodeIdRepresentation = nodeId.NamespaceIndex + "-" + Convert.ToBase64String((byte[])nodeId.Identifier);
            else
                nodeIdRepresentation = nodeId.NamespaceIndex + "-" + nodeId.Identifier;
            var jStringVal = new JValue(nodeIdRepresentation);
            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var nodeIds = (NodeId[])value.Value;
            string[] nodeIdRepresentations = new string[nodeIds.Length];
            for (int i = 0; i < nodeIds.Length; i++)
            {
                if (nodeIds[i].IdType == IdType.Opaque)
                    nodeIdRepresentations[i] = nodeIds[i].NamespaceIndex + "-" + Convert.ToBase64String((byte[])nodeIds[i].Identifier, 0, ((byte[])nodeIds[i].Identifier).Length);
                else
                    nodeIdRepresentations[i] = nodeIds[i].NamespaceIndex + "-" + nodeIds[i].Identifier.ToString();
            }
            var jArray = new JArray(nodeIdRepresentations);

            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var nodeIds = (NodeId[])matrix.Elements;
            string[] nodeIdRepresentations = new string[nodeIds.Length];
            for (int i = 0; i < nodeIds.Length; i++)
            {
                if (nodeIds[i].IdType == IdType.Opaque)
                    nodeIdRepresentations[i] = nodeIds[i].NamespaceIndex + "-" + Convert.ToBase64String((byte[])nodeIds[i].Identifier, 0, ((byte[])nodeIds[i].Identifier).Length);
                else
                    nodeIdRepresentations[i] = nodeIds[i].NamespaceIndex + "-" + nodeIds[i].Identifier.ToString();
            }
            var arr = (new Matrix(nodeIdRepresentations, BuiltInType.String, matrix.Dimensions)).ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);

            return jArr;
        }
    }

    private JToken SerializeQualifiedName(VariableNode variableNode, Variant value)
    {

        if (variableNode.ValueRank == -1)
        {
            var jStringVal = JObject.FromObject((QualifiedName)value.Value);

            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Array)((QualifiedName[])value.Value).Select(JObject.FromObject).ToArray();
            var jArray = new JArray(arr);


            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();

            var transformedArr = IterativeCopy<QualifiedName, JObject>(arr, matrix.Dimensions, JObject.FromObject);
            var arrStr = JsonConvert.SerializeObject(transformedArr);
            var jArr = JArray.Parse(arrStr);

            return jArr;
        }
    }

    private JToken SerializeSByte(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jIntVal = new JValue(value.Value);
            return jIntVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var bytes = (SByte[])value.Value;
            int[] byteRepresentations = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                byteRepresentations[i] = Convert.ToInt32(bytes[i]);
            }
            var jArray = new JArray(byteRepresentations);

            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);


            return jArr;
        }
    }
    private JToken SerializeStatusCode(VariableNode variableNode, Variant value)
    {

        if (variableNode.ValueRank == -1)
        {
            var statusValue = new PlatformStatusCode((StatusCode)value.Value);
            var jStringVal = JObject.FromObject(statusValue);

            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Array)((StatusCode[])value.Value).Select(val => JObject.FromObject(new PlatformStatusCode(val))).ToArray();

            var jArray = new JArray(arr);

            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();

            var transformedArr = IterativeCopy<StatusCode, JObject>(arr, matrix.Dimensions, i => JObject.FromObject(new PlatformStatusCode(i)));
            var arrStr = JsonConvert.SerializeObject(transformedArr);
            var jArr = JArray.Parse(arrStr);

            return jArr;
        }
    }

    private JToken SerializeString(VariableNode variableNode, Variant value)
    {
        if (variableNode.ValueRank == -1)
        {
            var jStringVal = new JValue(value.Value.ToString());
            return jStringVal;
        }
        else if (variableNode.ValueRank == 1)
        {
            var arr = (Array)value.Value;
            var jArray = new JArray(arr);

            return jArray;
        }
        else
        {
            var matrix = (Matrix)value.Value;
            var arr = matrix.ToArray();
            var arrStr = JsonConvert.SerializeObject(arr);
            var jArr = JArray.Parse(arrStr);

            return jArr;
        }
    }
    #endregion


    #region Write UA Values

    public DataValue GetDataValueFromVariableState(JToken variableState, VariableNode variableNode)
    {
        int actualValueRank = variableState.CalculateActualValueRank();
        if (!ValueRanks.IsValid(actualValueRank, variableNode.ValueRank))
            throw new("Rank of the Value provided does not match the Variable ValueRank");

        PlatformJsonDecoder platformJsonDecoder;
        var type = TypeInfo.GetBuiltInType(variableNode.DataType, _session.SystemContext.TypeTable);

        switch (actualValueRank)
        {
            case -1:
                platformJsonDecoder = PlatformJsonDecoder.CreateDecoder(
                    JsonConvert.SerializeObject(new { Value = variableState }),
                    variableNode.DataType,
                    _session);
                return GetDataValue(type.GetDecodeDelegate(platformJsonDecoder));
            case 1:
                platformJsonDecoder = PlatformJsonDecoder.CreateDecoder(
                    JsonConvert.SerializeObject(new { Value = variableState }),
                    variableNode.DataType,
                    _session);
                return GetDataValue(type.GetDecodeArrayDelegate(platformJsonDecoder));
            default:
                var dimensions = variableState.GetJsonArrayDimensions();
                variableState = variableState.ToOneDimensionJArray();
                platformJsonDecoder = PlatformJsonDecoder.CreateDecoder(
                    JsonConvert.SerializeObject(new { Value = variableState }),
                    variableNode.DataType,
                    _session,
                    dimensions);
                return GetDataValue(type.GetDecodeMatrixDelegate(platformJsonDecoder));
        }
    }

    private DataValue GetDataValue(Func<Variant> decode)
    {
        var valueToWrite = decode();
        return new DataValue(valueToWrite);
    }

    #endregion
}


internal class PlatformStatusCode
{
    public readonly string code;
    public readonly bool structureChanged;

    public PlatformStatusCode(StatusCode statusCode)
    {
        code = Regex.Match(statusCode.ToString(), @"(Good|Uncertain|Bad)").Groups[1].ToString();
        structureChanged = statusCode.StructureChanged;
    }
}