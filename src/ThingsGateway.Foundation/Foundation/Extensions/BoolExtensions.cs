namespace ThingsGateway.Foundation.Extension
{
    /// <summary>
    /// bool扩展
    /// </summary>
    public static class BoolExtensions
    {
        /// <summary>
        /// 将bool数组转换到byte数组
        /// </summary>
        public static byte[] BoolArrayToByte(this bool[] array)
        {
            if (array == null)
                return null;
            byte[] numArray = new byte[array.Length % 8 == 0 ? array.Length / 8 : (array.Length / 8) + 1];
            for (int index = 0; index < array.Length; ++index)
            {
                if (array[index])
                    numArray[index / 8] += DataHelper.GetDataByBitIndex(index % 8);
            }
            return numArray;
        }
    }
}