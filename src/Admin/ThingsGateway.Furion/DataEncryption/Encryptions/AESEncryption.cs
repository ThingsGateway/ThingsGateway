// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace ThingsGateway.DataEncryption;

/// <summary>
/// AES 加解密
/// </summary>
[SuppressSniffer]
public class AESEncryption
{
    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="text">加密文本</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <returns></returns>
    public static string Encrypt(string text, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
    {
        var bKey = Encoding.UTF8.GetBytes(skey);

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        // 如果是 ECB 模式，不需要 IV
        if (mode != CipherMode.ECB)
        {
            aesAlg.IV = iv ?? aesAlg.IV; // 如果未提供 IV，则使用随机生成的 IV
        }

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(text);
        }

        var encryptedContent = msEncrypt.ToArray();

        // 如果是 CBC 模式，将 IV 和密文拼接在一起
        if (mode != CipherMode.ECB)
        {
            var result = new byte[aesAlg.IV.Length + encryptedContent.Length];
            Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
            Buffer.BlockCopy(encryptedContent, 0, result, aesAlg.IV.Length, encryptedContent.Length);
            return Convert.ToBase64String(result);
        }

        // 如果是 ECB 模式，直接返回密文的 Base64 编码
        return Convert.ToBase64String(encryptedContent);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="hash">加密后字符串</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <returns></returns>
    public static string Decrypt(string hash, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
    {
        var fullCipher = Convert.FromBase64String(hash);

        var bKey = Encoding.UTF8.GetBytes(skey);

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        // 如果是 ECB 模式，不需要 IV
        if (mode != CipherMode.ECB)
        {
            var bVector = new byte[16];
            var cipher = new byte[fullCipher.Length - bVector.Length];

            Unsafe.CopyBlock(ref bVector[0], ref fullCipher[0], (uint)bVector.Length);
            Unsafe.CopyBlock(ref cipher[0], ref fullCipher[bVector.Length], (uint)(fullCipher.Length - bVector.Length));

            aesAlg.IV = iv ?? bVector;
            fullCipher = cipher;
        }

        using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        using var msDecrypt = new MemoryStream(fullCipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="bytes">源文件 字节数组</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <returns>加密后的字节数组</returns>
    public static byte[] Encrypt(byte[] bytes, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
    {
        // 确保密钥长度为 128 位、192 位或 256 位
        var bKey = new byte[32]; // 256 位密钥
        var keyBytes = Encoding.UTF8.GetBytes(skey);
        Array.Copy(keyBytes, bKey, Math.Min(keyBytes.Length, bKey.Length));

        // 如果是 ECB 模式，不需要 IV
        if (mode != CipherMode.ECB)
        {
            iv ??= GenerateRandomIV(); // 生成随机 IV
        }

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        if (mode != CipherMode.ECB)
        {
            aesAlg.IV = iv;
        }

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Write);

        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();

        // 如果是 CBC 模式，将 IV 和密文拼接在一起
        if (mode != CipherMode.ECB)
        {
            var result = new byte[aesAlg.IV.Length + memoryStream.ToArray().Length];
            Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
            Buffer.BlockCopy(memoryStream.ToArray(), 0, result, aesAlg.IV.Length, memoryStream.ToArray().Length);
            return result;
        }

        // 如果是 ECB 模式，直接返回密文
        return memoryStream.ToArray();
    }

    // 生成随机 IV
    private static byte[] GenerateRandomIV()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        return aes.IV;
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="bytes">加密后文件 字节数组</param>
    /// <param name="skey">密钥</param>
    /// <param name="iv">偏移量</param>
    /// <param name="mode">模式</param>
    /// <param name="padding">填充</param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] bytes, string skey, byte[] iv = null, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7)
    {
        // 确保密钥长度为 128 位、192 位或 256 位
        var bKey = new byte[32]; // 256 位密钥
        var keyBytes = Encoding.UTF8.GetBytes(skey);
        Array.Copy(keyBytes, bKey, Math.Min(keyBytes.Length, bKey.Length));

        // 如果是 ECB 模式，不需要 IV
        if (mode != CipherMode.ECB)
        {
            if (iv == null)
            {
                // 从密文中提取 IV
                iv = new byte[16];
                Array.Copy(bytes, iv, iv.Length);
                bytes = bytes.Skip(iv.Length).ToArray();
            }
        }

        using var aesAlg = Aes.Create();
        aesAlg.Key = bKey;
        aesAlg.Mode = mode;
        aesAlg.Padding = padding;

        if (mode != CipherMode.ECB)
        {
            aesAlg.IV = iv;
        }

        using var memoryStream = new MemoryStream(bytes);
        using var cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Read);
        using var originalStream = new MemoryStream();

        var buffer = new byte[1024];
        var readBytes = 0;

        while ((readBytes = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            originalStream.Write(buffer, 0, readBytes);
        }

        return originalStream.ToArray();
    }
}