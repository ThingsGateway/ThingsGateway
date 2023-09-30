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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 三元组合
    /// </summary>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public readonly struct Ternary<TKey1, TKey2, TValue>
    {
        /// <summary>
        /// 三元组合
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="value"></param>
        public Ternary(TKey1 key1, TKey2 key2, TValue value)
        {
            this.Key1 = key1;
            this.Key2 = key2;
            this.Value = value;
        }

        /// <summary>
        /// 首键
        /// </summary>
        public TKey1 Key1 { get; }

        /// <summary>
        /// 次键
        /// </summary>
        public TKey2 Key2 { get; }

        /// <summary>
        /// 值
        /// </summary>
        public TValue Value { get; }
    }

    /// <summary>
    /// 线程安全的双键字典
    /// </summary>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ConcurrentMultiDictionary<TKey1, TKey2, TValue>
    {
        private readonly ConcurrentDictionary<TKey1, Ternary<TKey1, TKey2, TValue>> m_key1ToValue =
            new ConcurrentDictionary<TKey1, Ternary<TKey1, TKey2, TValue>>();

        private readonly ConcurrentDictionary<TKey2, Ternary<TKey1, TKey2, TValue>> m_key2ToValue =
            new ConcurrentDictionary<TKey2, Ternary<TKey1, TKey2, TValue>>();

        /// <summary>
        /// 元素数量。
        /// </summary>
        public int Count { get => this.m_key1ToValue.Count; }
        /// <summary>
        /// Key1集合
        /// </summary>
        public ICollection<TKey1> Key1s => this.m_key1ToValue.Keys;

        /// <summary>
        /// Key2集合
        /// </summary>
        public ICollection<TKey2> Key2s => this.m_key2ToValue.Keys;

        /// <summary>
        /// 清空所有元素。
        /// </summary>
        public void Clear()
        {
            this.m_key1ToValue.Clear();
            this.m_key2ToValue.Clear();
        }

        /// <summary>
        /// 是否包含指定键。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey2 key)
        {
            return this.m_key2ToValue.ContainsKey(key);
        }

        /// <summary>
        /// 是否包含指定键。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey1 key)
        {
            return this.m_key1ToValue.ContainsKey(key);
        }

        /// <summary>
        /// 尝试添加。
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(TKey1 key1, TKey2 key2, TValue value)
        {
            var ternary = new Ternary<TKey1, TKey2, TValue>(key1, key2, value);
            if (this.m_key1ToValue.TryAdd(key1, ternary) && this.m_key2ToValue.TryAdd(key2, ternary))
            {
                return true;
            }
            else
            {
                this.m_key1ToValue.TryRemove(key1, out _);
                this.m_key2ToValue.TryRemove(key2, out _);
                return false;
            }
        }

        /// <summary>
        /// 由首键删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemove(TKey1 key, out TValue value)
        {
            if (this.m_key1ToValue.TryRemove(key, out var ternary))
            {
                this.m_key2ToValue.TryRemove(ternary.Key2, out _);
                value = ternary.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// 由次键删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemove(TKey2 key, out TValue value)
        {
            if (this.m_key2ToValue.TryRemove(key, out var ternary))
            {
                this.m_key1ToValue.TryRemove(ternary.Key1, out _);
                value = ternary.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// 由首键获取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey1 key, out TValue value)
        {
            if (this.m_key1ToValue.TryGetValue(key, out var ternary))
            {
                value = ternary.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// 由次键获取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey2 key, out TValue value)
        {
            if (this.m_key2ToValue.TryGetValue(key, out var ternary))
            {
                value = ternary.Value;
                return true;
            }
            value = default;
            return false;
        }
    }
}