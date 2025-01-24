using System.Security.Cryptography;


namespace ThingsGateway.NewLife.Security;

/// <summary>数据保护者。保护连接字符串中的密码</summary>
public class ProtectedKey
{
    #region 属性
    /// <summary>保护数据的密钥</summary>
    public Byte[]? Secret { get; set; }

    /// <summary>算法。默认AES</summary>
    public String Algorithm { get; set; } = "AES";

    /// <summary>隐藏字符串</summary>
    public String HideString { get; set; } = "{***}";

    /// <summary>密码名字</summary>
    public String[] Names { get; set; } = ["password", "pass", "pwd"];
    #endregion

    #region 方法
    /// <summary>保护连接字符串中的密码</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public String Protect(String value)
    {
        using var alg = Create(Algorithm);

        // 单纯待加密数据
        var p = value.IndexOf('=');
        if (p < 0)
        {
            var pass = alg.Encrypt(value.GetBytes(), Secret).ToUrlBase64();
            return $"${Algorithm}${pass}";
        }

        // 查找密码片段
        var dic = value.SplitAsDictionary("=", ";", true);
        foreach (var item in Names)
        {
            if (dic.TryGetValue(item, out var pass))
            {
                if (pass.IsNullOrEmpty()) break;

                // 加密密码后，重新组装
                pass = alg.Encrypt(pass.GetBytes(), Secret).ToUrlBase64();
                dic[item] = $"${Algorithm}${pass}";

                return dic.Join(";", e => $"{e.Key}={e.Value}");
            }
        }

        return value;
    }

    /// <summary>解保护连接字符串中的密码</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public String Unprotect(String value)
    {
        // 单纯待加密数据
        var p = value.IndexOf('=');
        if (p < 0)
        {
            // 分解加密算法，$AES$string
            var ss = value.Split('$');
            if (ss == null || ss.Length < 3) return value;

            using var alg = Create(ss[1]);

            return alg.Decrypt(ss[2].ToBase64(), Secret).ToStr();
        }

        // 查找密码片段
        var dic = value.SplitAsDictionary("=", ";");
        foreach (var item in Names)
        {
            if (dic.TryGetValue(item, out var pass))
            {
                if (pass.IsNullOrEmpty()) break;

                // 分解加密算法，$AES$string
                var ss = pass.Split('$');
                if (ss == null || ss.Length < 3) continue;

                using var alg = Create(ss[1]);

                dic[item] = alg.Decrypt(ss[2].ToBase64(), Secret).ToStr();

                return dic.Join(";", e => $"{e.Key}={e.Value}");
            }
        }

        return value;
    }

    /// <summary>隐藏连接字符串中的密码</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public String Hide(String value)
    {
        var dic = value.SplitAsDictionary("=", ";");
        foreach (var item in Names)
        {
            if (dic.TryGetValue(item, out var pass))
            {
                dic[item] = HideString;

                return dic.Join(";", e => $"{e.Key}={e.Value}");
            }
        }

        return value;
    }

    private static SymmetricAlgorithm Create(String name)
    {
        return name.ToLowerInvariant() switch
        {
            "aes" => Aes.Create(),
            "des" => DES.Create(),
            "rc2" => RC2.Create(),
            "tripledes" => TripleDES.Create(),
            _ => throw new NotSupportedException($"Not Supported [{name}]"),
        };
    }
    #endregion
}
