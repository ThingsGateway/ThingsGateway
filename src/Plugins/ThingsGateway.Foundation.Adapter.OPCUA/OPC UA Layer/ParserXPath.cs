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

using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace WebPlatform.OPCUALayer
{
    public class ParserXPath
    {
        private readonly XPathNavigator _nav;
        private readonly XmlNamespaceManager _ns;
        private BinaryDecoder _bd;

        public ParserXPath(string dict)
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

        /// <summary>
        /// Function that parses complex types
        /// </summary>
        /// <param name="descriptionId"></param>
        /// <param name="extensionObject"></param>
        /// <param name="context"></param>
        /// <param name="generateSchema"></param>
        /// <returns></returns>
        public UaValue Parse(string descriptionId, ExtensionObject extensionObject, ServiceMessageContext context, bool generateSchema)
        {
            _bd = new BinaryDecoder((byte[])extensionObject.Body, context);

            return BuildJsonForObject(descriptionId, generateSchema);
        }

        private UaValue BuildJsonForObject(string descriptionId, bool generateSchema)
        {
            var complexObj = new JObject();
            var complexSchema = generateSchema ? new JSchema { Type = JSchemaType.Object } : null;

            XPathNodeIterator iterator = _nav.Select($"/opc:TypeDictionary/opc:StructuredType[@Name='{descriptionId}']", _ns);

            while (iterator.MoveNext())
            {
                var structuredBaseType = iterator.Current.GetAttribute("BaseType", "");

                if (structuredBaseType == "ua:Union") throw new NotSupportedException("Union decoding not implemented in the current version of the platform");

                XPathNodeIterator newIterator = iterator.Current.SelectDescendants(XPathNodeType.Element, matchSelf: false);
                while (newIterator.MoveNext())
                {
                    if (newIterator.Current.Name.Equals("opc:Field"))
                    {
                        string fieldName = newIterator.Current.GetAttribute("Name", "");
                        string type = newIterator.Current.GetAttribute("TypeName", "");
                        string lengthSource = newIterator.Current.GetAttribute("LengthField", "");

                        int l = LengthField(lengthSource, complexObj);

                        if (!(type.Contains("opc:") || type.Contains("ua:")))
                        {
                            var uaValue = BuildInnerComplex(type.Split(':')[1], l, generateSchema);
                            complexObj[fieldName] = uaValue.Value;
                            if (generateSchema)
                            {
                                complexSchema.Properties.Add(fieldName, uaValue.Schema);
                            }
                        }
                        else
                        {
                            var uaValue = BuildSimple(type, l, generateSchema);
                            complexObj[fieldName] = uaValue.Value;
                            if (generateSchema)
                            {
                                complexSchema.Properties.Add(fieldName, uaValue.Schema);
                            }
                        }
                    }
                }
            }

            return new UaValue(complexObj, complexSchema);
        }

        private int LengthField(string lengthFieldSource, JToken currentJson)
        {
            if (string.IsNullOrEmpty(lengthFieldSource)) return 1;
            return int.Parse((string)currentJson[lengthFieldSource]);

        }

        private UaValue BuildSimple(string type, int length, bool generateSchema)
        {
            var builtinType = DataTypeAnalyzer.GetBuiltinTypeFromTypeName(type.Split(':')[0], type.Split(':')[1]);
            var jSchema = generateSchema ? DataTypeSchemaGenerator.GenerateSchemaForStandardTypeDescription(builtinType) : null;

            if (length == 1)
            {
                var jValue = JToken.FromObject(ReadBuiltinValue(builtinType));
                return new UaValue(jValue, jSchema);
            }

            var a = new List<object>();

            for (int i = 0; i < length; i++)
            {
                a.Add(ReadBuiltinValue(builtinType));
            }

            var arrSchema = generateSchema ? DataTypeSchemaGenerator.GenerateSchemaForArray(new[] { length }, jSchema) : null;

            return new UaValue(JToken.FromObject(a), arrSchema);
        }

        private UaValue BuildInnerComplex(string description, int length, bool generateSchema)
        {
            if (length == 1) return BuildJsonForObject(description, generateSchema);

            var jArray = new JArray();
            UaValue uaVal = new UaValue();

            for (int i = 0; i < length; i++)
            {
                uaVal = BuildJsonForObject(description, generateSchema);
                jArray.Insert(i, uaVal.Value);
            }

            var jSchema = (generateSchema) ? DataTypeSchemaGenerator.GenerateSchemaForArray(new[] { length }, uaVal.Schema) : null;

            return new UaValue(jArray, jSchema);
        }

        /// <summary>
        /// Read a Built-in value starting from the current cursor in the _bd BinaryDecoder
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Object ReadBuiltinValue(BuiltInType builtinType)
        {
            var methodToCall = "Read" + builtinType;

            MethodInfo mInfo = typeof(BinaryDecoder).GetMethod(methodToCall, new[] { typeof(string) });
            if (builtinType == BuiltInType.ByteString)
            {
                byte[] byteString = mInfo.Invoke(_bd, new object[] { "" }) as byte[];
                var base64ByteString = Convert.ToBase64String(byteString);
                return base64ByteString;
            }
            if (builtinType == BuiltInType.Guid)
            {
                string guid = mInfo.Invoke(_bd, new object[] { "" }).ToString();
                return guid;
            }
            if (builtinType == BuiltInType.ExtensionObject)
            {
                ExtensionObject extObject = mInfo.Invoke(_bd, new object[] { "" }) as ExtensionObject;
                return extObject.Body;
            }

            return mInfo.Invoke(_bd, new object[] { "" });
        }

    }
}