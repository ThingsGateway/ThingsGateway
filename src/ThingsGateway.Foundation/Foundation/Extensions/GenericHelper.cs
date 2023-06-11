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

using System.Linq;

namespace ThingsGateway.Foundation.Extension;

/// <inheritdoc/>
public static class GenericHelper
{
    /// <summary>
    /// 将一个数组进行扩充到指定长度，或是缩短到指定长度<br />
    /// </summary>
    public static T[] ArrayExpandToLength<T>(this T[] data, int length)
    {
        if (data == null)
            return new T[length];
        if (data.Length == length)
            return data;
        T[] destinationArray = new T[length];
        Array.Copy(data, destinationArray, Math.Min(data.Length, destinationArray.Length));
        return destinationArray;
    }

    /// <summary>
    /// 将一个数组进行扩充到偶数长度<br />
    /// </summary>
    public static T[] ArrayExpandToLengthEven<T>(this T[] data)
    {
        if (data == null)
            return new T[0];
        return data.Length % 2 == 1 ? data.ArrayExpandToLength(data.Length + 1) : data;
    }

    /// <summary>
    /// 将一个数组的前面指定位数移除，返回新的一个数组<br />
    /// </summary>
    public static T[] ArrayRemoveBegin<T>(T[] value, int length) => ArrayRemoveDouble<T>(value, length, 0);

    /// <summary>
    /// 将一个数组的前后移除指定位数，返回新的一个数组<br />
    /// </summary>
    public static T[] ArrayRemoveDouble<T>(T[] value, int leftLength, int rightLength)
    {
        if (value == null)
            return (T[])null;
        if (value.Length <= leftLength + rightLength)
            return new T[0];
        T[] destinationArray = new T[value.Length - leftLength - rightLength];
        Array.Copy((Array)value, leftLength, (Array)destinationArray, 0, destinationArray.Length);
        return destinationArray;
    }

    /// <summary>
    /// 将指定的数据按照指定长度进行分割
    /// </summary>
    public static List<T[]> ArraySplitByLength<T>(this T[] array, int length)
    {
        if (array == null)
            return new List<T[]>();
        List<T[]> objArrayList = new List<T[]>();
        int sourceIndex = 0;
        while (sourceIndex < array.Length)
        {
            if (sourceIndex + length < array.Length)
            {
                T[] destinationArray = new T[length];
                Array.Copy((Array)array, sourceIndex, (Array)destinationArray, 0, length);
                sourceIndex += length;
                objArrayList.Add(destinationArray);
            }
            else
            {
                T[] destinationArray = new T[array.Length - sourceIndex];
                Array.Copy((Array)array, sourceIndex, (Array)destinationArray, 0, destinationArray.Length);
                sourceIndex += length;
                objArrayList.Add(destinationArray);
            }
        }
        return objArrayList;
    }

    /// <summary>拷贝当前的实例数组，是基于引用层的浅拷贝，如果类型为值类型，那就是深度拷贝，如果类型为引用类型，就是浅拷贝</summary>
    public static T[] CopyArray<T>(this T[] value)
    {
        if (value == null)
        {
            return null;
        }

        T[] destinationArray = new T[value.Length];
        Array.Copy(value, destinationArray, value.Length);
        return destinationArray;
    }

    /// <summary>将一个一维数组中的所有数据按照行列信息拷贝到二维数组里，返回当前的二维数组</summary>
    public static T[,] CreateTwoArrayFromOneArray<T>(this T[] array, int row, int col)
    {
        T[,] arrayFromOneArray = new T[row, col];
        int index1 = 0;
        for (int index2 = 0; index2 < row; ++index2)
        {
            for (int index3 = 0; index3 < col; ++index3)
            {
                arrayFromOneArray[index2, index3] = array[index1];
                ++index1;
            }
        }
        return arrayFromOneArray;
    }
    /// <summary>
    /// For循环
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="objs"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> objs, Action<T> action)
    {
        foreach (T obj in objs)
        {
            action(obj);
        }
    }

    /// <summary>
    /// 将一个数组的前后移除指定位数，返回新的一个数组<br />
    /// </summary>
    public static T[] RemoveArray<T>(this T[] value, int leftLength, int rightLength)
    {
        if (value == null)
            return null;
        if (value.Length <= leftLength + rightLength)
            return new T[0];
        T[] destinationArray = new T[value.Length - leftLength - rightLength];
        Array.Copy(value, leftLength, destinationArray, 0, destinationArray.Length);
        return destinationArray;
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
    /// RmoveWhere
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="where"></param>
    public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> where)
    {
        foreach (T item in @this.Where(where).ToList())
        {
            @this.Remove(item);
        }
    }

    /// <summary>
    /// 选择一个数组的后面的几个数据信息<br />
    /// </summary>
    public static T[] SelectLast<T>(this T[] value, int length)
    {
        T[] destinationArray = new T[Math.Min(value.Length, length)];
        Array.Copy((Array)value, value.Length - length, (Array)destinationArray, 0, destinationArray.Length);
        return destinationArray;
    }

    /// <summary>
    /// 获取到数组里面的中间指定长度的数组<br />
    /// </summary>
    public static T[] SelectMiddle<T>(this T[] value, int index, int length)
    {
        if (value == null)
            return null;
        T[] destinationArray = new T[Math.Min(value.Length, length)];
        Array.Copy(value, index, destinationArray, 0, destinationArray.Length);
        return destinationArray;
    }

    /// <inheritdoc cref="DataHelper.SpliceArray" />
    public static T[] SpliceArray<T>(this T[] value, params T[][] arrays)
    {
        List<T[]> objArrayList = new List<T[]>(arrays.Length + 1)
        {
            value
        };
        objArrayList.AddRange(arrays);
        return DataHelper.SpliceArray<T>(objArrayList.ToArray());
    }
}