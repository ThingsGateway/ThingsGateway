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

public partial class MemoryChannelRuntime : ChannelRuntime
{
    public override void Init()
    {
        // 通过插件名称获取插件信息
        PluginInfo = GlobalData.PluginService.GetPluginList().FirstOrDefault(A => A.FullName == PluginName);

        //GlobalData.IdChannels.TryRemove(Id, out _);
        //GlobalData.Channels.TryRemove(Name, out _);

        //GlobalData.IdChannels.TryAdd(Id, this);
        //GlobalData.Channels.TryAdd(Name, this);
    }

    public override void Dispose()
    {
        //Config?.SafeDispose();

        //GlobalData.IdChannels.TryRemove(Id, out _);
        //GlobalData.Channels.TryRemove(Name, out _);
        DeviceThreadManage = null;
        GC.SuppressFinalize(this);
    }
}
