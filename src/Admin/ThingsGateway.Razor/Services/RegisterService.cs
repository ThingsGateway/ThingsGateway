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

public class RegisterService : IRegisterService
{

    public RegisterService(IHostApplicationLifetime hostApplicationLifetime)
    {
        UUID = DESEncryption.Encrypt($"{MachineInfo.Current.UUID}{MachineInfo.Current.Guid}{MachineInfo.Current.DiskID}");

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(86400000);
                if (!IsRegistered())
                {
                    hostApplicationLifetime.StopApplication();
                }
            }
        });

    }
    /// <summary>
    /// 唯一编码
    /// </summary>
    public string UUID { get; }
    private const string cacheKey = $"{nameof(DefaultRegisterService)}_{nameof(IsRegistered)}";
    public bool IsRegistered()
    {
        return MemoryCache.Instance.GetOrAdd(cacheKey, (a) =>
          {
              try
              {
                  var password = FileUtil.ReadFile(Path.Combine(AppContext.BaseDirectory, "password"));
                  if (password.IsNullOrEmpty()) return false;
                  var rawpassword = DESEncryption.Decrypt(password);
                  if (rawpassword.IsNullOrEmpty()) return false;
                  if (DESEncryption.Encrypt(UUID).Equals(rawpassword, StringComparison.OrdinalIgnoreCase)) return true;
                  return false;
              }
              catch
              {
                  return false;
              }
          });
    }

    public bool Register(string password)
    {
        try
        {
            if (password.IsNullOrEmpty()) return false;
            if (DESEncryption.Encrypt(UUID).Equals(password, StringComparison.OrdinalIgnoreCase))
            {
                FileUtil.WriteFile(Path.Combine(AppContext.BaseDirectory, "password"), DESEncryption.Encrypt(password));
                MemoryCache.Instance.Remove(cacheKey);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
