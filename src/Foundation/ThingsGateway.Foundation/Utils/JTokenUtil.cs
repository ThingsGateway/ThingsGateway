//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace ThingsGateway.Foundation;

/// <summary>
/// JTokenUtil
/// </summary>
public static class JTokenUtil
{
    /// <summary>
    /// 根据字符串解析对应JToken<br></br>
    /// 字符串可以不包含转义双引号，如果解析失败会直接转成String类型的JValue
    /// true/false可忽略大小写
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static JToken GetJTokenFromString(this string item)
    {
        try
        {
            // 尝试解析字符串为 JToken 对象
            return JToken.Parse(item);
        }
        catch
        {
            if (bool.TryParse(item, out bool parseBool))
            {
                return new JValue(parseBool);
            }
            // 解析失败时，将其转为 String 类型的 JValue
            return new JValue(item);
        }
    }

    /// <summary>
    /// 根据JToken获取Object类型值<br></br>
    /// 对应返回 对象字典 或 类型数组 或 类型值
    /// </summary>
    public static object? GetObjectFromJToken(this JToken jtoken)
    {
        switch (jtoken.Type)
        {
            case JTokenType.Object:
                // 如果是对象类型，递归调用本方法获取嵌套的键值对
                return jtoken.Children<JProperty>()
                    .ToDictionary(prop => prop.Name, prop => GetObjectFromJToken(prop.Value));

            case JTokenType.Array:
                // 如果是数组类型，递归调用本方法获取嵌套的元素
                return jtoken.Select(GetObjectFromJToken).ToArray();

            default:
                // 其他类型直接转换为对应的 Object 类型值
                return (jtoken as JValue)?.Value;
        }
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
        return jTokens.First().Type;
    }

    #endregion json
}
