namespace ThingsGateway.Application
{
    /// <inheritdoc cref="IConfigService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class ConfigService : DbRepository<DevConfig>, IConfigService
    {
        private readonly SysCacheService _sysCacheService;

        /// <inheritdoc cref="IConfigService"/>
        public ConfigService(SysCacheService sysCacheService)
        {
            _sysCacheService = sysCacheService;
        }

        /// <inheritdoc/>
        public async Task EditBatch(List<DevConfig> devConfigs)
        {
            if (devConfigs.Count > 0)
            {
                //根据分类分组
                var configGroups = devConfigs.GroupBy(it => it.Category).ToList();
                //遍历分组
                foreach (var item in configGroups)
                {
                    var configList = await GetListByCategory(item.Key);//获取系统配置列表
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


        /// <inheritdoc/>
        [OperDesc("添加配置项")]
        public async Task Add(ConfigAddInput input)
        {
            await CheckInput(input);
            var devConfig = input.Adapt<DevConfig>();//实体转换
            if (await InsertAsync(devConfig))//插入数据)
                await RefreshCache(input.Category);//刷新缓存
        }

        /// <inheritdoc/>
        [OperDesc("删除配置项")]
        public async Task Delete(params ConfigDeleteInput[] input)
        {
            if (input.Any(it => it.Category != CateGoryConst.Config_CUSTOM_DEFINE))
            {
                throw Oops.Bah("勿删除系统配置");
            }

            var ids = input.Select(it => it.Id).ToList();
            await AsDeleteable().Where(it => ids.Contains(it.Id)).ExecuteCommandAsync();
            foreach (var item in input.GroupBy(it => it.Category))
            {
                await RefreshCache(item.Key);//刷新缓存
            }
        }

        /// <inheritdoc/>
        [OperDesc("编辑配置项")]
        public async Task Edit(ConfigEditInput input)
        {
            await CheckInput(input);
            var devConfig = input.Adapt<DevConfig>();//实体转换
            if (await UpdateAsync(devConfig))//更新数据
                await RefreshCache(input.Category);//刷新缓存
        }

        /// <inheritdoc/>
        public async Task<DevConfig> GetByConfigKey(string category, string configKey)
        {
            var configList = await GetListByCategory(category);//获取系统配置列表
            var configValue = configList.Where(it => it.ConfigKey == configKey).FirstOrDefault();//根据configkey获取对应值
            return configValue;
        }

        /// <inheritdoc/>
        public async Task<List<DevConfig>> GetListByCategory(string category)
        {
            //先从cache拿配置
            var configList = _sysCacheService.Get<List<DevConfig>>(CacheConst.Cache_DevConfig, category);
            if (configList == null)
            {
                //cache没有再去数据可拿
                configList = await Context.CopyNew().Queryable<DevConfig>().Where(it => it.Category == category).OrderBy(it => it.SortCode).ToListAsync();//获取系统配置列表
                if (configList.Count > 0)
                {
                    _sysCacheService.Set(CacheConst.Cache_DevConfig, category, configList);//如果不为空,插入cache
                }
            }
            return configList;
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<DevConfig>> Page(ConfigPageInput input)
        {
            var query = Context.Queryable<DevConfig>()
                             .Where(it => it.Category == CateGoryConst.Config_CUSTOM_DEFINE)//自定义配置
                             .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.ConfigKey.Contains(input.SearchKey) || it.ConfigKey.Contains(input.SearchKey))//根据关键字查询
                             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")//排序
                             .OrderBy(it => it.SortCode);
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

        #region 方法

        /// <summary>
        /// 检查输入参数
        /// </summary>
        /// <param name="devConfig"></param>
        private async Task CheckInput(DevConfig devConfig)
        {
            var configs = await GetListByCategory(devConfig.Category);//获取全部字典
                                                                      //判断是否从存在重复字典名
            var hasSameKey = configs.Any(it => it.ConfigKey == devConfig.ConfigKey && it.Id != devConfig.Id);
            if (hasSameKey)
            {
                throw Oops.Bah($"存在重复的配置键:{devConfig.ConfigKey}");
            }
            devConfig.Category = CateGoryConst.Config_CUSTOM_DEFINE;
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <param name="category">分类</param>
        /// <returns></returns>
        private async Task RefreshCache(string category)
        {
            _sysCacheService.Remove(CacheConst.Cache_DevConfig, category);//cache删除
            await GetListByCategory(category);//重新获取
        }

        #endregion 方法
    }
}