
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------




#if NETFRAMEWORK || NETSTANDARD2_0

using System.Runtime.CompilerServices;

namespace System;

/// <summary></summary>
public readonly struct Range : IEquatable<Range>
{
    /// <summary></summary>
    public Index Start { get; }

    /// <summary></summary>
    public Index End { get; }

    /// <summary></summary>
    public static Range All => new(Index.Start, Index.End);

    /// <summary></summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public Range(Index start, Index end)
    {
        Start = start;
        End = end;
    }

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override Boolean Equals(Object value)
    {
        if (value is Range r)
        {
            if (r.Start.Equals(Start))
            {
                return r.End.Equals(End);
            }
        }
        return false;
    }

    /// <summary></summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Boolean Equals(Range other) => other.Start.Equals(Start) && other.End.Equals(End);

    /// <summary></summary>
    /// <returns></returns>
    public override Int32 GetHashCode() => Start.GetHashCode() * 31 + End.GetHashCode();

    /// <summary></summary>
    /// <returns></returns>
    public override String ToString() => Start.ToString() + ".." + End;

    /// <summary></summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public static Range StartAt(Index start) => new(start, Index.End);

    /// <summary></summary>
    /// <param name="end"></param>
    /// <returns></returns>
    public static Range EndAt(Index end) => new(Index.Start, end);

    /// <summary></summary>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Int32 Offset, Int32 Length) GetOffsetAndLength(Int32 length)
    {
        var startIndex = Start;
        var start = ((!startIndex.IsFromEnd) ? startIndex.Value : (length - startIndex.Value));
        var endIndex = End;
        var end = ((!endIndex.IsFromEnd) ? endIndex.Value : (length - endIndex.Value));
        if ((UInt32)end > (UInt32)length || (UInt32)start > (UInt32)end)
        {
            throw new ArgumentOutOfRangeException("length");
        }
        return (start, end - start);
    }
}

#endif
