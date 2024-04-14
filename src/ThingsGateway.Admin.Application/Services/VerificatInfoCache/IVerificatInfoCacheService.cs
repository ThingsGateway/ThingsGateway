
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Admin.Application;

public interface IVerificatInfoCacheService
{
    /// <summary>
    /// 根据id获取单个VerificatInfo列表
    /// </summary>
    /// <param name="id">要查询的id</param>
    /// <returns>VerificatInfo列表</returns>
    List<VerificatInfo> HashGetOne(long id);

    /// <summary>
    /// 根据多个id获取VerificatInfo列表集合
    /// </summary>
    /// <param name="ids">要查询的id数组</param>
    /// <returns>VerificatInfo列表集合</returns>
    IEnumerable<List<VerificatInfo>> HashGet(long[] ids);

    /// <summary>
    /// 添加或更新指定id的VerificatInfo列表
    /// </summary>
    /// <param name="id">要添加或更新的id</param>
    /// <param name="verificatInfos">要添加或更新的VerificatInfo列表</param>
    void HashAdd(long id, List<VerificatInfo> verificatInfos);

    /// <summary>
    /// 获取所有VerificatInfo数据
    /// </summary>
    Dictionary<long, List<VerificatInfo>> GetAll();

    /// <summary>
    /// 设置整个VerificatInfo缓存数据
    /// </summary>
    /// <param name="dictionary">以id为键，VerificatInfo列表为值的字典</param>
    void HashSet(Dictionary<long, List<VerificatInfo>> dictionary);

    /// <summary>
    /// 删除所有VerificatInfo缓存数据
    /// </summary>
    void Remove();

    /// <summary>
    /// 根据id删除对应的VerificatInfo数据
    /// </summary>
    /// <param name="ids">要删除的id数组</param>
    void HashDel(params long[] ids);

    /// <summary>
    /// 持久化数据
    /// </summary>
    /// <param name="dictionary">数据</param>
    void HashSetDB(Dictionary<long, List<VerificatInfo>> dictionary);
}