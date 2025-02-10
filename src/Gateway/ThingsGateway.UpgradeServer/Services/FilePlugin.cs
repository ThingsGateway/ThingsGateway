using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;

namespace ThingsGateway.Upgrade;

internal sealed class FilePlugin : PluginBase, IDmtpFileTransferringPlugin, IDmtpFileTransferredPlugin, IDmtpRoutingPlugin
{
    private readonly ILog m_logger;

    public FilePlugin(ILog logger)
    {
        m_logger = logger;
    }

    /// <summary>
    /// 该方法，会在每个文件被请求（推送）结束时触发。传输不一定成功，具体信息需要从e.Result判断状态。
    /// 其次，该方法也不一定会被执行，例如：在传输过程中，直接断网，则该方法将不会执行。
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public async Task OnDmtpFileTransferred(IDmtpActorObject client, FileTransferredEventArgs e)
    {
        //传输结束，但是不一定成功，甚至该方法都不一定会被触发，具体信息需要从e.Result判断状态。
        if (e.TransferType.IsPull())
        {
            m_logger.Info($"结束Pull文件，类型={e.TransferType}，文件名={e.ResourcePath}，结果={e.Result}");
        }
        else
        {
            m_logger.Info($"结束Push文件，类型={e.TransferType}，文件名={e.FileInfo.Name}，结果={e.Result}");
        }
        await e.InvokeNext().ConfigureAwait(false);
    }

    /// <summary>
    /// 该方法，会在每个文件被请求（推送）时第一时间触发。
    /// 当请求文件时，可以重新指定请求的文件路径，即对e.ResourcePath直接赋值。
    /// 当推送文件时，可以重新指定保存文件路径，即对e.SavePath直接赋值。
    ///
    /// 注意：当文件夹不存在时，需要手动创建。
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public async Task OnDmtpFileTransferring(IDmtpActorObject client, FileTransferringEventArgs e)
    {
        if (e.Metadata.TryGetValue(FileConst.FilePathKey, out var path))//获取元数据中的Path值
            e.IsPermitOperation = true;//每次传输都需要设置true，表示允许传输

        if (e.TransferType.IsPull())
        {
            e.ResourcePath = Path.Combine(AppContext.BaseDirectory, path);//重新指定请求的文件路径
            m_logger.Info($"请求Pull文件，类型={e.TransferType}，文件名={e.ResourcePath}");
            Directory.CreateDirectory(e.ResourcePath.AsFile().DirectoryName);
        }
        else
        {
            m_logger.Info($"请求Push文件，类型={e.TransferType}，文件名={e.FileInfo.Name}");
            e.SavePath = Path.Combine(AppContext.BaseDirectory, path);//重新指定请求的文件路径
            Directory.CreateDirectory(e.SavePath.AsFile().DirectoryName);
        }
        await e.InvokeNext().ConfigureAwait(false);
    }

    public async Task OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
    {
        e.IsPermitOperation = true;//允许路由
        m_logger.Info($"路由类型：{e.RouterType}");
        await e.InvokeNext().ConfigureAwait(false);
    }
}
