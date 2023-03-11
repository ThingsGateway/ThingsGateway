namespace OpcDaClient.Comn
{
    public static class Convert
    {
        public static object Clone(object source)
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


        //windows的filetime是从1601-1-1 00:00:00开始的，datetime是从1-1-1 00:00:00开始的
        //datetime和filetime的滴答单位都是100ns（100纳秒，千万分之一秒），所以转换时只需要考虑开始时间即可
        private static readonly DateTime FILETIME_BaseTime = new DateTime(1601, 1, 1);

        private static bool m_preserveUTC = false;
        public static DateTime FileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME filetime)
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
}
