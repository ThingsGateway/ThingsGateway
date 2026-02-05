using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public static class FileServerHelpers
{
    /// <summary>
    /// 传输限速
    /// </summary>
    public const long MaxSpeed = 1024 * 1024 * 10L;

    /// <summary>
    /// 客户端从服务器下载文件。
    /// </summary>
    public static async Task<bool> ClientPullFileFromService(IDmtpActor client, string path, string savePath)
    {

        Directory.CreateDirectory(savePath.AsFile().DirectoryName);

        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add(FileConst.FilePathKey, path);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = savePath,//客户端本地保存路径
            ResourcePath = path,//请求文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            Token = cts.Token,
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
#pragma warning disable CA2000 // 丢失范围之前释放对象
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client.Logger.Info($"请求文件：{fileOperator.ResourcePath}，进度：{(fileOperator.Progress * 100).ToString("F2")}%，速度：{(fileOperator.Speed() / 1024).ToString("F2")} KB/s");
        });
#pragma warning restore CA2000 // 丢失范围之前释放对象

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
        var result = await client.GetDmtpFileTransferActor().PullFileAsync(fileOperator).ConfigureAwait(false);

        if (result.IsSuccess)
            client.Logger.Info(result.ToString());
        else
            client.Logger.Warning(result.ToString());

        return result.IsSuccess;
    }



    /// <summary>
    /// 客户端上传文件到服务器。
    /// </summary>
    public static async Task ClientPushFileFromService(IDmtpActor client, string resourcePath, string serverPath)
    {
        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add(FileConst.FilePathKey, serverPath);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = serverPath,//服务器本地保存路径
            ResourcePath = resourcePath,//客户端本地即将上传文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            Token = cts.Token,
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
#pragma warning disable CA2000 // 丢失范围之前释放对象
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client.Logger.Info($"进度：{(fileOperator.Progress * 100).ToString("F2")}%，速度：{(fileOperator.Speed() / 1024).ToString("F2")} KB/s");
        });
#pragma warning restore CA2000 // 丢失范围之前释放对象

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PushFileAsync
        var result = await client.GetDmtpFileTransferActor().PushFileAsync(fileOperator).ConfigureAwait(false);

        client.Logger.Info(result.ToString());
    }

}