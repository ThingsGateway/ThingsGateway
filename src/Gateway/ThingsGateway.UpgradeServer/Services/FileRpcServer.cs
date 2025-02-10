using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace ThingsGateway.Upgrade;

public partial class FileRpcServer : RpcServer
{
    private readonly ILog _logger;

    public FileRpcServer(ILog logger)
    {
        _logger = logger;

    }

    [DmtpRpc(MethodInvoke = true)]
    public List<UpdateZipFile> GetList(UpdateZipFileInput input)
    {
        return App.GetService<IUpdateZipFileService>().GetList(input);
    }

}