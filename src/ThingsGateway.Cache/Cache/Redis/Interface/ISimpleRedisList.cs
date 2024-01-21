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

namespace ThingsGateway.Cache
{
    /// <summary>
    /// Redis实例
    /// </summary>
    public partial interface ISimpleRedis
    {
        /// <summary>
        /// 获取Redis列表实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>RedisList实例,用于列表高级操作</returns>
        RedisList<T> GetRedisList<T>(string key);

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="values">值</param>
        void ListAdd<T>(string key, T values);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="values">值</param>
        /// <returns>添加数量</returns>
        int ListAddRange<T>(string key, IEnumerable<T> values);

        /// <summary>
        /// 清空列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        void ListClear<T>(string key);

        /// <summary>
        /// 是否包含指定元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        bool ListContains<T>(string key, T value);

        /// <summary>
        /// 复制到目标数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="array">目标数组</param>
        /// <param name="arrayIndex">下标</param>
        void ListCopyTo<T>(string key, T[] array, int arrayIndex);

        /// <summary>
        /// 获取元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value"></param>
        /// <returns>值</returns>
        T ListGet<T>(string key, T value);

        /// <summary>
        /// 获取所有元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>元素列表</returns>
        List<T> ListGetAll<T>(string key);

        /// <summary>
        /// 返回指定范围的列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="start">开始下标</param>
        /// <param name="end">结束下标</param>
        /// <returns>元素素组</returns>
        T[] ListGetRange<T>(string key, int start, int end);

        /// <summary>
        /// 查找指定元素位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value"></param>
        /// <returns>位置</returns>
        int ListIndexOf<T>(string key, T value);

        /// <summary>
        /// 移除并返回最左边一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>最左边的元素</returns>
        T ListLeftPop<T>(string key);

        /// <summary>
        /// 左边批量添加，返回队列元素总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="values"></param>
        /// <returns>队列元素总数</returns>
        int ListLeftPush<T>(string key, IEnumerable<T> values);

        /// <summary>
        /// 删除元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool ListRemove<T>(string key, T value);

        /// <summary>
        /// 删除指定位置数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="index"></param>
        void ListRemove<T>(string key, int index);

        /// <summary>
        /// 移除并返回最右边一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns>左右边的元素</returns>
        T ListRightPop<T>(string key);

        /// <summary>
        /// 右边批量添加，返回队列元素总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="values"></param>
        /// <returns>元素总数</returns>
        int ListRightPush<T>(string key, IEnumerable<T> values);
    }
}