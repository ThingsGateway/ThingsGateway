// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Diagnostics;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     带应用速率限制的流
/// </summary>
/// <remarks>
///     <para>基于令牌桶算法（Token Bucket Algorithm）实现流量控制和速率限制。</para>
///     <para>参考文献：https://baike.baidu.com/item/令牌桶算法/6597000。</para>
/// </remarks>
public sealed class RateLimitedStream : Stream
{
    /// <summary>
    ///     单次读取或写入操作中处理的最大数据块大小
    /// </summary>
    internal const int CHUNK_SIZE = 4096;

    /// <summary>
    ///     每秒允许传输的最大字节数
    /// </summary>
    internal readonly double _bytesPerSecond;

    /// <inheritdoc cref="Stream" />
    internal readonly Stream _innerStream;

    /// <summary>
    ///     用于同步访问的锁对象
    /// </summary>
    internal readonly object _lockObject = new();

    /// <summary>
    ///     用来计算时间间隔的计时器
    /// </summary>
    internal readonly Stopwatch _stopwatch;

    /// <summary>
    ///     当前可用的令牌数量（字节数）
    /// </summary>
    internal double _availableTokens;

    /// <summary>
    ///     上次令牌补充的时间戳
    /// </summary>
    internal long _lastTokenRefillTime;

    /// <summary>
    ///     <inheritdoc cref="RateLimitedStream" />
    /// </summary>
    /// <param name="innerStream">
    ///     <see cref="Stream" />
    /// </param>
    /// <param name="bytesPerSecond">每秒允许传输的最大字节数</param>
    public RateLimitedStream(Stream innerStream, double bytesPerSecond)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(innerStream);

        // 小于或等于 0 检查
        if (bytesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesPerSecond),
                "The bytes per second must be greater than zero.");
        }

        _innerStream = innerStream;
        _bytesPerSecond = bytesPerSecond;

        // 开始计时
        _stopwatch = Stopwatch.StartNew();

        // 记录初始时间
        _lastTokenRefillTime = _stopwatch.ElapsedMilliseconds;

        // 初始化可用令牌数
        _availableTokens = bytesPerSecond;
    }

    /// <inheritdoc />
    public override bool CanRead => _innerStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => _innerStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => _innerStream.CanWrite;

    /// <inheritdoc />
    public override bool CanTimeout => _innerStream.CanTimeout;

    /// <inheritdoc />
    public override long Length => _innerStream.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    /// <inheritdoc />
    public override void Flush() => _innerStream.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        // 确保单次读取不会超过预设的数据块大小
        var adjustedCount = Math.Min(count, CHUNK_SIZE);

        // 等待直到有足够令牌可用
        WaitForTokens(adjustedCount);

        // 从内部流读取数据到缓冲区
        return _innerStream.Read(buffer, offset, adjustedCount);
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => _innerStream.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        // 确保单次写入不会超过预设的数据块大小
        var adjustedCount = Math.Min(count, CHUNK_SIZE);

        // 等待直到有足够令牌可用
        WaitForTokens(adjustedCount);

        // 向内部流写入数据
        _innerStream.Write(buffer, offset, adjustedCount);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // 释放托管资源
        if (disposing)
        {
            _innerStream.Dispose();
            _stopwatch.Stop();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     补充令牌的方法
    /// </summary>
    internal void RefillTokens()
    {
        // 获取当前计时器的时间
        var now = _stopwatch.ElapsedMilliseconds;

        // 计算自上次填充令牌以来经过的时间
        var timePassed = now - _lastTokenRefillTime;

        // 如果时间没有流逝或者流逝时间不足以产生新的令牌，则直接返回
        if (timePassed <= 0)
        {
            return;
        }

        // 据每秒允许的最大字节数以及经过的时间计算可以补充的令牌数量
        var newTokens = _bytesPerSecond * timePassed / 1000.0;

        // 更新可用令牌，但不超过每秒允许的最大值
        _availableTokens = Math.Min(_bytesPerSecond, _availableTokens + newTokens);

        // 更新最后一次填充令牌的时间戳
        _lastTokenRefillTime = now;
    }

    /// <summary>
    ///     等待直到有足够令牌可用
    /// </summary>
    /// <param name="desiredTokens">需要等待的令牌数量</param>
    internal void WaitForTokens(int desiredTokens)
    {
        while (true)
        {
            // 防止并发访问问题
            lock (_lockObject)
            {
                //  尝试补充令牌
                RefillTokens();

                // 检查是否已有足够的令牌
                if (_availableTokens >= desiredTokens)
                {
                    // 扣除所需的令牌数量
                    _availableTokens -= desiredTokens;

                    // 如果有足够的令牌，退出循环
                    return;
                }
            }

            // 如果没有足够的令牌，计算还需要多少令牌
            var requiredTokens = desiredTokens - _availableTokens;

            // 计算为了获得所需令牌需要等待的时间
            var waitTime = (int)(requiredTokens * 1000.0 / _bytesPerSecond);

            // 添加一点额外延迟用来确保精确性，具体是增加了 5% 的延迟
            waitTime = (int)(waitTime * 1.05);

            // 确保不会一次性等待过长时间，最多等待 100 毫秒
            if (waitTime > 0)
            {
                Thread.Sleep(Math.Min(100, waitTime));
            }
        }
    }
}