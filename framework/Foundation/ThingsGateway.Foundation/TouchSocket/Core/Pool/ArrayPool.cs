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

using System.Diagnostics;

#if NET6_0_OR_GREATER
using System.Numerics;
#endif

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 提供一个数组对象的池化容器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ArrayPool<T>
    {
        private const int m_defaultMaxArrayLength = 1024 * 1024;

        private const int m_defaultMaxNumberOfArraysPerBucket = 50;

        private readonly Bucket[] m_buckets;

        /// <summary>
        /// 提供一个数组对象的池化容器。
        /// </summary>
        public ArrayPool() : this(m_defaultMaxArrayLength, m_defaultMaxNumberOfArraysPerBucket)
        {
        }

        /// <summary>
        /// 提供一个数组对象的池化容器。
        /// </summary>
        /// <param name="maxArrayLength"></param>
        /// <param name="maxArraysPerBucket"></param>
        public ArrayPool(int maxArrayLength, int maxArraysPerBucket)
        {
            const int MinimumArrayLength = 16, MaximumArrayLength = int.MaxValue;
            if (maxArrayLength > MaximumArrayLength)
            {
                maxArrayLength = MaximumArrayLength;
            }
            else if (maxArrayLength < MinimumArrayLength)
            {
                maxArrayLength = MinimumArrayLength;
            }

            var capacity = 0L;
            var poolId = this.Id;
            var maxBuckets = SelectBucketIndex(maxArrayLength);
            var buckets = new Bucket[maxBuckets + 1];
            for (var i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new Bucket(GetMaxSizeForBucket(i), maxArraysPerBucket, poolId);
                long num = GetMaxSizeForBucket(i) * maxArraysPerBucket;
                capacity += num;
            }
            this.m_buckets = buckets;
            this.Capacity = capacity;
        }

        /// <summary>
        /// 对象池的最大容量。
        /// </summary>
        public long Capacity { get; private set; }

        /// <summary>
        /// 清理池中所有对象。
        /// </summary>
        public void Clear()
        {
            foreach (var item in this.m_buckets)
            {
                item.Clear();
            }
        }

        /// <summary>
        /// 获取当前池中的所有对象。
        /// </summary>
        /// <returns></returns>
        public long GetPoolSize()
        {
            long size = 0;
            foreach (var item in this.m_buckets)
            {
                size += item.Size;
            }
            return size;
        }

        /// <summary>
        /// 最大请求尺寸梯度。
        /// </summary>
        public int MaxBucketsToTry { get; set; } = 5;

        private int Id => this.GetHashCode();

        /// <summary>
        /// 获取一个不小于指定尺寸的池化数组对象。
        /// </summary>
        /// <param name="minimumLength"></param>
        /// <returns></returns>
        public virtual T[] Rent(int minimumLength)
        {
            if (minimumLength == 0)
            {
#if !NET45_OR_GREATER
                return Array.Empty<T>();
#else
                return new T[0];
#endif
            }

            T[] buffer;

            var index = SelectBucketIndex(minimumLength);
            if (index < this.m_buckets.Length)
            {
                var i = index;
                do
                {
                    buffer = this.m_buckets[i].Rent();
                    if (buffer != null)
                    {
                        return buffer;
                    }
                }
                while (++i < this.m_buckets.Length && i != index + this.MaxBucketsToTry);

                buffer = new T[this.m_buckets[index].m_bufferLength];
            }
            else
            {
                buffer = new T[minimumLength];
            }

            return buffer;
        }

        /// <summary>
        /// 归还池化对象。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="clearArray"></param>
        public virtual void Return(T[] array, bool clearArray = false)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length == 0)
            {
                return;
            }

            var bucket = SelectBucketIndex(array.Length);

            var haveBucket = bucket < this.m_buckets.Length;
            if (haveBucket)
            {
                if (clearArray)
                {
                    Array.Clear(array, 0, array.Length);
                }

                this.m_buckets[bucket].Return(array);
            }
        }

        /// <summary>
        /// 命中匹配尺寸
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int HitSize(int size)
        {
            return GetMaxSizeForBucket(SelectBucketIndex(size));
        }

        internal static int GetMaxSizeForBucket(int binIndex)
        {
            return 16 << binIndex;
        }

        internal static int SelectBucketIndex(int bufferSize)
        {
#if NET6_0_OR_GREATER
            return BitOperations.Log2((uint)(bufferSize - 1) | 15u) - 3;
#else
            return (int)(Math.Log((uint)(bufferSize - 1) | 15u, 2) - 3);
#endif
        }

        [DebuggerDisplay("Count={Count},Size={Size}")]
        private sealed class Bucket
        {
            internal readonly int m_bufferLength;
            private readonly int m_numberOfBuffers;
            private T[][] m_buffers;
            private readonly int m_poolId;

            private int m_index;
            private SpinLock m_lock;

            internal Bucket(int bufferLength, int numberOfBuffers, int poolId)
            {
                this.m_lock = new SpinLock(Debugger.IsAttached);
                this.m_buffers = new T[numberOfBuffers][];
                this.m_bufferLength = bufferLength;
                this.m_numberOfBuffers = numberOfBuffers;
                this.m_poolId = poolId;
            }

            public void Clear()
            {
                var lockTaken = false;
                try
                {
                    this.m_lock.Enter(ref lockTaken);
                    this.m_buffers = new T[this.m_numberOfBuffers][];
                    this.m_index = 0;
                }
                finally
                {
                    if (lockTaken) this.m_lock.Exit(false);
                }
            }

            public int Count
            {
                get
                {
                    var lockTaken = false;
                    try
                    {
                        this.m_lock.Enter(ref lockTaken);

                        var count = 0;
                        foreach (var item in this.m_buffers)
                        {
                            if (item != null)
                            {
                                count++;
                            }
                        }

                        return count;
                    }
                    finally
                    {
                        if (lockTaken) this.m_lock.Exit(false);
                    }
                }
            }

            internal int Id => this.GetHashCode();

            public long Size
            {
                get
                {
                    var lockTaken = false;
                    try
                    {
                        this.m_lock.Enter(ref lockTaken);

                        long size = 0;
                        foreach (var item in this.m_buffers)
                        {
                            if (item != null)
                            {
                                size += item.LongLength;
                            }
                        }

                        return size;
                    }
                    finally
                    {
                        if (lockTaken) this.m_lock.Exit(false);
                    }
                }
            }

            internal T[] Rent()
            {
                T[] buffer = null;

                bool lockTaken = false, allocateBuffer = false;
                try
                {
                    this.m_lock.Enter(ref lockTaken);

                    if (this.m_index < this.m_buffers.Length)
                    {
                        buffer = this.m_buffers[this.m_index];
                        this.m_buffers[this.m_index++] = null;
                        allocateBuffer = buffer == null;
                    }
                }
                finally
                {
                    if (lockTaken) this.m_lock.Exit(false);
                }

                if (allocateBuffer)
                {
                    buffer = new T[this.m_bufferLength];
                }

                return buffer;
            }

            internal void Return(T[] array)
            {
                if (array.Length != this.m_bufferLength)
                {
                    throw new ArgumentException();
                }

                bool returned;

                var lockTaken = false;
                try
                {
                    this.m_lock.Enter(ref lockTaken);

                    returned = this.m_index != 0;
                    if (returned)
                    {
                        this.m_buffers[--this.m_index] = array;
                    }
                }
                finally
                {
                    if (lockTaken) this.m_lock.Exit(false);
                }
            }
        }
    }
}