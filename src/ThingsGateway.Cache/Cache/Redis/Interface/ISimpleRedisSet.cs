//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Caching.Models;

namespace ThingsGateway.Cache
{
    /// <summary>
    /// Redis实例
    /// </summary>
    public partial interface ISimpleRedis
    {
        /// <summary>
        /// 获取RedisSet实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>RedisSet实例</returns>
        RedisSet<T> GetRedisSet<T>(string key);

        /// <summary>
        /// 搜索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="model">SearchModel</param>
        /// <returns></returns>
        List<string> Search<T>(string key, SearchModel model);

        /// <summary>
        /// 搜索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="pattern">匹配表达式</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        List<string> Search<T>(string key, string pattern, int count);

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void SetAdd<T>(string key, T value);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value"></param>
        int SetAddList<T>(string key, T[] value);

        /// <summary>
        /// 清空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        void SetClear<T>(string key);

        /// <summary>
        /// 是否包含指定元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetContains<T>(string key, T value);

        /// <summary>
        /// 复制到目标数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="array">目标数组</param>
        /// <param name="arrayIndex">下标</param>
        void SetCopyTo<T>(string key, T[] array, int arrayIndex);

        /// <summary>
        /// 移除指定元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        bool SetDel<T>(string key, T value);

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>删除数量</returns>
        int SetDelRange<T>(string key, T[] value);

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        T[] SetGetAll<T>(string key);

        /// <summary>
        /// 随机获取并弹出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        T[] SetPop<T>(string key, int count);

        /// <summary>
        /// 随机获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        T[] SetRandom<T>(string key, int count);
    }
}