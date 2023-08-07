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

using Furion;
using Furion.DependencyInjection;
using Furion.FriendlyException;

using System.Data;

using ThingsGateway.Admin.Application;

using Yitter.IdGenerator;

namespace ThingsGateway.Application;

/// <inheritdoc cref="IDriverPluginService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public partial class DriverPluginService : DbRepository<DriverPlugin>, IDriverPluginService
{

    /// <inheritdoc/>
    [OperDesc("添加/更新插件")]
    public async Task AddAsync(DriverPluginAddInput input)
    {
        var pluginService = App.GetService<PluginSingletonService>();
        var datas = await pluginService.TestAddDriverAsync(input);

        var driverPlugins = GetCacheList();
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
        var delete = driverPlugins.Where(a => a.FilePath == datas.FirstOrDefault()?.FilePath).ToList();
        //事务
        var result = await itenant.UseTranAsync(async () =>
        {
            await Context.Deleteable(delete).ExecuteCommandAsync();
            await Context.Storageable(datas).ExecuteCommandAsync();
        });
        if (result.IsSuccess)//如果成功了
        {
            CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_DriverPlugin);//cache删除
        }
        else
        {
            throw Oops.Oh(result.ErrorMessage);
        }
    }

    /// <inheritdoc/>
    public List<DriverPlugin> GetCacheList(bool isMapster = true)
    {
        //先从Cache拿
        var driverPlugins = CacheStatic.Cache.Get<List<DriverPlugin>>(ThingsGatewayCacheConst.Cache_DriverPlugin, isMapster);
        if (driverPlugins == null)
        {
            driverPlugins = Context.Queryable<DriverPlugin>()
            .Select((u) => new DriverPlugin { Id = u.Id.SelectAll() })
            .ToList();
            if (driverPlugins != null)
            {
                //插入Cache
                CacheStatic.Cache.Set(ThingsGatewayCacheConst.Cache_DriverPlugin, driverPlugins, isMapster);
            }
        }
        return driverPlugins;
    }

    /// <inheritdoc/>
    public DriverPlugin GetDriverPluginById(long Id)
    {
        var data = GetCacheList();
        return data.FirstOrDefault(it => it.Id == Id);
    }

    /// <inheritdoc/>
    public List<DriverPluginCategory> GetDriverPluginChildrenList(DriverEnum? driverTypeEnum = null)
    {
        var data = GetCacheList(false);
        if (driverTypeEnum != null)
        {
            data = data.Where(a => a.DriverTypeEnum == driverTypeEnum).ToList();
        }
        var driverPluginCategories = data.GroupBy(a => a.FileName).Select(it =>
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

    /// <inheritdoc/>
    public long? GetIdByName(string name)
    {
        var data = GetCacheList(false);
        return data.FirstOrDefault(it => it.AssembleName == name)?.Id;
    }
    /// <inheritdoc/>
    public string GetNameById(long id)
    {
        var data = GetCacheList(false);
        return data.FirstOrDefault(it => it.Id == id)?.AssembleName;
    }
    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<DriverPlugin>> PageAsync(DriverPluginPageInput input)
    {
        var query = Context.Queryable<DriverPlugin>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.AssembleName.Contains(input.Name))//根据关键字查询
         .WhereIF(!string.IsNullOrEmpty(input.FileName), u => u.FileName.Contains(input.FileName));//根据关键字查询
        for (int i = 0; i < input.SortField.Count; i++)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

}