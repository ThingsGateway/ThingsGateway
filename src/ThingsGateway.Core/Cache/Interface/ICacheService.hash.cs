//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Core;

/// <summary>
/// 缓存服务
/// </summary>
public partial interface ICacheService
{
    /// <summary>
    /// 添加一条数据到HashMap
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">键</param>
    /// <param name="hashKey">hash列表里的Key</param>
    /// <param name="value">值</param>
    void HashAdd<T>(string key, string hashKey, T value);

    /// <summary>
    /// 添加多条数据到HashMap
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">键</param>
    /// <param name="dic">键值对字典</param>
    /// <returns></returns>
    bool HashSet<T>(string key, Dictionary<string, T> dic);

    /// <summary>
    /// 从HashMap中删除数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">键</param>
    /// <param name="fields">hash键列表</param>
    /// <returns>执行结果</returns>
    int HashDel<T>(string key, params string[] fields);

    /// <summary>
    /// 根据键获取hash列表中的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">键</param>
    /// <param name="fields">hash键列表</param>
    /// <returns>数据列表</returns>
    List<T> HashGet<T>(string key, params string[] fields);

    /// <summary>
    /// 根据键获取hash列表中的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">键</param>
    /// <param name="field">hash键</param>
    /// <returns></returns>
    T HashGetOne<T>(string key, string field);

    /// <summary>
    /// 获取所有键值对
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">键</param>
    /// <returns>数据字典</returns>
    IDictionary<string, T> HashGetAll<T>(string key);
}