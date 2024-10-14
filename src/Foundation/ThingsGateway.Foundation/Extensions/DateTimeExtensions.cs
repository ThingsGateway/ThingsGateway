// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------
namespace ThingsGateway.Foundation;

/// <summary>
/// 时间扩展类
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// DateTime转Unix时间戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long DateTimeToUnixTimestamp(this DateTime dateTime)
    {
        // Unix 时间起点
        var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // 计算时间差
        var timeSpan = dateTime.ToUniversalTime() - unixStart;
        // 返回毫秒数
        return (long)timeSpan.TotalMilliseconds;
    }
}
