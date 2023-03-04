/// <summary>
/// 服务扩展
/// </summary>
public static class SevicesExtension
{
    /// <summary>
    /// 添加linux服务支持
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureLinuxService(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSystemd();

        return hostBuilder;
    }

    /// <summary>
    /// 添加windows服务支持
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureWindowsService(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseWindowsService();

        return hostBuilder;
    }


}