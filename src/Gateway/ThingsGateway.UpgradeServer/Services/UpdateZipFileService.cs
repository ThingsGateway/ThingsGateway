using Microsoft.AspNetCore.Http;

using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Upgrade;

public class UpdateZipFileService : IUpdateZipFileService
{
    public List<UpdateZipFile>? GetList(UpdateZipFileInput input)
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir, input.AppName);

        var files = path.AsDirectory().GetAllFiles("*.json", true);
        var data = files.Select(a => Encoding.UTF8.GetString(a.ReadBytes()).FromJsonNetString<UpdateZipFile>());
        return data.Where(a => a.Version > input.Version && a.MinimumCompatibleVersion <= input.Version && a.AppName == input.AppName && a.DotNetVersion.Major == input.DotNetVersion.Major && a.OSPlatform.EqualIgnoreCase(input.OSPlatform) && a.Architecture == input.Architecture).ToList();
    }
    public async Task<QueryData<UpdateZipFile>>? Page(QueryPageOptions options)
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir);

        var files = path.AsDirectory().GetAllFiles("*.json", true);
        var data = files.Select(a => Encoding.UTF8.GetString(a.ReadBytes()).FromJsonNetString<UpdateZipFile>()).GetQueryData(options);

        await Task.CompletedTask.ConfigureAwait(false);
        return data;
    }
    public async Task SaveUpdateZipFile(UpdateZipFileAddInput input)
    {
        var jsonStream = input.JsonFile.OpenReadStream(1024 * 1024 * 5);
        MemoryStream jsonMemoryStream = new MemoryStream();
        try
        {
            await jsonStream.CopyToAsync(jsonMemoryStream).ConfigureAwait(false);
            var model = Encoding.UTF8.GetString(jsonMemoryStream.ToArray()).FromJsonNetString<UpdateZipFile>();
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir, model.AppName, model.Version.ToString(), model.DotNetVersion.ToString(), model.OSPlatform.ToString()));

            var path = Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir, model.AppName, model.Version.ToString(), model.DotNetVersion.ToString(), model.OSPlatform.ToString(), model.Architecture.ToString());
            if (path.AsDirectory().Exists)
            {

                path.AsDirectory().Delete(true);
                var stream = input.ZipFile.OpenReadStream(1024 * 1024 * 500);
                await jsonStream.DisposeAsync().ConfigureAwait(false);
                await Create(stream, model, path).ConfigureAwait(false);
                await stream.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                var stream = input.ZipFile.OpenReadStream(1024 * 1024 * 500);
                await jsonStream.DisposeAsync().ConfigureAwait(false);
                await Create(stream, model, path).ConfigureAwait(false);
                await stream.DisposeAsync().ConfigureAwait(false);
            }


        }
        finally
        {
            await jsonStream.DisposeAsync().ConfigureAwait(false);
            await jsonMemoryStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async Task SaveUpdateZipFile(UpdateZipFileAddInput1 input)
    {
        var jsonStream = input.JsonFile;
        MemoryStream jsonMemoryStream = new MemoryStream();
        try
        {
            await jsonStream.CopyToAsync(jsonMemoryStream).ConfigureAwait(false);
            var model = Encoding.UTF8.GetString(jsonMemoryStream.ToArray()).FromJsonNetString<UpdateZipFile>();
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir, model.AppName, model.Version.ToString(), model.DotNetVersion.ToString(), model.OSPlatform.ToString()));

            var path = Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir, model.AppName, model.Version.ToString(), model.DotNetVersion.ToString(), model.OSPlatform.ToString(), model.Architecture.ToString());
            if (path.AsDirectory().Exists)
            {
                path.AsDirectory().Delete(true);
                var stream = input.ZipFile;
                await Create(stream, model, path).ConfigureAwait(false);
            }
            else
            {
                var stream = input.ZipFile;
                await Create(stream, model, path).ConfigureAwait(false);
            }


        }
        finally
        {
            await jsonMemoryStream.DisposeAsync().ConfigureAwait(false);
        }
    }
    private static async Task Create(IFormFile stream, UpdateZipFile model, string path)
    {
        path.AsDirectory().Create();
        using var fs = new FileStream(Path.Combine(path, $"{model.AppName}.zip"), FileMode.Create);
        await stream.CopyToAsync(fs).ConfigureAwait(false);
        var relativePath = Path.GetRelativePath(AppContext.BaseDirectory, path);
        model.FilePath = Path.Combine(relativePath, $"{model.AppName}.zip");
        model.FileSize = fs.Length;
        // 将JSON字符串转换为字节数组
        byte[] byteArray = Encoding.UTF8.GetBytes(model.ToJsonString());
        using MemoryStream stream1 = new MemoryStream(byteArray);
        using var fs1 = new FileStream(Path.Combine(path, $"{model.AppName}.json"), FileMode.Create);
        await stream1.CopyToAsync(fs1).ConfigureAwait(false);
    }

    private static async Task Create(Stream stream, UpdateZipFile model, string path)
    {
        path.AsDirectory().Create();
        using var fs = new FileStream(Path.Combine(path, $"{model.AppName}.zip"), FileMode.Create);
        await stream.CopyToAsync(fs).ConfigureAwait(false);
        var relativePath = Path.GetRelativePath(AppContext.BaseDirectory, path);
        model.FilePath = Path.Combine(relativePath, $"{model.AppName}.zip");
        model.FileSize = fs.Length;
        // 将JSON字符串转换为字节数组
        byte[] byteArray = Encoding.UTF8.GetBytes(model.ToJsonString());
        using MemoryStream stream1 = new MemoryStream(byteArray);
        using var fs1 = new FileStream(Path.Combine(path, $"{model.AppName}.json"), FileMode.Create);
        await stream1.CopyToAsync(fs1).ConfigureAwait(false);
    }
    public async Task<bool> DeleteAsync(IEnumerable<UpdateZipFile> updateZipFiles)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        var path = Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir);
        foreach (var item in updateZipFiles)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, FileConst.ServerDir, item.AppName, item.Version.ToString(), item.DotNetVersion.ToString(), item.OSPlatform.ToString(), item.Architecture.ToString());
            if (filePath.AsDirectory().Exists)
            {
                filePath.AsDirectory().Delete(true);
            }
        }
        return true;
    }
}
