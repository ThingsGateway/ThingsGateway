//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;

using ThingsGateway.DataEncryption;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Caching;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Razor;

public class DefaultRegisterService : IRegisterService
{

    public DefaultRegisterService(IHostApplicationLifetime hostApplicationLifetime)
    {
        UUID = DESEncryption.Encrypt($"{MachineInfo.Current.UUID}{MachineInfo.Current.Guid}{MachineInfo.Current.DiskID}");
    }

    /// <summary>
    /// 唯一编码
    /// </summary>
    public string UUID { get; }

    public bool IsRegistered()
    {
        return true;
    }

    public bool Register(string password)
    {
        return true;
    }
}
