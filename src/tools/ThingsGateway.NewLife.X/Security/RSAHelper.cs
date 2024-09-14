﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Security.Cryptography;

namespace ThingsGateway.NewLife.X.Security;

/// <summary>RSA算法</summary>
/// <remarks>
/// RSA加密或签名小数据块时，密文长度128，速度也很快。
/// </remarks>
public static class RSAHelper
{
    #region 加密解密

    /// <summary>创建RSA对象，支持Xml密钥和Pem密钥</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static RSACryptoServiceProvider Create(String key)
    {
        key = key.Trim();
        if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));

        var rsa = new RSACryptoServiceProvider();
        if (key.StartsWith("<RSAKeyValue>") && key.EndsWith("</RSAKeyValue>"))
            rsa.FromXmlString(key);
        else if (key.StartsWith("--") || key.Contains('\r') || key.Contains('\n'))
            rsa.ImportParameters(ReadPem(key));
        else
            rsa.ImportParameters(ReadParameters(key));

        return rsa;
    }

    /// <summary>RSA私钥解密。仅用于加密少量数据</summary>
    /// <remarks>
    /// (PKCS # 1 v2) 的 OAEP 填充	模数大小-2-2 * hLen，其中 hLen 是哈希的大小。
    /// 直接加密 (PKCS # 1 1.5 版)	模数大小-11。 (11 个字节是可能的最小填充。 )
    /// </remarks>
    /// <param name="data">数据密文</param>
    /// <param name="priKey">私钥</param>
    /// <param name="fOAEP">如果为 true，则使用 OAEP 填充（仅可用于运行 Microsoft Windows XP 及更高版本的计算机）执行直接 System.Security.Cryptography.RSA解密；否则，如果为 false 则使用 PKCS#1 v1.5 填充。</param>
    /// <returns></returns>
    public static Byte[] Decrypt(Byte[] data, String priKey, Boolean fOAEP = true)
    {
        var rsa = Create(priKey);

        return rsa.Decrypt(data, fOAEP);
    }

    /// <summary>RSA公钥加密。仅用于加密少量数据</summary>
    /// <remarks>
    /// (PKCS # 1 v2) 的 OAEP 填充	模数大小-2-2 * hLen，其中 hLen 是哈希的大小。
    /// 直接加密 (PKCS # 1 1.5 版)	模数大小-11。 (11 个字节是可能的最小填充。 )
    /// </remarks>
    /// <param name="data">数据明文</param>
    /// <param name="pubKey">公钥</param>
    /// <param name="fOAEP">如果为 true，则使用 OAEP 填充（仅可用于运行 Windows XP 及更高版本的计算机）执行直接 System.Security.Cryptography.RSA加密；否则，如果为 false，则使用 PKCS#1 v1.5 填充。</param>
    /// <returns></returns>
    public static Byte[] Encrypt(Byte[] data, String pubKey, Boolean fOAEP = true)
    {
        var rsa = Create(pubKey);

        return rsa.Encrypt(data, fOAEP);
    }

    /// <summary>产生非对称密钥对</summary>
    /// <remarks>
    /// RSAParameters的各个字段采用大端字节序，转为BigInteger的之前一定要倒序。
    /// RSA加密后密文最小长度就是密钥长度，所以1024密钥最小密文长度是128字节。
    /// </remarks>
    /// <param name="keySize">密钥长度，默认1024位强密钥</param>
    /// <returns></returns>
    public static String[] GenerateKey(Int32 keySize = 2048)
    {
        var rsa = new RSACryptoServiceProvider(keySize);

        var ss = new String[2];
        ss[0] = rsa.ToXmlString(true);
        ss[1] = rsa.ToXmlString(false);

        return ss;
    }

    /// <summary>产生非对称密钥对</summary>
    /// <remarks>
    /// RSAParameters的各个字段采用大端字节序，转为BigInteger的之前一定要倒序。
    /// RSA加密后密文最小长度就是密钥长度，所以1024密钥最小密文长度是128字节。
    /// </remarks>
    /// <param name="keySize">密钥长度，默认1024位强密钥</param>
    /// <returns></returns>
    public static String[] GenerateParameters(Int32 keySize = 2048)
    {
        var rsa = new RSACryptoServiceProvider(keySize);

        var ss = new String[2];
        ss[0] = WriteParameters(rsa.ExportParameters(true));
        ss[1] = WriteParameters(rsa.ExportParameters(false));

        return ss;
    }

    /// <summary>根据Base64密钥创建RSA参数</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static RSAParameters ReadParameters(String key)
    {
        using var ms = new MemoryStream(key.ToBase64());

        var p = new RSAParameters
        {
            Modulus = ms.ReadArray(),
            Exponent = ms.ReadArray(),
        };

        if (ms.Position < ms.Length)
        {
            p.D = ms.ReadArray();
            p.P = ms.ReadArray();
            p.Q = ms.ReadArray();
            p.DP = ms.ReadArray();
            p.DQ = ms.ReadArray();
            p.InverseQ = ms.ReadArray();
        }

        return p;
    }

    /// <summary>RSA参数转为Base64密钥</summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static String WriteParameters(RSAParameters p)
    {
        // 判断参数p的每一个成员是否为空，如果为空则抛出异常
        if (p.Modulus == null || p.Exponent == null) throw new ArgumentNullException(nameof(p));
        var ms = new MemoryStream();
        ms.WriteArray(p.Modulus);
        ms.WriteArray(p.Exponent);

        if (p.D != null && p.D.Length > 0)
        {
            if (p.D == null || p.P == null || p.Q == null ||
                p.DP == null || p.DQ == null || p.InverseQ == null) throw new ArgumentNullException(nameof(p));

            ms.WriteArray(p.D);
            ms.WriteArray(p.P);
            ms.WriteArray(p.Q);
            ms.WriteArray(p.DP);
            ms.WriteArray(p.DQ);
            ms.WriteArray(p.InverseQ);
        }

        return ms.ToArray().ToUrlBase64();
    }

    #endregion 加密解密

    #region 数字签名

    private static HashAlgorithm _sha256 = SHA256.Create();

    private static HashAlgorithm _sha384 = SHA384.Create();

    private static HashAlgorithm _sha512 = SHA512.Create();

    /// <summary>签名，MD5散列</summary>
    /// <param name="data"></param>
    /// <param name="priKey"></param>
    /// <returns></returns>
    public static Byte[] Sign(Byte[] data, String priKey)
    {
        var rsa = Create(priKey);

        return rsa.SignData(data, MD5.Create());
    }

    /// <summary>RS256</summary>
    /// <param name="data"></param>
    /// <param name="priKey"></param>
    /// <returns></returns>
    public static Byte[] SignSha256(this Byte[] data, String priKey)
    {
        var rsa = Create(priKey);
        return rsa.SignData(data, _sha256);
    }

    /// <summary>RS384</summary>
    /// <param name="data"></param>
    /// <param name="priKey"></param>
    /// <returns></returns>
    public static Byte[] SignSha384(this Byte[] data, String priKey)
    {
        var rsa = Create(priKey);
        return rsa.SignData(data, _sha384);
    }

    /// <summary>RS512</summary>
    /// <param name="data"></param>
    /// <param name="priKey"></param>
    /// <returns></returns>
    public static Byte[] SignSha512(this Byte[] data, String priKey)
    {
        var rsa = Create(priKey);
        return rsa.SignData(data, _sha512);
    }

    /// <summary>验证，MD5散列</summary>
    /// <param name="data"></param>
    /// <param name="pukKey"></param>
    /// <param name="rgbSignature"></param>
    /// <returns></returns>
    public static Boolean Verify(Byte[] data, String pukKey, Byte[] rgbSignature)
    {
        var rsa = Create(pukKey);

        return rsa.VerifyData(data, MD5.Create(), rgbSignature);
    }

    /// <summary>RS256</summary>
    /// <param name="data"></param>
    /// <param name="pukKey"></param>
    /// <param name="rgbSignature"></param>
    /// <returns></returns>
    public static Boolean VerifySha256(this Byte[] data, String pukKey, Byte[] rgbSignature)
    {
        var rsa = Create(pukKey);
        return rsa.VerifyData(data, _sha256, rgbSignature);
    }

    /// <summary>RS384</summary>
    /// <param name="data"></param>
    /// <param name="pukKey"></param>
    /// <param name="rgbSignature"></param>
    /// <returns></returns>
    public static Boolean VerifySha384(this Byte[] data, String pukKey, Byte[] rgbSignature)
    {
        var rsa = Create(pukKey);
        return rsa.VerifyData(data, _sha384, rgbSignature);
    }

    /// <summary>RS512</summary>
    /// <param name="data"></param>
    /// <param name="pukKey"></param>
    /// <param name="rgbSignature"></param>
    /// <returns></returns>
    public static Boolean VerifySha512(this Byte[] data, String pukKey, Byte[] rgbSignature)
    {
        var rsa = Create(pukKey);
        return rsa.VerifyData(data, _sha512, rgbSignature);
    }

    #endregion 数字签名

    #region PEM

    /// <summary>读取PEM文件到RSA参数</summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static RSAParameters ReadPem(String content)
    {
        if (String.IsNullOrEmpty(content)) throw new ArgumentNullException(nameof(content));

        // 公钥私钥分别处理
        content = content.Trim();
        if (content.StartsWithIgnoreCase("-----BEGIN RSA PRIVATE KEY-----", "-----BEGIN PRIVATE KEY-----"))
        {
            var content2 = content.TrimStart("-----BEGIN RSA PRIVATE KEY-----")
                 .TrimEnd("-----END RSA PRIVATE KEY-----")
                 .TrimStart("-----BEGIN PRIVATE KEY-----")
                 .TrimEnd("-----END PRIVATE KEY-----")
                 .Replace("\n", null).Replace("\r", null);

            var data = Convert.FromBase64String(content2);

            // PrivateKeyInfo: version + Algorithm(algorithm + parameters) + privateKey
            var asn = Asn1.Read(data) ?? throw new InvalidDataException();
            var keys = asn.Value as Asn1[] ?? throw new InvalidDataException();

            // 可能直接key，也可能有Oid包装
            var oids = asn.GetOids();
            if (oids.Any(e => e.FriendlyName == "RSA"))
            {
                var buf = keys[2].Value as Byte[];
                if (buf != null) keys = Asn1.Read(buf)?.Value as Asn1[];
            }

            if (keys == null) throw new InvalidDataException();

            // 参数数据
            return new RSAParameters
            {
                Modulus = keys[1].GetByteArray(true),
                Exponent = keys[2].GetByteArray(false),
                D = keys[3].GetByteArray(true),
                P = keys[4].GetByteArray(true),
                Q = keys[5].GetByteArray(true),
                DP = keys[6].GetByteArray(true),
                DQ = keys[7].GetByteArray(true),
                InverseQ = keys[8].GetByteArray(true)
            };
        }
        else
        {
            content = content.Replace("-----BEGIN PUBLIC KEY-----", null)
                .Replace("-----END PUBLIC KEY-----", null)
                .Replace("\n", null).Replace("\r", null);

            var data = Convert.FromBase64String(content);

            var asn = Asn1.Read(data) ?? throw new InvalidDataException();
            var keys = asn.Value as Asn1[] ?? throw new InvalidDataException();

            // 可能直接key，也可能有Oid包装
            var oids = asn.GetOids();
            if (oids.Any(e => e.FriendlyName == "RSA"))
            {
                var buf = keys.FirstOrDefault(e => e.Tag == Asn1Tags.BitString)?.Value as Byte[];
                if (buf != null) keys = Asn1.Read(buf)?.Value as Asn1[];
            }

            if (keys == null) throw new InvalidDataException();

            // 参数数据
            return new RSAParameters
            {
                Modulus = keys[0].GetByteArray(true),
                Exponent = keys[1].GetByteArray(false),
            };
        }
    }

    #endregion PEM
}
