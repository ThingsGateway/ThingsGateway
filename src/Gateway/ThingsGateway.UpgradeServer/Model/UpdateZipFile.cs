using System.Runtime.InteropServices;

namespace ThingsGateway.Upgrade;

public class UpdateZipFile
{
    /// <summary>
    /// APP名称
    /// </summary>
    [AutoGenerateColumn(Sortable = true, Filterable = true)]
    public string AppName { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [AutoGenerateColumn(Sortable = true, Filterable = true)]
    public Version Version { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [AutoGenerateColumn(Sortable = true, Filterable = true)]
    public long FileSize { get; set; }

    /// <summary>
    /// 最小兼容版本
    /// </summary>
    [AutoGenerateColumn(Sortable = true, Filterable = true)]
    public Version MinimumCompatibleVersion { get; set; }

    /// <summary>
    /// .net版本
    /// </summary>
    [AutoGenerateColumn(Sortable = true, Filterable = true)]
    public Version DotNetVersion { get; set; }

    /// <summary>
    /// 系统版本
    /// </summary>
    [AutoGenerateColumn(Sortable = true, Filterable = true)]
    public string OSPlatform { get; set; }

    [AutoGenerateColumn(Sortable = true, Filterable = true)]
    public Architecture Architecture { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public string FilePath { get; set; }
}
