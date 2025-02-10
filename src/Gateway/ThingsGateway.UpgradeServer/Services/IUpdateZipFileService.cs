




namespace ThingsGateway.Upgrade;

public interface IUpdateZipFileService
{
    Task<bool> DeleteAsync(IEnumerable<UpdateZipFile> updateZipFiles);
    List<UpdateZipFile>? GetList(UpdateZipFileInput input);
    Task<QueryData<UpdateZipFile>>? Page(QueryPageOptions options);
    Task SaveUpdateZipFile(UpdateZipFileAddInput input);
    Task SaveUpdateZipFile(UpdateZipFileAddInput1 input);
}