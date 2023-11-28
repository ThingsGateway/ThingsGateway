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

using System.Text.RegularExpressions;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

[Injection(Proxy = typeof(OperDispatchProxy))]
public class CollectDeviceService : DeviceService<CollectDevice>, ITransient
{
    protected override string DeviceSheetName => ExportHelpers.CollectDeviceSheetName;

    public CollectDeviceService(IServiceScopeFactory serviceScopeFactory, IFileService fileService) : base(serviceScopeFactory, fileService)
    {
    }

    /// <summary>
    /// 编辑多个设备，不检测名称重复
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [OperDesc("编辑设备", IsRecordPar = false)]
    public async Task EditsAsync(List<CollectDevice> input)
    {
        if (await Context.Updateable(input).ExecuteCommandAsync() > 0)//修改数据
            RemoveCache();
    }


    /// <inheritdoc/>
    [OperDesc("复制设备与变量", IsRecordPar = false)]
    public virtual async Task CopyDevAndVarAsync(IEnumerable<CollectDevice> input)
    {
        var variableService = _serviceScope.ServiceProvider.GetService<VariableService>();
        List<DeviceVariable> variables = new();
        var newDevs = input.Adapt<List<CollectDevice>>();
        foreach (var item in newDevs)
        {
            var newId = YitIdHelper.NextId();
            var deviceVariables = await Context.Queryable<DeviceVariable>().Where(a => a.DeviceId == item.Id).ToListAsync();

            item.Id = newId;
            item.Name = $"{Regex.Replace(item.Name, @"\d", "")}{newId}";
            deviceVariables.ForEach(b =>
            {
                b.Id = YitIdHelper.NextId();
                b.DeviceId = newId;
                b.Name = $"{Regex.Replace(b.Name, @"\d", "")}{b.Id}";
            });
            variables.AddRange(deviceVariables);

        }

        var result = await itenant.UseTranAsync(async () =>
        {
            await InsertRangeAsync(newDevs);//添加数据
            await Context.Insertable(variables).ExecuteCommandAsync();//添加数据
        });

        if (result.IsSuccess)
        {
            RemoveCache();
        }
        else
        {
            throw Oops.Oh(result.ErrorMessage);
        }


    }
    /// <inheritdoc/>
    public async Task<IEnumerable<DeviceRunTime>> GetDeviceRuntimeAsync(long devId = 0)
    {
        if (devId == 0)
        {
            var devices = GetCacheList(false).Where(a => a.Enable).ToList();
            var runtime = devices.Adapt<List<CollectDeviceRunTime>>().ToDictionary(a => a.Id);
            var variableService = _serviceScope.ServiceProvider.GetService<VariableService>();
            var collectVariableRunTimes = await variableService.GetDeviceVariableRuntimeAsync();
            runtime.Values.ParallelForEach(device =>
            {
                device.DeviceVariableRunTimes = collectVariableRunTimes.Where(a => a.DeviceId == device.Id).ToList();
            });

            collectVariableRunTimes.ParallelForEach(variable =>
            {
                if (runtime.TryGetValue(variable.DeviceId, out var device))
                {
                    variable.CollectDeviceRunTime = device;
                    variable.DeviceName = device.Name;
                }
            });
            return runtime.Values.ToList();
        }
        else
        {
            var device = GetCacheList(false).Where(a => a.Enable).ToList().FirstOrDefault(it => it.Id == devId);
            var runtime = device.Adapt<CollectDeviceRunTime>();
            var variableService = _serviceScope.ServiceProvider.GetService<VariableService>();
            if (runtime == null) return new List<CollectDeviceRunTime>() { runtime };
            var collectVariableRunTimes = await variableService.GetDeviceVariableRuntimeAsync(devId);
            runtime.DeviceVariableRunTimes = collectVariableRunTimes;

            collectVariableRunTimes.ParallelForEach(variable =>
            {
                variable.CollectDeviceRunTime = runtime;
                variable.DeviceName = runtime.Name;
            });
            return new List<CollectDeviceRunTime>() { runtime };

        }

    }

}