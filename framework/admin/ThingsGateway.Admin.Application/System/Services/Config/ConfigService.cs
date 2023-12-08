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

using Furion.DependencyInjection;
using Furion.FriendlyException;

using Mapster;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IConfigService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class ConfigService : DbRepository<SysConfig>, IConfigService
{
    private readonly IServiceScope _serviceScope;

    public ConfigService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
    }

    /// <inheritdoc/>
    [OperDesc("编辑网关系统配置")]
    public async Task EditBatchAsync(List<SysConfig> sysConfigs)
    {
        if (await UpdateRangeAsync(sysConfigs))
            RefreshCache(sysConfigs.FirstOrDefault()?.Category);//刷新缓存
    }

    /// <inheritdoc/>
    [OperDesc("添加配置项")]
    public async Task AddAsync(ConfigAddInput input)
    {
        await CheckInputAsync(input);//检查
        var sysConfig = input.Adapt<SysConfig>();//实体转换
        if (await InsertAsync(sysConfig))//插入数据)
            RefreshCache(input.Category);//刷新缓存
    }

    /// <inheritdoc/>
    [OperDesc("删除配置项")]
    public async Task DeleteAsync(params long[] input)
    {
        await AsDeleteable().Where(it => input.Contains(it.Id)).ExecuteCommandAsync();
        RefreshCache(ConfigConst.SYS_CONFIGOTHER);//刷新缓存
    }

    /// <inheritdoc/>
    [OperDesc("编辑配置项")]
    public async Task EditAsync(ConfigEditInput input)
    {
        await CheckInputAsync(input);
        var sysConfig = input.Adapt<SysConfig>();//实体转换
        if (await UpdateAsync(sysConfig))//更新数据
            RefreshCache(input.Category);//刷新缓存
    }

    /// <inheritdoc/>
    public async Task<SysConfig> GetByConfigKeyAsync(string category, string configKey)
    {
        var configList = await GetListByCategoryAsync(category);//获取系统配置列表
        return configList.FirstOrDefault(it => it.ConfigKey == configKey);//根据configkey获取对应值
    }

    /// <inheritdoc/>
    public async Task<List<SysConfig>> GetListByCategoryAsync(string category)
    {
        //先从Cache拿，需要获取新的对象，避免操作导致缓存中对象改变
        var configList = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<List<SysConfig>>(CacheConst.SYS_CONFIGCATEGORY + category, true);
        if (configList == null)
        {
            //cache没有再去数据可拿
            configList = await Context.Queryable<SysConfig>().Where(it => it.Category == category).OrderBy(it => it.SortCode).ToListAsync();//获取系统配置列表
            if (configList.Count > 0)
            {
                _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(CacheConst.SYS_CONFIGCATEGORY + category, configList, true);//如果不为空,插入cache
            }
        }
        return configList;
    }

    /// <inheritdoc/>
    public async Task<ISqlSugarPagedList<SysConfig>> PageAsync(ConfigPageInput input)
    {
        var query = Context.Queryable<SysConfig>()
                         .Where(it => it.Category == ConfigConst.SYS_CONFIGOTHER)//自定义配置
                         .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.ConfigKey.Contains(input.SearchKey) || it.ConfigKey.Contains(input.SearchKey));
        //根据关键字查询
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.SortCode);//排序

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    #region 方法

    /// <summary>
    /// 检查输入参数，并设置分类为自定义分类
    /// </summary>
    /// <param name="sysConfig"></param>
    private async Task CheckInputAsync(SysConfig sysConfig)
    {
        var configs = await GetListByCategoryAsync(sysConfig.Category);//获取全部字典
        var hasSameKey = configs.Any(it => it.ConfigKey == sysConfig.ConfigKey && it.Id != sysConfig.Id);
        //判断是否从存在重复字典名
        if (hasSameKey)
        {
            throw Oops.Bah($"存在重复的配置键:{sysConfig.ConfigKey}");
        }
        sysConfig.Category = ConfigConst.SYS_CONFIGOTHER;
    }

    /// <summary>
    /// 刷新缓存
    /// </summary>
    /// <param name="category">分类</param>
    /// <returns></returns>
    private void RefreshCache(string category)
    {
        _serviceScope.ServiceProvider.GetService<MemoryCache>().Remove(CacheConst.SYS_CONFIGCATEGORY + category);//cache删除
    }

    #endregion 方法
}