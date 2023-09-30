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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// <see cref="decimal"/>与字节数组转换
    /// </summary>
    public static class DecimalConver
    {
        /// <summary>
        /// 将<see cref="decimal"/>对象转换为固定字节长度（16）数组。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToBytes(decimal value)
        {
            var bits = decimal.GetBits(value);
            var bytes = new byte[bits.Length * 4];
            for (var i = 0; i < bits.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    bytes[i * 4 + j] = (byte)(bits[i] >> (j * 8));
                }
            }
            return bytes;
        }
        /// <summary>
        /// 将固定字节长度（16）数组转换为<see cref="decimal"/>对象。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static decimal FromBytes(byte[] array)
        {
            var bits = new int[array.Length / 4];
            for (var i = 0; i < bits.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    bits[i] |= array[i * 4 + j] << j * 8;
                }
            }
            return new decimal(bits);
        }
    }
}
