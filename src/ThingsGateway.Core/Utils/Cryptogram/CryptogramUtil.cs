namespace ThingsGateway.Core.Utils
{
    /// <summary>
    /// 加解密功能
    /// </summary>
    public class CryptogramUtil
    {
        #region Sm4

        /// <summary>
        /// SM4解密
        /// </summary>
        /// <param name="str">密文</param>
        /// <returns>明文</returns>
        public static string Sm4Decrypt(string str)
        {
            if (str != null)
                return SM4Util.Decrypt(new SM4Util { Data = str });// 解密
            else
                return null;
        }

        /// <summary>
        /// SM4加密
        /// </summary>
        /// <param name="str">明文</param>
        /// <returns>密文</returns>
        public static string Sm4Encrypt(string str)
        {
            if (str != null)
                return SM4Util.Encrypt(new SM4Util { Data = str });            // 加密
            else
                return null;
        }

        #endregion Sm4
    }
}