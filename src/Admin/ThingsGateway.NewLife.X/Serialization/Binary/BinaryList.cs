﻿using System.Collections;

using ThingsGateway.NewLife.Reflection;

namespace ThingsGateway.NewLife.Serialization;

/// <summary>列表数据编码</summary>
public class BinaryList : BinaryHandlerBase
{
    /// <summary>初始化</summary>
    public BinaryList() => Priority = 20;

    /// <summary>写入</summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public override Boolean Write(Object? value, Type type)
    {
        if (!type.As<IList>() && value is not IList) return false;

        // 先写入长度
        if (value is not IList list || list.Count == 0)
        {
            Host.WriteSize(0);
            return true;
        }

        Host.WriteSize(list.Count);

        // 循环写入数据
        foreach (var item in list)
        {
            Host.Write(item);
        }

        return true;
    }

    /// <summary>读取</summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override Boolean TryRead(Type type, ref Object? value)
    {
        if (!type.As<IList>() && !type.As(typeof(IList<>))) return false;

        // 先读取长度
        var count = Host.ReadSize();
        if (count == 0) return true;

        // 子元素类型
        var elmType = type.GetElementTypeEx();

        if (value == null)
        {
            // 数组的创建比较特别
            if (type.As<Array>() && elmType != null)
            {
                value = Array.CreateInstance(elmType, count);
            }
            else
                value = type.CreateInstance();
        }

        if (elmType == null) return false;
        if (value is not IList list) return false;

        // 如果是数组，则需要先加起来，再
        //if (value is Array) list = typeof(IList<>).MakeGenericType(value.GetType().GetElementTypeEx()).CreateInstance() as IList;
        for (var i = 0; i < count; i++)
        {
            Object? obj = null;
            if (!Host.TryRead(elmType, ref obj)) return false;

            if (value is Array)
                list[i] = obj;
            else
                list.Add(obj);
        }

        return true;
    }
}