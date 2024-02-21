//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Core.Extension.Json;

/// <summary>
/// JsonExtension
/// </summary>
public static class JsonExtension
{
    #region Json序列化和反序列化

    /// <summary>
    /// 转换为Json
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isIndented"></param>
    /// <returns></returns>
    public static string ToJsonString(this object item, bool isIndented = false)
    {
        if (isIndented)
            return Newtonsoft.Json.JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
        else
            return Newtonsoft.Json.JsonConvert.SerializeObject(item);
    }

    /// <summary>
    /// 从字符串到json
    /// </summary>
    /// <param name="json"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object FromJsonString(this string json, Type type)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject(json, type);
    }

    /// <summary>
    /// 从字符串到json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T FromJsonString<T>(this string json)
    {
        return (T)FromJsonString(json, typeof(T));
    }

    /// <summary>
    /// Json序列化数据对象
    /// </summary>
    /// <param name="obj">数据对象</param>
    /// <returns></returns>
    public static byte[] JsonSerializeToBytes(object obj)
    {
        return Encoding.UTF8.GetBytes(ToJsonString(obj));
    }

    /// <summary>
    /// Json序列化至文件
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    public static void JsonSerializeToFile(object obj, string path)
    {
        using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            var date = JsonSerializeToBytes(obj);
            fileStream.Write(date, 0, date.Length);
            fileStream.Close();
        }
    }

    /// <summary>
    /// Json反序列化
    /// </summary>
    /// <typeparam name="T">反序列化类型</typeparam>
    /// <param name="datas">数据</param>
    /// <returns></returns>
    public static T JsonDeserializeFromBytes<T>(byte[] datas)
    {
        return (T)JsonDeserializeFromBytes(datas, typeof(T));
    }

    /// <summary>
    /// Json反序列化
    /// </summary>
    /// <param name="datas"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object JsonDeserializeFromBytes(byte[] datas, Type type)
    {
        return FromJsonString(Encoding.UTF8.GetString(datas), type);
    }

    /// <summary>
    /// Json反序列化
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="json">json字符串</param>
    /// <returns></returns>
    public static T JsonDeserializeFromString<T>(string json)
    {
        return FromJsonString<T>(json);
    }

    /// <summary>
    /// Json反序列化
    /// </summary>
    /// <typeparam name="T">反序列化类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static T JsonDeserializeFromFile<T>(string path)
    {
        return JsonDeserializeFromString<T>(File.ReadAllText(path));
    }

    #endregion Json序列化和反序列化
}