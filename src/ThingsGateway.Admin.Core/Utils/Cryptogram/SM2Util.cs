//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Core.Utils;

/// <summary>
/// SM2加密解密
/// </summary>
public class SM2Util
{
    /// <summary>
    /// 公钥
    /// </summary>
    public static string PublicKey = App.GetConfig<string>("Cryptogram:SM2:PublicKey");

    /// <summary>
    /// 私钥
    /// </summary>
    public static string PrivateKey = App.GetConfig<string>("Cryptogram:SM2:PrivateKey");

    /// <summary>
    /// 公钥加密明文
    /// </summary>
    /// <param name="plainText">明文</param>
    /// <returns>密文</returns>
    public static string Encrypt(string plainText)
    {
        return SM2CryptoUtil.Encrypt(PublicKey, plainText);
    }

    /// <summary>
    /// 私钥解密密文
    /// </summary>
    /// <param name="cipherText">密文</param>
    /// <returns>明文</returns>
    public static string Decrypt(string cipherText)
    {
        if (!cipherText.StartsWith("04")) cipherText = "04" + cipherText;//如果不是04开头加上04
        return SM2CryptoUtil.Decrypt(PrivateKey, cipherText);
    }
}