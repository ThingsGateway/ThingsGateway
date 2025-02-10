//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------


using ThingsGateway.NewLife.Extension;
namespace ThingsGateway.Management;

internal sealed class RedundancyService : BaseService<SysDict>, IRedundancyService
{
    private ISysDictService _sysDictService;
    public RedundancyService(ISysDictService sysDictService)
    {
        _sysDictService = sysDictService;
    }
    private void RefreshCache()
    {
        App.CacheService.Remove($"{ThingsGateway.Admin.Application.CacheConst.Cache_SysDict}{DictTypeEnum.System}");
        App.CacheService.Remove($"{ThingsGateway.Admin.Application.CacheConst.Cache_SysDict}{DictTypeEnum.System}{nameof(RedundancyOptions)}");
    }

    /// <inheritdoc/>
    [OperDesc("EditRedundancyOption", localizerType: typeof(RedundancyService))]
    public async Task EditRedundancyOptionAsync(RedundancyOptions input)
    {
        using var db = GetDB();
        //更新数据
        List<SysDict> dicts = new List<SysDict>()
        {
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.Enable), Code = input.Enable.ToString() },
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.MasterUri), Code = input.MasterUri.ToString() },
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.IsMaster), Code = input.IsMaster.ToString() },
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.VerifyToken), Code = input.VerifyToken.ToString() },
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.HeartbeatInterval), Code = input.HeartbeatInterval.ToString() },
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.MaxErrorCount), Code = input.MaxErrorCount.ToString() },
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.IsStartBusinessDevice), Code = input.IsStartBusinessDevice.ToString() },
            new SysDict() { DictType = DictTypeEnum.System, Category = nameof(RedundancyOptions), Name = nameof(RedundancyOptions.SyncInterval), Code = input.SyncInterval.ToString() },
         };
        var storageable = db.Storageable(dicts).WhereColumns(it => new { it.DictType, it.Category, it.Name }).ToStorage();

        //事务
        var result = await db.UseTranAsync(async () =>
        {
            await storageable.AsUpdateable.UpdateColumns(it => new { it.Code }).ExecuteCommandAsync().ConfigureAwait(false);
            await storageable.AsInsertable.ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            RefreshCache();//刷新缓存
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc/>
    public async Task<RedundancyOptions> GetRedundancyAsync()
    {
        var key = $"{CacheConst.Cache_SysDict}{DictTypeEnum.System}{nameof(RedundancyOptions)}";//系统配置key
        var redundancy = App.CacheService.Get<RedundancyOptions>(key);
        if (redundancy == null)
        {
            List<SysDict> sysDicts = await _sysDictService.GetSystemConfigAsync().ConfigureAwait(false);

            redundancy = new RedundancyOptions();

            redundancy.Enable = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.Enable))?.Code.ToBoolean() ?? false;
            redundancy.MasterUri = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.MasterUri))?.Code.ToString();
            redundancy.IsMaster = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.IsMaster))?.Code.ToBoolean() ?? true;
            redundancy.VerifyToken = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.VerifyToken))?.Code.ToString();
            redundancy.HeartbeatInterval = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.HeartbeatInterval))?.Code.ToInt() ?? 3000;
            redundancy.MaxErrorCount = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.MaxErrorCount))?.Code.ToInt() ?? 3000;
            redundancy.IsStartBusinessDevice = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.IsStartBusinessDevice))?.Code.ToBoolean() ?? false;
            redundancy.SyncInterval = sysDicts.FirstOrDefault(a => a.Category == nameof(RedundancyOptions) && a.Name == nameof(RedundancyOptions.SyncInterval))?.Code.ToInt() ?? 30000;

            App.CacheService.Set(key, redundancy);
        }

        return redundancy;
    }




}
