//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.OpcDa.Comn;

internal static class Convert
{
    /// <summary>
    /// windows的filetime是从1601-1-1 00:00:00开始的，datetime是从1-1-1 00:00:00开始的
    /// datetime和filetime的滴答单位都是100ns（100纳秒，千万分之一秒），所以转换时只需要考虑开始时间即可
    /// </summary>
    private static readonly DateTime FILETIME_BaseTime = new DateTime(1601, 1, 1);

    private static bool m_preserveUTC = false;

    internal static object Clone(object source)
    {
        if (source == null)
        {
            return null;
        }

        if (source.GetType().IsValueType)
        {
            return source;
        }

        if (source.GetType().IsArray || source.GetType() == typeof(Array))
        {
            Array array = (Array)((Array)source).Clone();
            for (int i = 0; i < array.Length; i++)
            {
                array.SetValue(Clone(array.GetValue(i)), i);
            }

            return array;
        }

        try
        {
            return ((ICloneable)source).Clone();
        }
        catch
        {
            throw new NotSupportedException("Object cannot be cloned.");
        }
    }

    internal static DateTime FileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME filetime)
    {
        long num = filetime.dwHighDateTime;
        if (num < 0)
        {
            num += 4294967296L;
        }
        long num2 = num << 32;
        num = filetime.dwLowDateTime;
        if (num < 0)
        {
            num += 4294967296L;
        }
        num2 += num;
        if (num2 == 0)
        {
            return DateTime.MinValue;
        }
        if (m_preserveUTC)
        {
            DateTime fILETIME_BaseTime = FILETIME_BaseTime;
            return fILETIME_BaseTime.Add(new TimeSpan(num2));
        }
        DateTime fILETIME_BaseTime2 = FILETIME_BaseTime;
        return fILETIME_BaseTime2.Add(new TimeSpan(num2)).ToLocalTime();
    }
}