using Newtonsoft.Json.Linq;

using Opc.Ua;

using System.Text.RegularExpressions;

namespace ThingsGateway.OPCUA;

public static class BuiltInExtensionMethods
{
    public static object GetDecodeArrayDelegate(this BuiltInType builtIn, ServerPlatformJSONDecoder decoder)
    {
        switch (builtIn)
        {
            case BuiltInType.Boolean:
                return decoder.ReadBooleanArray("Value").ToArray();
            case BuiltInType.SByte:
                return decoder.ReadSByteArray("Value").ToArray();
            case BuiltInType.Byte:
                return decoder.ReadByteArray("Value").ToArray();
            case BuiltInType.Int16:
                return decoder.ReadInt16Array("Value").ToArray();
            case BuiltInType.UInt16:
                return decoder.ReadUInt16Array("Value").ToArray();
            case BuiltInType.Int32:
                return decoder.ReadInt32Array("Value").ToArray();
            case BuiltInType.UInt32:
                return decoder.ReadUInt32Array("Value").ToArray();
            case BuiltInType.Int64:
                return decoder.ReadInt64Array("Value").ToArray();
            case BuiltInType.UInt64:
                return decoder.ReadUInt64Array("Value").ToArray();
            case BuiltInType.Float:
                return decoder.ReadFloatArray("Value").ToArray();
            case BuiltInType.Double:
                return decoder.ReadDoubleArray("Value").ToArray();
            case BuiltInType.String:
                return decoder.ReadStringArray("Value").ToArray();
            case BuiltInType.DateTime:
                return decoder.ReadDateTimeArray("Value").ToArray();
            case BuiltInType.Guid:
                return decoder.ReadGuidArray("Value").ToArray();
            case BuiltInType.ByteString:
                return decoder.ReadByteStringArray("Value").ToArray();
            case BuiltInType.XmlElement:
                return decoder.ReadXmlElementArray("Value").ToArray();
            case BuiltInType.NodeId:
                return decoder.ReadNodeIdArray("Value").ToArray();
            case BuiltInType.ExpandedNodeId:
                return decoder.ReadExpandedNodeIdArray("Value").ToArray();
            case BuiltInType.StatusCode:
                return decoder.ReadStatusCodeArray("Value").ToArray();
            case BuiltInType.QualifiedName:
                return decoder.ReadQualifiedNameArray("Value").ToArray();
            case BuiltInType.LocalizedText:
                return decoder.ReadLocalizedTextArray("Value").ToArray();
            case BuiltInType.ExtensionObject:
                return decoder.ReadExtensionObjectArray("Value").ToArray();
            case BuiltInType.DiagnosticInfo:
                return decoder.ReadDiagnosticInfoArray("Value").ToArray();
            default:
                throw new NotImplementedException();
        }
    }

    public static object GetDecodeDelegate(this BuiltInType builtIn, ServerPlatformJSONDecoder decoder)
    {
        switch (builtIn)
        {
            case BuiltInType.Boolean:
                return decoder.ReadBoolean("Value");
            case BuiltInType.SByte:
                return decoder.ReadSByte("Value");
            case BuiltInType.Byte:
                return decoder.ReadByte("Value");
            case BuiltInType.Int16:
                return decoder.ReadInt16("Value");
            case BuiltInType.UInt16:
                return decoder.ReadUInt16("Value");
            case BuiltInType.Int32:
                return decoder.ReadInt32("Value");
            case BuiltInType.UInt32:
                return decoder.ReadUInt32("Value");
            case BuiltInType.Int64:
                return decoder.ReadInt64("Value");
            case BuiltInType.UInt64:
                return decoder.ReadUInt64("Value");
            case BuiltInType.Float:
                return decoder.ReadFloat("Value");
            case BuiltInType.Double:
                return decoder.ReadDouble("Value");
            case BuiltInType.String:
                return decoder.ReadString("Value");
            case BuiltInType.DateTime:
                return decoder.ReadDateTime("Value");
            case BuiltInType.Guid:
                return decoder.ReadGuid("Value");
            case BuiltInType.ByteString:
                return decoder.ReadByteString("Value");
            case BuiltInType.XmlElement:
                return decoder.ReadXmlElement("Value");
            case BuiltInType.NodeId:
                return decoder.ReadNodeId("Value");
            case BuiltInType.ExpandedNodeId:
                return decoder.ReadExpandedNodeId("Value");
            case BuiltInType.StatusCode:
                return decoder.ReadStatusCode("Value");
            case BuiltInType.QualifiedName:
                return decoder.ReadQualifiedName("Value");
            case BuiltInType.LocalizedText:
                return decoder.ReadLocalizedText("Value");
            case BuiltInType.ExtensionObject:
                return decoder.ReadExtensionObject("Value");
            case BuiltInType.DiagnosticInfo:
                return decoder.ReadDiagnosticInfo("Value");
            default:
                throw new NotImplementedException();
        }
    }
    public static object GetDecodeMatrixDelegate(this BuiltInType builtIn, ServerPlatformJSONDecoder decoder)
    {
        switch (builtIn)
        {
            case BuiltInType.Boolean:
                return decoder.ReadBooleanArray("Value").ToArray();
            case BuiltInType.SByte:
                return decoder.ReadSByteArray("Value").ToArray();
            case BuiltInType.Byte:
                return decoder.ReadByteArray("Value").ToArray();
            case BuiltInType.Int16:
                return decoder.ReadInt16Array("Value").ToArray();
            case BuiltInType.UInt16:
                return decoder.ReadUInt16Array("Value").ToArray();
            case BuiltInType.Int32:
                return decoder.ReadInt32Array("Value").ToArray();
            case BuiltInType.UInt32:
                return decoder.ReadUInt32Array("Value").ToArray();
            case BuiltInType.Int64:
                return decoder.ReadInt64Array("Value").ToArray();
            case BuiltInType.UInt64:
                return decoder.ReadUInt64Array("Value").ToArray();
            case BuiltInType.Float:
                return decoder.ReadFloatArray("Value").ToArray();
            case BuiltInType.Double:
                return decoder.ReadDoubleArray("Value").ToArray();
            case BuiltInType.String:
                return decoder.ReadStringArray("Value").ToArray();
            case BuiltInType.DateTime:
                return decoder.ReadDateTimeArray("Value").ToArray();
            case BuiltInType.Guid:
                return decoder.ReadGuidArray("Value").ToArray();
            case BuiltInType.ByteString:
                return decoder.ReadByteStringArray("Value").ToArray();
            case BuiltInType.XmlElement:
                return decoder.ReadXmlElementArray("Value").ToArray();
            case BuiltInType.NodeId:
                return decoder.ReadNodeIdArray("Value").ToArray();
            case BuiltInType.ExpandedNodeId:
                return decoder.ReadExpandedNodeIdArray("Value").ToArray();
            case BuiltInType.StatusCode:
                return decoder.ReadStatusCodeArray("Value").ToArray();
            case BuiltInType.QualifiedName:
                return decoder.ReadQualifiedNameArray("Value").ToArray();
            case BuiltInType.LocalizedText:
                return decoder.ReadLocalizedTextArray("Value").ToArray();
            case BuiltInType.ExtensionObject:
                return decoder.ReadExtensionObjectArray("Value").ToArray();
            case BuiltInType.DiagnosticInfo:
                return decoder.ReadDiagnosticInfoArray("Value").ToArray();
            default:
                throw new NotImplementedException();
        }
    }
}

public static class CollectionInitializerExtensionMethods
{
    public static void Add(this IList<JToken> list, IList<JToken> toAdd)
    {
        foreach (var a in toAdd)
        {
            list.Add(a);
        }
    }
}

public static class ExpandedNodeIdExtensionMethods
{
    public static string ToStringId(this ExpandedNodeId expandedNodeId, NamespaceTable namespaceTable)
    {
        var nodeId = ExpandedNodeId.ToNodeId(expandedNodeId, namespaceTable);
        return $"{nodeId.NamespaceIndex}-{nodeId.Identifier}";
    }
}
public static class JTokenExtensionMethods
{
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

    public static JTokenType GetElementsType(this JToken[] jTokens)
    {
        if (!jTokens.ElementsHasSameType())
            throw new Exception("The array sent must have the same type of element in each dimension");
        return jTokens.First().Type;
    }

    public static int[] GetJsonArrayDimensions(this JToken jToken)
    {
        if (jToken.Type != JTokenType.Array)
            throw new Exception("Expected a JSON Array but received a " + jToken.Type);
        while (jToken.HasValues)
        {
            var children = jToken.Children();
            var count = children.First().Count();

            //if(children.All(x => x.Count() == count)) throw new Exception("The array sent must have the same number of element in each dimension");

            foreach (var child in children)
            {
                if (child.Count() != count)
                    throw new Exception("The array sent must have the same number of element in each dimension");
            }
            jToken = jToken.Last;
        }

        const string pattern = @"\[(\d+)\]";
        var regex = new Regex(pattern);
        var matchColl = regex.Matches(jToken.Path);
        var dimensions = new int[matchColl.Count];
        for (var i = 0; i < matchColl.Count; i++)
        {
            dimensions[i] = int.Parse(matchColl[i].Groups[1].Value) + 1;
        }
        return dimensions;
    }

    public static JArray ToOneDimensionJArray(this JToken jToken)
    {
        var dimensions = jToken.GetJsonArrayDimensions();
        return jToken.ToOneDimensionJArray(dimensions);
    }

    public static JArray ToOneDimensionJArray(this JToken jToken, int[] dimensions)
    {
        var flatValuesToWrite = jToken.Children().ToArray();
        for (var i = 0; i < dimensions.Length - 1; i++)
            flatValuesToWrite = flatValuesToWrite.SelectMany(a => a).ToArray();

        return new JArray(flatValuesToWrite);
    }
    private static bool ElementsHasSameType(this JToken[] jTokens)
    {
        var checkType = jTokens[0].Type == JTokenType.Integer ? JTokenType.Float : jTokens[0].Type;
        return jTokens
            .Select(x => (x.Type == JTokenType.Integer) ? JTokenType.Float : x.Type)
            .All(t => t == checkType);
    }

}
