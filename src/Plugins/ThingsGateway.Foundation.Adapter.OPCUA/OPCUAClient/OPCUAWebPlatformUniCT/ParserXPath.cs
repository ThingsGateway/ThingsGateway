#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Newtonsoft.Json.Linq;

using Opc.Ua;

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace ThingsGateway.Foundation.Adapter.OPCUA
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
        public JToken Parse(string descriptionId, ExtensionObject extensionObject, IServiceMessageContext context)
        {
            _bd = new BinaryDecoder((byte[])extensionObject.Body, context);

            return BuildJsonForObject(descriptionId);
        }

        private JToken BuildInnerComplex(string description, int length)
        {
            if (length == 1) return BuildJsonForObject(description);

            var jArray = new JArray();
            JToken uaVal;

            for (int i = 0; i < length; i++)
            {
                uaVal = BuildJsonForObject(description);
                jArray.Insert(i, uaVal);
            }

            return jArray;
        }

        private JToken BuildJsonForObject(string descriptionId)
        {
            var complexObj = new JObject();

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
                            var uaValue = BuildInnerComplex(type.Split(':')[1], l);
                            complexObj[fieldName] = uaValue;
                        }
                        else
                        {
                            var uaValue = BuildSimple(type, l);
                            complexObj[fieldName] = uaValue;
                        }
                    }
                }
            }
            return complexObj;
        }

        private JToken BuildSimple(string type, int length)
        {
            var builtinType = DataTypeAnalyzer.GetBuiltinTypeFromTypeName(type.Split(':')[0], type.Split(':')[1]);

            if (length == 1)
            {
                var jValue = JToken.FromObject(ReadBuiltinValue(builtinType));
                return jValue;
            }

            var a = new List<object>();

            for (int i = 0; i < length; i++)
            {
                a.Add(ReadBuiltinValue(builtinType));
            }

            return JToken.FromObject(a);
        }

        private int LengthField(string lengthFieldSource, JToken currentJson)
        {
            if (string.IsNullOrEmpty(lengthFieldSource)) return 1;
            return int.Parse((string)currentJson[lengthFieldSource]);

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