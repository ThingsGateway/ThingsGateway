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

using System.Data;
using System.Text.Json;

namespace ThingsGateway.DataEncryption;

/// <summary>
/// KSort 加密（数据签名）
/// </summary>
[SuppressSniffer]
public class KSortEncryption
{
    private static DateTime _timeStampStartTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// 数据加密（签名）
    /// </summary>
    /// <param name="appId">APP_ID</param>
    /// <param name="appKey">APP_KEY</param>
    /// <param name="command">命令</param>
    /// <param name="data">序列化后的字符串</param>
    /// <param name="timestamp">时间戳</param>
    /// <returns><see cref="object"/></returns>
    public static KSortSignature Encrypt(string appId, string appKey, string command, string data, long? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(appId);
        ArgumentNullException.ThrowIfNull(appKey);
        ArgumentNullException.ThrowIfNull(command);

        timestamp ??= (long)(DateTime.Now.ToUniversalTime() - _timeStampStartTime).TotalMilliseconds;

        var dic = new Dictionary<string, object>
        {
            {"app_id", appId },
            {"app_key", appKey },
            {"command", command },
            {"data", data },
            {"timestamp", timestamp },
         };

        // ksort 排序
        var sortedDic = dic.OrderBy(kvp => kvp.Key);

        // 半角逗号连接
        var output = string.Join(",", sortedDic.Select(kvp => $"{kvp.Key}={kvp.Value}"));

        // utf8 编码，32位小写
        var signature = MD5Encryption.Encrypt(output);

        return new KSortSignature
        {
            app_id = appId,
            app_key = appKey,
            command = command,
            data = data,
            timestamp = timestamp.Value,
            signature = signature
        };
    }

    /// <summary>
    /// 比较数据签名
    /// </summary>
    /// <param name="kSortSignature"></param>
    /// <returns></returns>
    public static bool Compare(KSortSignature kSortSignature)
    {
        ArgumentNullException.ThrowIfNull(kSortSignature);

        return Encrypt(kSortSignature.app_id, kSortSignature.app_key, kSortSignature.command, kSortSignature.data, kSortSignature.timestamp) == kSortSignature;
    }

    /// <summary>
    /// 比较数据签名
    /// </summary>
    /// <param name="signatureData">签名数据</param>
    /// <param name="appId">新的 APP_ID</param>
    /// <param name="appKey">新的 APP_KEY</param>
    /// <returns></returns>
    public static bool Compare(string signatureData, string appId = null, string appKey = null)
    {
        var kSortSignature = JsonSerializer.Deserialize<KSortSignature>(signatureData);
        ArgumentNullException.ThrowIfNull(kSortSignature);

        return Encrypt(appId ?? kSortSignature.app_id, appKey ?? kSortSignature.app_key, kSortSignature.command, kSortSignature.data, kSortSignature.timestamp) == kSortSignature;
    }
}

/// <summary>
/// KSort 签名类
/// </summary>
public class KSortSignature : IEquatable<KSortSignature>
{
    /// <summary>
    /// APP_ID
    /// </summary>
    public string app_id { get; set; }

    /// <summary>
    /// APP_KEY
    /// </summary>
    public string app_key { get; set; }

    /// <summary>
    /// 命令
    /// </summary>
    public string command { get; set; }

    /// <summary>
    /// 序列化的字符串
    /// </summary>
    public string data { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public long timestamp { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    public string signature { get; set; }

    /// <inheritdoc />
    public bool Equals(KSortSignature other)
    {
        if (other == null) return false;

        return app_id == other.app_id &&
               app_key == other.app_key &&
               command == other.command &&
               data == other.data &&
               timestamp == other.timestamp &&
               signature == other.signature;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as KSortSignature);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(app_id, app_key, command, data, timestamp, signature);
    }

    /// <inheritdoc />
    public static bool operator ==(KSortSignature lhs, KSortSignature rhs)
    {
        if (ReferenceEquals(lhs, rhs)) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    /// <inheritdoc />
    public static bool operator !=(KSortSignature lhs, KSortSignature rhs)
    {
        return !(lhs == rhs);
    }
}