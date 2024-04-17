
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




// 版权归百小僧及百签科技（广东）有限公司所有。
//
using NewLife.Extension;

using System.Security.Cryptography;
using System.Text;

namespace ThingsGateway.Core;

/// <summary>
/// DESC 加解密
/// </summary>
public class DESCEncryption
{
    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="text">加密文本</param>
    /// <param name="skey">密钥</param>
    /// <param name="uppercase">是否输出大写加密，默认 false</param>
    /// <returns></returns>
    public static string Encrypt(string? text, string skey = "ThingsGateway", bool uppercase = false)
    {
        if (text.IsNullOrWhiteSpace()) return text;
        using var des = DES.Create();
        var inputByteArray = Encoding.Default.GetBytes(text);

        var md5Bytes = Encoding.ASCII.GetBytes(MD5Encryption.Encrypt(skey, uppercase)[..8]);
        des.Key = md5Bytes;
        des.IV = md5Bytes;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);

        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();

        var ret = new StringBuilder();
        foreach (var b in ms.ToArray())
        {
            ret.AppendFormat("{0:X2}", b);
        }

        return ret.ToString();
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="hash">加密后字符串</param>
    /// <param name="skey">密钥</param>
    /// <param name="uppercase">是否输出大写加密，默认 false</param>
    /// <returns></returns>
    public static string Decrypt(string? hash, string skey = "ThingsGateway", bool uppercase = false)
    {
        if (hash.IsNullOrWhiteSpace()) return hash;
        using var des = DES.Create();
        var len = hash.Length / 2;
        var inputByteArray = new byte[len];
        int x, i;

        for (x = 0; x < len; x++)
        {
            i = Convert.ToInt32(hash.Substring(x * 2, 2), 16);
            inputByteArray[x] = (byte)i;
        }

        var md5Bytes = Encoding.ASCII.GetBytes(MD5Encryption.Encrypt(skey, uppercase)[..8]);
        des.Key = md5Bytes;
        des.IV = md5Bytes;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);

        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();

        return Encoding.Default.GetString(ms.ToArray());
    }
}