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

using Newtonsoft.Json.Linq;

using Opc.Ua;

using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using WebPlatform.Extensions;
using WebPlatform.OPCUALayer;

namespace WebPlatform.OPC_UA_Layer
{
    public class StructuredEncoder
    {
        private readonly XPathNavigator _nav;
        private readonly XmlNamespaceManager _ns;
        private BinaryEncoder _be;

        public StructuredEncoder(string dict)
        {
            using (TextReader sr = new StringReader(dict))
            {
                var pathDoc = new XPathDocument(sr);
                _nav = pathDoc.CreateNavigator();

                //add all xmlns to namespaceManager.
                _nav.MoveToFollowing(XPathNodeType.Element);

                IDictionary<string, string> namespaces = _nav.GetNamespacesInScope(XmlNamespaceScope.All);
                _ns = new XmlNamespaceManager(_nav.NameTable);

                foreach (KeyValuePair<string, string> entry in namespaces)
                    _ns.AddNamespace(entry.Key, entry.Value);
            }
        }

        public ExtensionObject BuildExtensionObjectFromJsonObject(string descriptionId, JObject dataToEncode, ServiceMessageContext serviceMessageContext, NodeId dataTypeEncodingNodeId)
        {
            _be = new BinaryEncoder(serviceMessageContext);
            EncodeJsonObject(descriptionId, dataToEncode);
            return new ExtensionObject(dataTypeEncodingNodeId, _be.CloseAndReturnBuffer());
        }

        private void EncodeJsonObject(string descriptionId, JObject dataToEncode)
        {
            XPathNodeIterator iterator = _nav.Select($"/opc:TypeDictionary/opc:StructuredType[@Name='{descriptionId}']", _ns);
            while (iterator.MoveNext())
            {
                XPathNodeIterator newIterator = iterator.Current.SelectDescendants(XPathNodeType.Element, matchSelf: false);
                while (newIterator.MoveNext())
                {
                    if (newIterator.Current.Name.Equals("opc:Field"))
                    {
                        string fieldName = newIterator.Current.GetAttribute("Name", "");

                        string typeName = newIterator.Current.GetAttribute("TypeName", "");
                        string lengthSource = newIterator.Current.GetAttribute("LengthField", "");

                        if (!dataToEncode.ContainsKey(fieldName))
                            throw new ValueToWriteTypeException("Wrong Object Properties: Expected a property named " + fieldName);
                        JToken innerData = dataToEncode[fieldName];
                        int l = LengthField(lengthSource, dataToEncode);
                        if (!(typeName.Contains("opc:") || typeName.Contains("ua:")))
                        {
                            if (l == 1)
                            {
                                if (innerData.Type != JTokenType.Object)
                                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a JSON Object");
                                EncodeJsonObject(typeName.Split(':')[1], innerData.ToObject<JObject>());
                            }
                            else
                            {
                                if (innerData.Type != JTokenType.Array)
                                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array");
                                JToken[] jtArray = innerData.Children().ToArray();
                                if (jtArray.Length != l)
                                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + l);
                                if (jtArray.GetElementsType() != JTokenType.Object)
                                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of JSON Object");
                                for (int i = 0; i < l; i++)
                                {
                                    EncodeJsonObject(typeName.Split(':')[1], jtArray[i].ToObject<JObject>());
                                }
                            }
                        }
                        else
                        {
                            var builtinType = DataTypeAnalyzer.GetBuiltinTypeFromTypeName(typeName.Split(':')[0], typeName.Split(':')[1]);
                            switch (builtinType)
                            {
                                case BuiltInType.Boolean:
                                    EncodeBoolean(innerData, fieldName, l);
                                    break;
                                case BuiltInType.SByte:
                                    EncodeSByte(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Byte:
                                    EncodeByte(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Int16:
                                    EncodeInt16(innerData, fieldName, l);
                                    break;
                                case BuiltInType.UInt16:
                                    EncodeUInt16(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Int32:
                                    EncodeInt32(innerData, fieldName, l);
                                    break;
                                case BuiltInType.UInt32:
                                    EncodeUInt32(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Int64:
                                    EncodeInt64(innerData, fieldName, l);
                                    break;
                                case BuiltInType.UInt64:
                                    EncodeUInt64(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Float:
                                    EncodeFloat(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Double:
                                    EncodeDouble(innerData, fieldName, l);
                                    break;
                                case BuiltInType.String:
                                    EncodeString(innerData, fieldName, l);
                                    break;
                                case BuiltInType.DateTime:
                                    EncodeDateTime(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Guid:
                                    EncodeGuid(innerData, fieldName, l);
                                    break;
                                case BuiltInType.DiagnosticInfo:
                                    throw new NotImplementedException("Write of DiagnosticInfo element is not implemented");
                                case BuiltInType.LocalizedText:
                                    EncodeLocalizedText(innerData, fieldName, l);
                                    break;
                                case BuiltInType.NodeId:
                                    EncodeNodeId(innerData, fieldName, l);
                                    break;
                                case BuiltInType.ExpandedNodeId:
                                    EncodeExpandedNodeId(innerData, fieldName, l);
                                    break;
                                case BuiltInType.StatusCode:
                                    EncodeStatusCode(innerData, fieldName, l);
                                    break;
                                case BuiltInType.QualifiedName:
                                    EncodeQualifiedName(innerData, fieldName, l);
                                    break;
                                case BuiltInType.XmlElement:
                                    throw new NotImplementedException("Write of Xml element is not implemented");
                                case BuiltInType.ByteString:
                                    EncodeByteString(innerData, fieldName, l);
                                    break;
                                case BuiltInType.Enumeration:
                                    throw new NotImplementedException("Write of Enumeration element is not implemented");
                                default:
                                    throw new NotImplementedException("Write of " + builtinType.ToString() + " element is not implemented");
                            }
                        }
                    }
                }
            }
        }

        private void EncodeBoolean(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Boolean)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Boolean but received a " + jToken.Type.ToString());
                _be.WriteBoolean(fieldName, jToken.ToObject<Boolean>());
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Boolean)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of JSON Boolean");
                Boolean[] valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Boolean>()));
                _be.WriteBooleanArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeInt16(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                Int16 valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<Int16>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteInt16(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                Int16[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Int16>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteInt16Array(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeInt32(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                Int32 valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<Int32>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteInt32(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                Int32[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Int32>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteInt32Array(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeInt64(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                Int64 valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<Int64>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteInt64(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                Int64[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Int64>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteInt64Array(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeUInt16(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                UInt16 valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<UInt16>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteUInt16(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                UInt16[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<UInt16>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteUInt16Array(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeUInt32(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                UInt32 valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<UInt32>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteUInt32(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                UInt32[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<UInt32>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteUInt32Array(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeUInt64(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                UInt64 valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<UInt64>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteUInt64(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                UInt64[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<UInt64>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteUInt64Array(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeByte(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                Byte valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<Byte>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteByte(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                Byte[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Byte>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteByteArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeSByte(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                SByte valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<SByte>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteSByte(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                SByte[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<SByte>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteSByteArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeFloat(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Float && jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Number but received a " + jToken.Type.ToString());
                Single valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<Single>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteFloat(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Number");
                Single[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Single>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteFloatArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeDouble(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Float && jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Number but received a " + jToken.Type.ToString());
                Double valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<Double>();
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteDouble(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Number");
                Double[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Double>()));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteDoubleArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeString(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a String but received a " + jToken.Type.ToString());
                _be.WriteString(fieldName, jToken.ToObject<String>());
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of JSON String");
                String[] valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<String>()));
                _be.WriteStringArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeGuid(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a String but received a " + jToken.Type.ToString());
                Guid valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<Guid>();
                }
                catch (FormatException exc)
                {
                    throw new ValueToWriteTypeException("String not formatted correctly. " + exc.Message);
                }

                _be.WriteGuid(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of JSON String");
                Guid[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<Guid>()));
                }
                catch (FormatException exc)
                {
                    throw new ValueToWriteTypeException("One or more Strings in the Array not formatted correctly. " + exc.Message);
                }
                _be.WriteGuidArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeDateTime(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Date)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Date but received a " + jToken.Type.ToString());
                DateTime valueToWrite;
                try
                {
                    valueToWrite = jToken.ToObject<DateTime>();
                }
                catch (FormatException exc)
                {
                    throw new ValueToWriteTypeException("String not formatted correctly. " + exc.Message);
                }

                _be.WriteDateTime(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Date)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Date");
                DateTime[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => item.ToObject<DateTime>()));
                }
                catch (FormatException exc)
                {
                    throw new ValueToWriteTypeException("One or more Strings in the Array not formatted correctly. " + exc.Message);
                }
                _be.WriteDateTimeArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeStatusCode(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Integer but received a " + jToken.Type.ToString());
                StatusCode valueToWrite;
                try
                {
                    valueToWrite = new StatusCode(jToken.ToObject<UInt32>());
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteStatusCode(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Integer)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Integer");
                StatusCode[] valuesToWriteArray;
                try
                {
                    valuesToWriteArray = Array.ConvertAll(jtArray, (item => new StatusCode(item.ToObject<UInt32>())));
                }
                catch (OverflowException exc)
                {
                    throw new ValueToWriteTypeException("Range Error: " + exc.Message);
                }
                _be.WriteStatusCodeArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeLocalizedText(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Object)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Object but received a " + jToken.Type.ToString());
                LocalizedText valueToWrite = null;
                JObject jObject = jToken.ToObject<JObject>();
                if (!jObject.ContainsKey("Locale") || !jObject.ContainsKey("Text"))
                    throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string Properties \"Locale\" and \"Text\"");
                JToken jtLocale = jObject["Locale"];
                JToken jtText = jObject["Text"];
                if (jtLocale.Type != jtText.Type && jtLocale.Type != JTokenType.String)
                    throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string Properties \"Locale\" and \"Text\"");
                valueToWrite = new LocalizedText(jtLocale.ToObject<String>(), jtText.ToObject<String>());
                _be.WriteLocalizedText(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Object)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Object");
                LocalizedText[] valuesToWriteArray = new LocalizedText[lenghtOfArray];
                JObject jObject;
                for (int i = 0; i < lenghtOfArray; i++)
                {
                    jObject = jtArray[i].ToObject<JObject>();
                    if (!jObject.ContainsKey("Locale") || !jObject.ContainsKey("Text"))
                        throw new ValueToWriteTypeException("The elements of the Array Property " + fieldName + " must be object made up by the string Properties \"Locale\" and \"Text\"");
                    JToken jtLocale = jObject["Locale"];
                    JToken jtText = jObject["Text"];
                    if (jtLocale.Type != jtText.Type && jtLocale.Type != JTokenType.String)
                        throw new ValueToWriteTypeException("The elements of the Array Property " + fieldName + " must be object made up by the string Properties \"Locale\" and \"Text\"");
                    valuesToWriteArray[i] = new LocalizedText(jtLocale.ToObject<String>(), jtText.ToObject<String>());
                }
                _be.WriteLocalizedTextArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeNodeId(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a String but received a " + jToken.Type.ToString());
                _be.WriteNodeId(fieldName, ParsePlatformNodeIdString(jToken.ToObject<String>()));
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of JSON String");
                NodeId[] valuesToWriteArray = Array.ConvertAll(jtArray, (item => ParsePlatformNodeIdString(item.ToObject<String>())));
                _be.WriteNodeIdArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeExpandedNodeId(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                ExpandedNodeId valueToWrite;
                if (jToken.Type != JTokenType.Object)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Object but received a " + jToken.Type.ToString());
                JObject jObject = jToken.ToObject<JObject>();
                if (!jObject.ContainsKey("NodeId") || jObject["NodeId"].Type != JTokenType.String)
                    throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string Property \"NodeId\"");
                if (!jObject.ContainsKey("NamespaceUri") || jObject["NamespaceUri"].Type != JTokenType.String)
                    throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string Property \"NamespaceUri\"");
                if (!jObject.ContainsKey("ServerIndex") || jObject["ServerIndex"].Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("The Object " + fieldName + " must have the integer Property \"ServerIndex\"");
                NodeId nodeId = ParsePlatformNodeIdString(jObject["NodeId"].ToObject<String>());
                valueToWrite = new ExpandedNodeId(nodeId, jObject["NamespaceUri"].ToObject<String>(), jObject["ServerIndex"].ToObject<UInt32>());
                _be.WriteExpandedNodeId(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Object)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of JSON Object");
                JObject jObject;
                NodeId nodeId;
                ExpandedNodeId[] valuesToWriteArray = new ExpandedNodeId[lenghtOfArray];
                for (int i = 0; i < lenghtOfArray; i++)
                {
                    jObject = jtArray[i].ToObject<JObject>();
                    if (!jObject.ContainsKey("NodeId") || jObject["NodeId"].Type != JTokenType.String)
                        throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string Property \"NodeId\"");
                    if (!jObject.ContainsKey("NamespaceUri") || jObject["NamespaceUri"].Type != JTokenType.String)
                        throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string Property \"NamespaceUri\"");
                    if (!jObject.ContainsKey("ServerIndex") || jObject["ServerIndex"].Type != JTokenType.Integer)
                        throw new ValueToWriteTypeException("The Object " + fieldName + " must have the integer Property \"ServerIndex\"");
                    nodeId = ParsePlatformNodeIdString(jObject["NodeId"].ToObject<String>());
                    valuesToWriteArray[i] = new ExpandedNodeId(nodeId, jObject["NamespaceUri"].ToObject<String>(), jObject["ServerIndex"].ToObject<UInt32>());
                }
                _be.WriteExpandedNodeIdArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeByteString(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a String but received a " + jToken.Type.ToString());
                _be.WriteByteString(fieldName, Convert.FromBase64String(jToken.ToObject<String>()));
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.String)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of JSON String");
                Byte[][] valuesToWriteArray = Array.ConvertAll(jtArray, (item => Convert.FromBase64String(jToken.ToObject<String>())));
                _be.WriteByteStringArray(fieldName, valuesToWriteArray);
            }
        }

        private void EncodeQualifiedName(JToken jToken, string fieldName, int lenghtOfArray)
        {
            if (lenghtOfArray == 1)
            {
                if (jToken.Type != JTokenType.Object)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be a Object but received a " + jToken.Type.ToString());
                QualifiedName valueToWrite = null;
                JObject jObject = jToken.ToObject<JObject>();
                if (!jObject.ContainsKey("Name") || !jObject.ContainsKey("NamespaceIndex"))
                    throw new ValueToWriteTypeException("The Object " + fieldName + " must have the Properties \"Name\" and \"NamespaceIndex\"");
                JToken jtName = jObject["Name"];
                JToken jtNamespaceIndex = jObject["NamespaceIndex"];
                if (jtName.Type != JTokenType.String || jtNamespaceIndex.Type != JTokenType.Integer)
                    throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string property \"Name\" and the integer property \"NamespaceIndex\"");
                valueToWrite = new QualifiedName(jtName.ToObject<String>(), jtNamespaceIndex.ToObject<UInt16>());
                _be.WriteQualifiedName(fieldName, valueToWrite);
            }
            else
            {
                if (jToken.Type != JTokenType.Array)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array but received a " + jToken.Type.ToString());
                JToken[] jtArray = jToken.Children().ToArray();
                if (jtArray.Length != lenghtOfArray)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The Array property named " + fieldName + " should be an Array with lenght = " + lenghtOfArray);
                if (jtArray.GetElementsType() != JTokenType.Object)
                    throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + fieldName + " should be an Array of Object");
                QualifiedName[] valuesToWriteArray = new QualifiedName[lenghtOfArray];
                JObject jObject;
                for (int i = 0; i < lenghtOfArray; i++)
                {
                    jObject = jtArray[i].ToObject<JObject>();
                    if (!jObject.ContainsKey("Name") || !jObject.ContainsKey("NamespaceIndex"))
                        throw new ValueToWriteTypeException("The Object " + fieldName + " must have the Properties \"Name\" and \"NamespaceIndex\"");
                    JToken jtName = jObject["Name"];
                    JToken jtNamespaceIndex = jObject["NamespaceIndex"];
                    if (jtName.Type != JTokenType.String || jtNamespaceIndex.Type != JTokenType.Integer)
                        throw new ValueToWriteTypeException("The Object " + fieldName + " must have the string property \"Name\" and the integer property \"NamespaceIndex\"");
                    valuesToWriteArray[i] = new QualifiedName(jtName.ToObject<String>(), jtNamespaceIndex.ToObject<UInt16>());
                }
                _be.WriteQualifiedNameArray(fieldName, valuesToWriteArray);
            }
        }

        private int LengthField(string lengthFieldSource, JObject currentJson)
        {
            if (string.IsNullOrEmpty(lengthFieldSource)) return 1;
            if (!currentJson.ContainsKey(lengthFieldSource))
                throw new ValueToWriteTypeException("Wrong Object Properties: Expected a property named " + lengthFieldSource);
            if (currentJson[lengthFieldSource].Type != JTokenType.Integer)
                throw new ValueToWriteTypeException("Wrong Object Properties: The property named " + lengthFieldSource + " should be a JSON Integer");
            return currentJson[lengthFieldSource].ToObject<int>();
        }

        private NodeId ParsePlatformNodeIdString(string str)
        {
            const string pattern = @"^(\d+)-(?:(\d+)|(\S+))$";
            var match = Regex.Match(str, pattern);
            if (!match.Success)
                return null;
            var isString = match.Groups[3].Length != 0;
            var isNumeric = match.Groups[2].Length != 0;

            var idStr = (isString) ? $"s={match.Groups[3]}" : $"i={match.Groups[2]}";
            var builtStr = $"ns={match.Groups[1]};" + idStr;
            NodeId nodeId = null;
            try
            {
                nodeId = new NodeId(builtStr);
            }
            catch (ServiceResultException exc)
            {
                switch (exc.StatusCode)
                {
                    case StatusCodes.BadNodeIdInvalid:
                        throw new ValueToWriteTypeException("Wrong Type Error: String is not formatted as expected (number-yyy where yyy can be string or number or guid)");
                    default:
                        throw new ValueToWriteTypeException(exc.Message);
                }
            }


            return nodeId;
        }
    }
}