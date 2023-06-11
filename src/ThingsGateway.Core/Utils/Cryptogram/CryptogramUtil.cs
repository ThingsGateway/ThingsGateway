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