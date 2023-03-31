namespace ThingsGateway.Foundation.Extension
{
    /// <inheritdoc/>
    public static class ByteExtensions
    {
        /// <summary>
        /// 获取byte数据类型的第offset位，是否为True<br />
        /// </summary>
        /// <param name="value">byte数值</param>
        /// <param name="offset">索引位置</param>
        /// <returns>结果</returns>
        public static bool BoolOnByteIndex(this byte value, int offset)
        {
            byte dataByBitIndex = DataHelper.GetDataByBitIndex(offset);
            return (value & dataByBitIndex) == dataByBitIndex;
        }

        /// <summary>
        /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
        /// </summary>
        /// <param name="inBytes">输入的字节信息</param>
        /// <returns>反转后的数据</returns>
        public static byte[] BytesReverseByWord(this byte[] inBytes)
        {
            if (inBytes == null)
                return null;
            if (inBytes.Length == 0)
                return new byte[0];
            byte[] lengthEven = inBytes.CopyArray<byte>().ArrayExpandToLengthEven();
            for (int index = 0; index < lengthEven.Length / 2; ++index)
            {
                byte num = lengthEven[index * 2];
                lengthEven[index * 2] = lengthEven[(index * 2) + 1];
                lengthEven[(index * 2) + 1] = num;
            }
            return lengthEven;
        }

        /// <summary>
        /// 从Byte数组中提取位数组，length代表位数<br />
        /// </summary>
        /// <param name="InBytes">原先的字节数组</param>
        /// <param name="length">想要转换的长度，如果超出自动会缩小到数组最大长度</param>
        /// <returns>转换后的bool数组</returns>
        public static bool[] ByteToBoolArray(this byte[] InBytes, int length)
        {
            if (InBytes == null)
                return null;
            if (length > InBytes.Length * 8)
                length = InBytes.Length * 8;
            bool[] boolArray = new bool[length];
            for (int index = 0; index < length; ++index)
                boolArray[index] = InBytes[index / 8].BoolOnByteIndex(index % 8);
            return boolArray;
        }

        /// <summary>
        /// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位 <br />
        /// </summary>
        /// <param name="bytes">字节数组信息</param>
        /// <param name="boolIndex">指定字节的位偏移</param>
        /// <returns>bool值</returns>
        public static bool GetBoolByIndex(this byte[] bytes, int boolIndex)
        {
            return bytes[boolIndex / 8].BoolOnByteIndex(boolIndex % 8);
        }

        /// <summary>
        /// 字节数组默认转16进制字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] buffer, string splite = default)
        {
            if (string.IsNullOrEmpty(splite))
            {
                return BitConverter.ToString(buffer, 0, buffer.Length).Replace("-", string.Empty);
            }
            else
            {
                return BitConverter.ToString(buffer, 0, buffer.Length).Replace("-", splite);
            }
        }

        /// <summary>
        /// 字节数组转16进制字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] buffer, int offset, int length, string splite = default)
        {
            if (string.IsNullOrEmpty(splite))
            {
                return BitConverter.ToString(buffer, offset, length).Replace("-", string.Empty);
            }
            else
            {
                return BitConverter.ToString(buffer, offset, length).Replace("-", splite);
            }
        }
    }
}