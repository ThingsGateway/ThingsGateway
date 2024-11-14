
using ThingsGateway.AutoUpdate;

namespace ThingsGateway.Gateway.Management;

public interface IUpdateZipFileService
{
    TextFileLogger TextLogger { get; }
    string LogPath { get; }

    Task<List<UpdateZipFile>> GetList();
    Task Update(UpdateZipFile updateZipFile, Func<Task<bool>> check = null);
}