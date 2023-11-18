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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 安全双向字典
    /// </summary>
    public class ConcurrentDoublyDictionary<TKey, TValue>
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConcurrentDoublyDictionary()
        {
            this.KeyToValue = new ConcurrentDictionary<TKey, TValue>();
            this.ValueToKey = new ConcurrentDictionary<TValue, TKey>();
        }

        /// <summary>
        /// 由键指向值得集合
        /// </summary>
        public ConcurrentDictionary<TKey, TValue> KeyToValue { get; private set; }

        /// <summary>
        /// 由值指向键的集合
        /// </summary>
        public ConcurrentDictionary<TValue, TKey> ValueToKey { get; private set; }

        /// <summary>
        ///  尝试将指定的键和值添加到字典中。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(TKey key, TValue value)
        {
            if (this.KeyToValue.TryAdd(key, value))
            {
                if (this.ValueToKey.TryAdd(value, key))
                {
                    return true;
                }
                else
                {
                    this.KeyToValue.TryRemove(key, out _);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 由键尝试移除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemoveFromKey(TKey key, out TValue value)
        {
            if (this.KeyToValue.TryRemove(key, out value))
            {
                if (this.ValueToKey.TryRemove(value, out _))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 由值尝试移除
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryRemoveFromValue(TValue value, out TKey key)
        {
            if (this.ValueToKey.TryRemove(value, out key))
            {
                if (this.KeyToValue.TryRemove(key, out _))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 由键获取到值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetFromKey(TKey key, out TValue value)
        {
            return this.KeyToValue.TryGetValue(key, out value);
        }

        /// <summary>
        /// 由值获取到键
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryGetFromValue(TValue value, out TKey key)
        {
            return this.ValueToKey.TryGetValue(value, out key);
        }
    }
}