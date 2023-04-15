namespace ThingsGateway.Core;

public class ImportPreviewOutputBase
{
    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError { get; set; }
    public virtual int DataCount { get; }
    public string ErrorStr { get; set; }
}
public class ImportPreviewOutput<T> : ImportPreviewOutputBase where T : class
{
    public override int DataCount { get => Data == null ? 0 : Data.Count; }
    public IList<T> Data { get; set; }
}


