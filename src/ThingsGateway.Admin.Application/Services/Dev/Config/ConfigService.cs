//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.FriendlyException;

using Mapster;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IConfigService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class ConfigService : DbRepository<SysConfig>, IConfigService
{
    private readonly ISimpleCacheService _simpleCacheService;

    public ConfigService(ISimpleCacheService simpleCacheService)
    {
        _simpleCacheService = simpleCacheService;
    }

    /// <inheritdoc/>
    public async Task<List<SysConfig>> GetListByCategoryAsync(string category)
    {
        var key = SystemConst.Cache_DevConfig + category;//系统配置key
        //先从redis拿配置
        var configList = _simpleCacheService.Get<List<SysConfig>>(key);
        if (configList == null)
        {
            //redis没有再去数据可拿
            configList = await GetListAsync(it => it.Category == category);//获取系统配置列表
            if (configList.Count > 0)
            {
                _simpleCacheService.Set(key, configList);//如果不为空,插入redis
            }
        }
        return configList;
    }

    /// <inheritdoc/>
    public async Task<SysConfig> GetByConfigKeyAsync(string category, string configKey)
    {
        var configList = await GetListByCategoryAsync(category);//获取系统配置列表
        var configValue = configList.Where(it => it.ConfigKey == configKey).FirstOrDefault();//根据configkey获取对应值
        return configValue;
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<SysConfig>> PageAsync(ConfigPageInput input)
    {
        var query = Context.Queryable<SysConfig>()
            .Where(it => it.Category == CateGoryConst.Config_BIZ_DEFINE)//自定义配置
            .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.ConfigKey.Contains(input.SearchKey) || it.ConfigValue.Contains(input.SearchKey));//根据关键字查询

        //根据关键字查询
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.SortCode);//排序
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    /// <inheritdoc/>
    [OperDesc("添加配置项")]
    public async Task AddAsync(ConfigAddInput input)
    {
        await CheckInput(input);
        var devConfig = input.Adapt<SysConfig>();//实体转换
        if (await InsertAsync(devConfig))//插入数据)
            await RefreshCache(CateGoryConst.Config_BIZ_DEFINE);//刷新缓存
    }

    /// <inheritdoc/>
    [OperDesc("编辑配置项")]
    public async Task EditAsync(ConfigEditInput input)
    {
        await CheckInput(input);
        var devConfig = input.Adapt<SysConfig>();//实体转换
        if (await UpdateAsync(devConfig))//更新数据
            await RefreshCache(CateGoryConst.Config_BIZ_DEFINE);//刷新缓存
    }

    /// <inheritdoc/>
    [OperDesc("删除配置项")]
    public async Task DeleteAsync(List<BaseIdInput> input)
    {
        //获取所有业务配置
        var configs = await GetListByCategoryAsync(CateGoryConst.Config_BIZ_DEFINE);
        var ids = input.Select(a => a.Id).ToList();
        if (configs.Any(it => ids.Contains(it.Id)))//如果有当前配置
        {
            if (await Context.Deleteable<SysConfig>().Where(a => ids.Contains(a.Id)).ExecuteCommandAsync() > 0)//删除配置
                await RefreshCache(CateGoryConst.Config_BIZ_DEFINE);//刷新缓存
        }
    }

    /// <inheritdoc/>
    public async Task EditBatchAsync(List<SysConfig> devConfigs)
    {
        if (devConfigs.Count > 0)
        {
            //根据分类分组
            var configGroups = devConfigs.GroupBy(it => it.Category).ToList();
            //遍历分组
            foreach (var item in configGroups)
            {
                var configList = await GetListByCategoryAsync(item.Key);//获取系统配置列表
                var configs = item.ToList();//获取分组完的配置列表
                var keys = configs.Select(it => it.ConfigKey).ToList();//获取key列表
                configList = configList.Where(it => keys.Contains(it.ConfigKey)).ToList();//获取要修改的列表
                //遍历配置列表
                configList.ForEach(it =>
                {
                    //赋值ConfigValue
                    it.ConfigValue = configs.Where(c => c.ConfigKey == it.ConfigKey).First().ConfigValue;
                });
                //更新数据
                if (await UpdateRangeAsync(configList))
                    await RefreshCache(item.Key);//刷新缓存
            }
        }
    }

    #region 方法

    /// <summary>
    /// 刷新缓存
    /// </summary>
    /// <param name="category">分类</param>
    /// <returns></returns>
    private async Task RefreshCache(string category)
    {
        _simpleCacheService.Remove(SystemConst.Cache_DevConfig + category);//redis删除
        await GetListByCategoryAsync(category);//重新获取
    }

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="devConfig"></param>
    private async Task CheckInput(SysConfig devConfig)
    {
        var configs = await GetListByCategoryAsync(CateGoryConst.Config_BIZ_DEFINE);//获取全部字典
        //判断是否从存在重复字典名
        var hasSameKey = configs.Any(it => it.ConfigKey == devConfig.ConfigKey && it.Id != devConfig.Id);
        if (hasSameKey)
        {
            throw Oops.Bah($"存在重复的配置键:{devConfig.ConfigKey}");
        }
        //设置分类为业务
        devConfig.Category = CateGoryConst.Config_BIZ_DEFINE;
    }

    #endregion 方法
}