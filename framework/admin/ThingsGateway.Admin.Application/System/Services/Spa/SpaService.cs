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

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="ISpaService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class SpaService : DbRepository<SysResource>, ISpaService
{
    private readonly IResourceService _resourceService;

    /// <inheritdoc cref="ISpaService"/>
    public SpaService(IResourceService resourceService)
    {
        this._resourceService = resourceService;
    }

    /// <inheritdoc />
    [OperDesc("添加单页")]
    public async Task AddAsync(SpaAddInput input)
    {
        CheckInput(input);//检查参数
        input.Code = YitIdHelper.NextId().ToString();//code取随机值
        var sysResource = input.Adapt<SysResource>();//实体转换
        if (await InsertAsync(sysResource))//插入数据
            _resourceService.RefreshCache(ResourceCategoryEnum.SPA);//刷新缓存
    }

    /// <inheritdoc />
    [OperDesc("删除单页")]
    public async Task DeleteAsync(params long[] input)
    {
        //获取所有ID
        var ids = input.ToList();
        if (ids.Count > 0)
        {
            //获取所有
            var resourceList = await _resourceService.GetListByCategoryAsync(ResourceCategoryEnum.SPA);
            //找到要删除的
            var sysresources = resourceList.Where(it => ids.Contains(it.Id)).ToList();
            //查找内置单页面
            var system = sysresources.Where(it => it.Code == ResourceConst.System).FirstOrDefault();
            if (system != null)
                throw Oops.Bah($"不可删除系统内置单页面:{system.Title}");
            //删除菜单
            await DeleteAsync(sysresources);
            _resourceService.RefreshCache(ResourceCategoryEnum.SPA);//刷新缓存
        }
    }

    /// <inheritdoc />
    [OperDesc("编辑单页")]
    public async Task EditAsync(SpaEditInput input)
    {
        CheckInput(input);//检查参数
        var sysResource = input.Adapt<SysResource>();//实体转换
        if (await UpdateAsync(sysResource))//更新数据
            _resourceService.RefreshCache(ResourceCategoryEnum.SPA);//刷新缓存
    }

    /// <inheritdoc/>
    public async Task<ISqlSugarPagedList<SysResource>> PageAsync(SpaPageInput input)
    {
        var query = Context.Queryable<SysResource>()
                         .Where(it => it.Category == ResourceCategoryEnum.SPA)//单页
                         .WhereIF(input.TargetType != 0, it => it.TargetType == input.TargetType)//根据菜单类型查询
                         .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Title.Contains(input.SearchKey) || it.Component.Contains(input.SearchKey));//根据关键字查询
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
    /// 检查输入参数
    /// </summary>
    /// <param name="sysResource"></param>
    private void CheckInput(SysResource sysResource)
    {
        //判断菜单类型
        if (sysResource.TargetType == TargetTypeEnum.SELF)
        {
            if (string.IsNullOrEmpty(sysResource.Component))
            {
                throw Oops.Bah($"组件地址不能为空");
            }
        }
        else if (sysResource.TargetType == TargetTypeEnum.BLANK)//如果是内链或者外链
        {
            sysResource.Component = null;
        }
        else
        {
            throw Oops.Bah($"单页类型错误:{sysResource.TargetType}");//都不是
        }
        //设置为单页
        sysResource.Category = ResourceCategoryEnum.SPA;
    }

    #endregion 方法
}