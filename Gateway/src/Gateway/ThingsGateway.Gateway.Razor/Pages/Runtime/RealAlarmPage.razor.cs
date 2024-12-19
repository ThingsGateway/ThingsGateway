//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;
using ThingsGateway.Extension.Generic;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class RealAlarmPage
{
    private VariableRunTime? SearchModel { get; set; } = new();
    private List<long>? DataScope;
    [Inject]
    private ISysUserService SysUserService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        DataScope = await SysUserService.GetCurrentUserDataScopeAsync();
        await base.OnInitializedAsync();
    }
    #region 查询

    private Task<QueryData<VariableRunTime>> OnQueryAsync(QueryPageOptions options)
    {
        var data = GlobalData.ReadOnlyRealAlarmVariables
           .WhereIf(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
         .WhereIf(DataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId)
            .Select(a => a.Value).GetQueryData(options);
        return Task.FromResult(data);
    }

    #endregion 查询
}
