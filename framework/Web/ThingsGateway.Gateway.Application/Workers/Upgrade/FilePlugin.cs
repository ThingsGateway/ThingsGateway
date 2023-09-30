#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Text.RegularExpressions;

using ThingsGateway.Foundation.Dmtp;
using ThingsGateway.Foundation.Dmtp.FileTransfer;

namespace ThingsGateway.Gateway.Application;

internal class FilePlugin : PluginBase, IDmtpFileTransferingPlugin, IDmtpFileTransferedPlugin, IDmtpRoutingPlugin
{

    private readonly ILog m_logger;
    public FilePlugin(ILog logger)
    {
        this.m_logger = logger;
    }

    /// <summary>
    /// 该方法，会在每个文件被请求（推送）结束时触发。传输不一定成功，具体信息需要从e.Result判断状态。
    /// 其次，该方法也不一定会被执行，例如：在传输过程中，直接断网，则该方法将不会执行。
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public async Task OnDmtpFileTransfered(IDmtpActorObject client, FileTransferedEventArgs e)
    {
        //传输结束，但是不一定成功，甚至该方法都不一定会被触发，具体信息需要从e.Result判断状态。
        if (e.TransferType.IsPull())
        {
            this.m_logger.Trace($"结束Pull文件，类型={e.TransferType}，文件名={e.ResourcePath}，结果={e.Result}");
        }
        else
        {
            this.m_logger.Trace($"结束Push文件，类型={e.TransferType}，文件名={e.ResourcePath}，结果={e.Result}");
        }
        await e.InvokeNext();
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
    public async Task OnDmtpFileTransfering(IDmtpActorObject client, FileTransferingEventArgs e)
    {
        //有可能是上传，也有可能是下载

        if (e.TransferType.IsPull())
        {
            if (e.Metadata.TryGetValue(FilePluginUtil.DmtpType, out var dmtpTypeEnum))
            {
                if (Enum.TryParse<DmtpTypeEnum>(dmtpTypeEnum, out var result))
                {
                    switch (result)
                    {
                        case DmtpTypeEnum.File:




                            break;
                        case DmtpTypeEnum.GatewayDB:
                            var config = DbContext.DbConfigs.FirstOrDefault(a => a.ConfigId == SqlSugarConst.DB_Custom);
                            if (config != null && config.DbType == DbType.Sqlite)
                            {
                                // 使用正则表达式匹配并提取数据库名称
                                string pattern = @"Data Source=(\w+\.db)";
                                Match match = Regex.Match(config.ConnectionString, pattern);
                                if (match.Success)
                                {
                                    string databaseName = match.Groups[1].Value;
                                    e.ResourcePath = databaseName;
                                    e.IsPermitOperation = true;//每次传输都需要设置true，表示允许传输
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    this.m_logger.Trace($"请求Pull文件，类型={e.TransferType}，文件名={e.ResourcePath}");//拉取文件

                }
            }
        }
        else
        {

            if (e.Metadata.TryGetValue(FilePluginUtil.DmtpType, out var dmtpTypeEnum))
            {
                if (e.Metadata.TryGetValue(FilePluginUtil.FileName, out var fileName))
                    if (Enum.TryParse<DmtpTypeEnum>(dmtpTypeEnum, out var result))
                    {
                        switch (result)
                        {
                            case DmtpTypeEnum.File:

                                e.SavePath = $"{FilePluginUtil.GetFileTempPath((TcpDmtpClient)client)}/{fileName}";
                                var dir = Path.GetDirectoryName(e.SavePath);
                                if (!fileName.Contains("FileTemp"))
                                {
                                    // 检查文件夹是否存在
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }

                                    e.IsPermitOperation = true;//每次传输都需要设置true，表示允许传输

                                }

                                break;
                        }
                    }
            }

            this.m_logger.Trace($"请求Push文件，类型={e.TransferType}，文件名={e.SavePath}");//推送文件

        }
        await e.InvokeNext();
    }

    public async Task OnDmtpRouting(IDmtpActorObject client, PackageRouterEventArgs e)
    {
        e.IsPermitOperation = true;//允许路由
        await e.InvokeNext();
    }
}






