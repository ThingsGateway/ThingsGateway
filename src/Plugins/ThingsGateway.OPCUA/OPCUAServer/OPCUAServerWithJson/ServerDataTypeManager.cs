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

using System.Text.RegularExpressions;

using TypeInfo = Opc.Ua.TypeInfo;

namespace ThingsGateway.OPCUA;

public class ServerDataTypeManager
{
    private readonly IServiceMessageContext _messageContext;
    private readonly SystemContext _systemContext;
    public ServerDataTypeManager(IServiceMessageContext messageContext, SystemContext systemContext)
    {
        _messageContext = messageContext;
        _systemContext = systemContext;
    }


    #region Write UA Values

    public object GetDataValueFromVariableState(JToken variableState, NodeId dataType)
    {
        int actualValueRank = variableState.CalculateActualValueRank();
        //if (!ValueRanks.IsValid(actualValueRank, variableNode.ValueRank))
        //    throw new("Rank of the Value provided does not match the Variable ValueRank");

        ServerPlatformJSONDecoder platformJsonDecoder;
        var type = TypeInfo.GetBuiltInType(dataType, _systemContext.TypeTable);
        switch (actualValueRank)
        {
            case -1:
                platformJsonDecoder = ServerPlatformJSONDecoder.CreateDecoder(
                    JsonConvert.SerializeObject(new { Value = variableState }),
                    dataType,
                    _messageContext);
                return type.GetDecodeDelegate(platformJsonDecoder);
            case 1:
                platformJsonDecoder = ServerPlatformJSONDecoder.CreateDecoder(
                    JsonConvert.SerializeObject(new { Value = variableState }),
                    dataType,
                   _messageContext);
                return type.GetDecodeArrayDelegate(platformJsonDecoder);
            default:
                var dimensions = variableState.GetJsonArrayDimensions();
                variableState = variableState.ToOneDimensionJArray();
                platformJsonDecoder = ServerPlatformJSONDecoder.CreateDecoder(
                    JsonConvert.SerializeObject(new { Value = variableState }),
                    dataType,
                   _messageContext,
                    dimensions);
                return type.GetDecodeMatrixDelegate(platformJsonDecoder);
        }
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