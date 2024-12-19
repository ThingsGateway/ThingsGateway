////------------------------------------------------------------------------------
////  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
////  此代码版权（除特别声明外的代码）归作者本人Diego所有
////  源代码使用协议遵循本仓库的开源协议及附加协议
////  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
////  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
////  使用文档：https://thingsgateway.cn/
////  QQ群：605534569
////------------------------------------------------------------------------------

//using Org.BouncyCastle.Crypto;

//using System.Security.Cryptography;
//using System.Text;

//namespace ThingsGateway.Server;

///// <summary>
///// 解密数据库连接字符串demo，需注入服务
///// </summary>
//internal sealed class SugarConfigAopService : ISugarConfigAopService
//{
//    public SqlSugarOptions Config(SqlSugarOptions sqlSugarOptions)
//    {
//        foreach (var item in sqlSugarOptions)
//        {
//            //解密连接字符串,自定 算法方式，例如RSA
//            RSA rsa = RSA.Create();

//            string publicKey = "demo"; // 公钥
//            string privateKey = "demo"; // 私钥
//            rsa.FromXmlString(publicKey);
//            rsa.FromXmlString(privateKey);

//            var encryptedText = item.ConnectionString;
//            byte[] encryptedData = Convert.FromBase64String(encryptedText);
//            byte[] decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);

//            item.ConnectionString = Encoding.UTF8.GetString(decryptedData);

//        }
//        return sqlSugarOptions;
//    }
//}
