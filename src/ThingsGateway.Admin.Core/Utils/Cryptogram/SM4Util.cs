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

using Org.BouncyCastle.Utilities.Encoders;

using System.ComponentModel;
using System.Text;

namespace ThingsGateway.Admin.Core.Utils;

//加密和解密结构相同，只不过，解密密钥是加密密钥的逆序
/// <summary>
/// Sm4算法
/// 对标国际DES算法
/// </summary>
public class SM4Util
{
    public SM4Util()
    {
        Key = "1814546261730461";//密钥长度必须为16字节。
        Iv = "0000000000000000";
        HexString = false;
        CryptoMode = Sm4CryptoEnum.ECB;
    }

    /// <summary>
    /// 数据
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// 秘钥
    /// </summary>
    public string Key { get; }//不同的key，加密出来的数据不一样，所以此处设定好key以后，禁止修改

    /// <summary>
    /// 向量
    /// </summary>
    public string Iv { get; set; }

    /// <summary>
    /// 明文是否是十六进制
    /// </summary>
    public bool HexString { get; }//set;

    /// <summary>
    /// 加密模式(默认ECB)
    /// 统一改为ECB模式
    /// </summary>
    public Sm4CryptoEnum CryptoMode { get; }

    #region 加密

    public static string Encrypt(SM4Util entity)
    {
        return entity.CryptoMode == Sm4CryptoEnum.CBC ? EncryptCBC(entity) : EncryptECB(entity);
    }

    /// <summary>
    /// ECB加密
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static string EncryptECB(SM4Util entity)
    {
        Sm4Context ctx = new Sm4Context
        {
            IsPadding = true
        };
        byte[] keyBytes = entity.HexString ? Hex.Decode(entity.Key) : Encoding.Default.GetBytes(entity.Key);
        SM4CryptoUtil sm4 = new SM4CryptoUtil();
        sm4.SetKeyEnc(ctx, keyBytes);
        byte[] encrypted = sm4.Sm4CryptEcb(ctx, Encoding.Default.GetBytes(entity.Data));
        return Encoding.Default.GetString(Hex.Encode(encrypted));
    }

    /// <summary>
    /// CBC加密
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static string EncryptCBC(SM4Util entity)
    {
        Sm4Context ctx = new Sm4Context
        {
            IsPadding = true
        };
        byte[] keyBytes = entity.HexString ? Hex.Decode(entity.Key) : Encoding.Default.GetBytes(entity.Key);
        byte[] ivBytes = entity.HexString ? Hex.Decode(entity.Iv) : Encoding.Default.GetBytes(entity.Iv);
        SM4CryptoUtil sm4 = new SM4CryptoUtil();
        sm4.SetKeyEnc(ctx, keyBytes);
        byte[] encrypted = sm4.Sm4CryptCbc(ctx, ivBytes, Encoding.Default.GetBytes(entity.Data));
        return Convert.ToBase64String(encrypted);
    }

    #endregion 加密

    #region 解密

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static string Decrypt(SM4Util entity)
    {
        return entity.CryptoMode == Sm4CryptoEnum.CBC ? DecryptCBC(entity) : DecryptECB(entity);
    }

    /// <summary>
    ///  ECB解密
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static string DecryptECB(SM4Util entity)
    {
        Sm4Context ctx = new Sm4Context
        {
            IsPadding = true,
            Mode = 0
        };
        byte[] keyBytes = entity.HexString ? Hex.Decode(entity.Key) : Encoding.Default.GetBytes(entity.Key);
        SM4CryptoUtil sm4 = new SM4CryptoUtil();
        sm4.Sm4SetKeyDec(ctx, keyBytes);
        byte[] decrypted = sm4.Sm4CryptEcb(ctx, Hex.Decode(entity.Data));
        return Encoding.Default.GetString(decrypted);
    }

    /// <summary>
    /// CBC解密
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static string DecryptCBC(SM4Util entity)
    {
        Sm4Context ctx = new Sm4Context
        {
            IsPadding = true,
            Mode = 0
        };
        byte[] keyBytes = entity.HexString ? Hex.Decode(entity.Key) : Encoding.Default.GetBytes(entity.Key);
        byte[] ivBytes = entity.HexString ? Hex.Decode(entity.Iv) : Encoding.Default.GetBytes(entity.Iv);
        SM4CryptoUtil sm4 = new SM4CryptoUtil();
        sm4.Sm4SetKeyDec(ctx, keyBytes);
        byte[] decrypted = sm4.Sm4CryptCbc(ctx, ivBytes, Convert.FromBase64String(entity.Data));
        return Encoding.Default.GetString(decrypted);
    }

    #endregion 解密

    /// <summary>
    /// 加密类型
    /// </summary>
    public enum Sm4CryptoEnum
    {
        /// <summary>
        /// ECB(电码本模式)
        /// </summary>
        [Description("ECB模式")]
        ECB = 0,

        /// <summary>
        /// CBC(密码分组链接模式)
        /// </summary>
        [Description("CBC模式")]
        CBC = 1
    }
}