using Magicodes.ExporterAndImporter.Core.Models;

namespace ThingsGateway.Core;

public class ImportPreviewOutputBase
{
    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError { get; set; }
    public virtual int DataCount { get; }
    public IList<DataRowErrorInfo> RowErrors { get; set; }
}
/// <summary>
/// 文件导入通用输出
/// </summary>
public class ImportPreviewOutput<T> : ImportPreviewOutputBase where T : class
{
    /// <summary>
    /// 数据
    /// </summary>
    public List<T> Data { get; set; }
    public override int DataCount { get => Data == null ? 0 : Data.Count; }

}

