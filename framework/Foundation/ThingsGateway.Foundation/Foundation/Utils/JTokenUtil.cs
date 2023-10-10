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

using Newtonsoft.Json.Linq;
namespace ThingsGateway.Foundation.Core;
/// <summary>
/// JTokenUtil
/// </summary>
public static class JTokenUtil
{
    /// <summary>
    /// GetJTokenFromObj
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static JToken GetJTokenFromObj(this string item)
    {
        //写入变量
        JToken tagValue;
        try
        {
            tagValue = JToken.Parse(item);
        }
        catch (Exception)
        {
            tagValue = JToken.Parse("\"" + item + "\"");
        }

        return tagValue;
    }
    /// <summary>
    /// GetObjFromJToken
    /// </summary>

    public static object GetObjFromJToken(this JToken jtoken)
    {
        var rank = jtoken.CalculateActualValueRank();
        object rawWriteValue;
        switch (rank)
        {
            case -1:
                rawWriteValue = ((JValue)jtoken).Value;
                break;
            default:
                var jarray = ((JArray)jtoken);
                rawWriteValue = jarray.Select(j => (object)j).ToArray();
                break;
        }

        return rawWriteValue;
    }

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
        return jTokens.Select(x => (x.Type == JTokenType.Integer) ? JTokenType.Float : x.Type)
            .All(t => t == checkType);
    }

    #endregion

}