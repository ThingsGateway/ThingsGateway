// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Extension.Generic;

/// <inheritdoc/>
public static class GenericExtensions
{
    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度<br />
    /// </summary>
    public static T[] ArrayExpandToLength<T>(this T[] data, int length)
    {
        if (data == null)
        {
            return new T[length];
        }

        if (data.Length == length)
        {
            return data;
        }
        Array.Resize(ref data, length);

        return data;
    }

    /// <summary>
    /// 将一个数组进行扩充到偶数长度<br />
    /// </summary>
    public static T[] ArrayExpandToLengthEven<T>(this T[] data)
    {
        if (data == null)
        {
            return Array.Empty<T>();
        }

        return data.Length % 2 == 1 ? data.ArrayExpandToLength(data.Length + 1) : data;
    }

    /// <summary>
    /// <inheritdoc cref="ArrayRemoveDouble{T}(T[], int, int)"/>
    /// </summary>
    public static T[] ArrayRemoveBegin<T>(T[] value, int length) => ArrayRemoveDouble(value, length, 0);

    /// <summary>
    /// 从数组中移除指定数量的元素，并返回新的数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">要移除元素的数组</param>
    /// <param name="leftLength">从左侧移除的元素个数</param>
    /// <param name="rightLength">从右侧移除的元素个数</param>
    /// <returns>移除元素后的新数组</returns>
    public static T[] ArrayRemoveDouble<T>(T[] value, int leftLength, int rightLength)
    {
        // 如果输入数组为空或者剩余长度不足以移除左右两侧指定的元素，则返回空数组
        if (value == null || value.Length <= leftLength + rightLength)
        {
            return Array.Empty<T>();
        }

        // 计算新数组的长度
        int newLength = value.Length - leftLength - rightLength;

        // 创建新数组
        T[] result = new T[newLength];

        // 将剩余的元素复制到新数组中
        Array.Copy(value, leftLength, result, 0, newLength);

        return result;
    }

    /// <summary>
    /// 将指定的数据按照指定长度进行分割
    /// </summary>
    public static List<T[]> ArraySplitByLength<T>(this T[] array, int length)
    {
        if (array == null || array.Length == 0)
        {
            return new List<T[]>();
        }

        int arrayLength = array.Length;
        int numArrays = (arrayLength + length - 1) / length; // 计算所需的数组数量

        List<T[]> objArrayList = new List<T[]>(numArrays);
        for (int i = 0; i < arrayLength; i += length)
        {
            int remainingLength = Math.Min(arrayLength - i, length);
            T[] destinationArray = new T[remainingLength];
            Array.Copy(array, i, destinationArray, 0, remainingLength);
            objArrayList.Add(destinationArray);
        }

        return objArrayList;
    }

    /// <summary>
    /// 将项目列表分解为特定大小的块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">原数组</param>
    /// <param name="chunkSize">分组大小</param>
    /// <param name="isToList">是否ToList</param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> ChunkBetter<T>(this IEnumerable<T> source, int chunkSize, bool isToList = false)
    {
        if (chunkSize <= 0)
            chunkSize = source.Count();
        var pos = 0;
        while (source.Skip(pos).Any())
        {
            var chunk = source.Skip(pos).Take(chunkSize);
            yield return isToList ? chunk.ToList() : chunk;
            pos += chunkSize;
        }
    }

    /// <summary>拷贝当前的实例数组，是基于引用层的浅拷贝，如果类型为值类型，那就是深度拷贝，如果类型为引用类型，就是浅拷贝</summary>
    public static T[] CopyArray<T>(this T[] value)
    {
        if (value == null)
        {
            return Array.Empty<T>();
        }

        T[] destinationArray = new T[value.Length];
        Array.Copy(value, destinationArray, value.Length);
        return destinationArray;
    }

    /// <summary>将一个一维数组中的所有数据按照行列信息拷贝到二维数组里，返回当前的二维数组</summary>
    public static T[,] CreateTwoArrayFromOneArray<T>(this T[] array, int row, int col)
    {
        T[,] arrayFromOneArray = new T[row, col];
        int index = 0;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                arrayFromOneArray[i, j] = array[index++];
                if (index >= array.Length) return arrayFromOneArray; // 防止数组越界
            }
        }

        return arrayFromOneArray;
    }

    /// <summary>
    /// 将一个数组的前后移除指定位数，返回新的一个数组<br />
    /// </summary>
    public static T[] RemoveArray<T>(this T[] value, int leftLength, int rightLength)
    {
        if (value == null || value.Length == 0)
        {
            return Array.Empty<T>();
        }

        int newLength = value.Length - leftLength - rightLength;
        if (newLength <= 0)
        {
            return Array.Empty<T>();
        }

        T[] result = new T[newLength];
        Array.Copy(value, leftLength, result, 0, newLength);

        return result;
    }

    /// <summary>
    /// 将一个数组的前面指定位数移除，返回新的一个数组<br />
    /// </summary>
    public static T[] RemoveBegin<T>(this T[] value, int length) => value.RemoveArray(length, 0);

    /// <summary>
    /// 将一个数组的后面指定位数移除，返回新的一个数组<br />
    /// </summary>
    public static T[] RemoveLast<T>(this T[] value, int length) => value.RemoveArray(0, length);

    /// <summary>
    /// 选择数组中的最后几个元素组成新的数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">输入数组</param>
    /// <param name="length">选择的元素个数</param>
    /// <returns>由最后几个元素组成的新数组</returns>
    public static T[] SelectLast<T>(this T[] value, int length)
    {
        // 如果输入数组为空，则返回空数组
        if (value == null || value.Length == 0)
        {
            return Array.Empty<T>();
        }

        // 计算实际需要复制的元素个数，取输入数组长度和指定长度的较小值
        int count = Math.Min(value.Length, length);

        // 创建新数组来存储选择的元素
        T[] result = new T[count];

        // 复制最后几个元素到新数组中
        Array.Copy(value, value.Length - count, result, 0, count);

        return result;
    }

    /// <summary>
    /// 从数组中获取指定索引开始的中间一段长度的子数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="value">输入数组</param>
    /// <param name="index">起始索引</param>
    /// <param name="length">选择的元素个数</param>
    /// <returns>中间指定长度的子数组</returns>
    public static T[] SelectMiddle<T>(this T[] value, int index, int length)
    {
        // 如果输入数组为空，则返回空数组
        if (value == null || value.Length == 0)
        {
            return Array.Empty<T>();
        }

        // 计算实际需要复制的元素个数，取输入数组剩余元素和指定长度的较小值
        int count = Math.Min(value.Length - index, length);

        // 创建新数组来存储选择的元素
        T[] result = new T[count];

        // 复制中间指定长度的元素到新数组中
        Array.Copy(value, index, result, 0, count);

        return result;
    }
}
