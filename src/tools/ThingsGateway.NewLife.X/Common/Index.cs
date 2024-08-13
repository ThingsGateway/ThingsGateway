
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




#if NETFRAMEWORK || NETSTANDARD2_0

using System.Runtime.CompilerServices;

namespace System;

/// <summary></summary>
public readonly struct Index : IEquatable<Index>
{
    /// <summary></summary>
    private readonly Int32 _value;

    /// <summary></summary>
    public static Index Start => new(0);

    /// <summary></summary>
    public static Index End => new(-1);

    /// <summary></summary>
    public Int32 Value => _value < 0 ? ~_value : _value;

    /// <summary></summary>
    public Boolean IsFromEnd => _value < 0;

    /// <summary></summary>
    /// <param name="value"></param>
    /// <param name="fromEnd"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Index(Int32 value, Boolean fromEnd = false)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException("value", "value must be non-negative");
        }
        _value = fromEnd ? ~value : value;
    }

    private Index(Int32 value) => _value = value;

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromStart(Int32 value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException("value", "value must be non-negative");
        }
        return new Index(value);
    }

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromEnd(Int32 value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException("value", "value must be non-negative");
        }
        return new Index(~value);
    }

    /// <summary></summary>
    /// <param name="length"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int32 GetOffset(Int32 length)
    {
        var offset = _value;
        if (IsFromEnd)
        {
            offset += length + 1;
        }
        return offset;
    }

    /// <summary></summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override Boolean Equals(Object value) => value is Index index && _value == index._value;

    /// <summary></summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Boolean Equals(Index other) => _value == other._value;

    /// <summary></summary>
    /// <returns></returns>
    public override Int32 GetHashCode() => _value;

    /// <summary></summary>
    /// <param name="value"></param>
    public static implicit operator Index(Int32 value) => FromStart(value);

    /// <summary></summary>
    /// <returns></returns>
    public override String ToString()
    {
        if (IsFromEnd)
        {
            return "^" + (UInt32)Value;
        }
        return ((UInt32)Value).ToString();
    }
}

#endif
