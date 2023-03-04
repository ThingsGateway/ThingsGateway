using Furion.FriendlyException;

using System.Data;
using System.Linq;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public partial class DriverPluginService : DbRepository<DriverPlugin>, IDriverPluginService
    {
        private readonly SysCacheService _sysCacheService;

        public DriverPluginService(SysCacheService sysCacheService)
        {
            _sysCacheService = sysCacheService;
        }

        /// <inheritdoc/>
        [OperDesc("添加/更新插件")]
        public async Task Add(DriverPluginAddInput input)
        {
            var pluginService = App.GetService<PluginCore>();
            var datas = await pluginService.TestAddDriver(input);

            var driverPlugins = GetCacheListAsync();
            foreach (var item in datas)
            {
                var data = driverPlugins.FirstOrDefault(a => a.AssembleName == item.AssembleName);
                if (data != null)
                {
                    item.Id = data.Id;
                }
                else
                {
                    item.Id = YitIdHelper.NextId();
                }
            }
            var delete = driverPlugins.Where(a => a.FileName == datas.FirstOrDefault()?.FileName).ToList();
            //事务
            var result = await itenant.UseTranAsync(async () =>
            {
                await Context.Deleteable(delete).ExecuteCommandAsync();
                await Context.Storageable(datas).ExecuteCommandAsync();
            });
            if (result.IsSuccess)//如果成功了
            {
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_DriverPlugin, "");//cache删除
            }
            else
            {
                //写日志
                throw Oops.Oh(result.ErrorMessage);
            }
        }

        /// <inheritdoc/>
        public long? GetIdByName(string name)
        {
            var data = GetCacheListAsync();
            return data.FirstOrDefault(it => it.AssembleName == name)?.Id;
        }
        /// <inheritdoc/>
        public string GetNameById(long id)
        {
            var data = GetCacheListAsync();
            return data.FirstOrDefault(it => it.Id == id)?.AssembleName;
        }

        /// <inheritdoc/>
        public DriverPlugin GetDriverPluginById(long Id)
        {
            var data = GetCacheListAsync();
            return data.FirstOrDefault(it => it.Id == Id);
        }
        /// <inheritdoc/>
        public List<DriverPluginCategory> GetDriverPluginChildrenList(DriverEnum driverTypeEnum)
        {
            var data = GetCacheListAsync();
            var driverPluginCategories = data.Where(a => a.DriverTypeEnum == driverTypeEnum).GroupBy(a => a.FileName).Select(it =>
             {
                 var childrens = new List<DriverPluginCategory>();
                 foreach (var item in it)
                 {
                     childrens.Add(new DriverPluginCategory
                     {
                         Id = item.Id,
                         Name = item.AssembleName,
                     }
                     );
                 }
                 return new DriverPluginCategory
                 {
                     Id = YitIdHelper.NextId(),
                     Name = it.Key,
                     Children = childrens,
                 };
             });
            return driverPluginCategories.ToList();
        }
        public List<DriverPlugin> GetCacheListAsync()
        {
            //先从Cache拿
            var driverPlugins = _sysCacheService.Get<List<DriverPlugin>>(ThingsGatewayCacheConst.Cache_DriverPlugin, "");
            if (driverPlugins == null)
            {
                driverPlugins = Context.Queryable<DriverPlugin>()
                .Select((u) => new DriverPlugin { Id = u.Id.SelectAll() })
                .ToList();
                if (driverPlugins != null)//做个大小写限制
                {
                    //插入Cache
                    _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DriverPlugin, "", driverPlugins);
                }
            }
            return driverPlugins;
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<DriverPlugin>> Page(DriverPluginPageInput input)
        {
            var query = Context.Queryable<DriverPlugin>()
             .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.AssembleName.Contains(input.Name))//根据关键字查询
             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
             .OrderBy(u => u.Id)//排序
             .Select((u) => new DriverPlugin { Id = u.Id.SelectAll() })
             ;
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

    }
}