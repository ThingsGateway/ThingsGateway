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

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using MiniExcelLibs;

using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

using ThingsGateway.Admin.Application;
using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation.Extension.Generic;

using Yitter.IdGenerator;

namespace ThingsGateway.Application;

/// <inheritdoc cref="ICollectDeviceService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class CollectDeviceService : DbRepository<CollectDevice>, ICollectDeviceService
{
    private readonly IDriverPluginService _driverPluginService;
    private readonly IFileService _fileService;

    /// <inheritdoc cref="ICollectDeviceService"/>
    public CollectDeviceService(
    IDriverPluginService driverPluginService,
    IFileService fileService
        )
    {
        _fileService = fileService;

        _driverPluginService = driverPluginService;
    }

    /// <inheritdoc/>
    [OperDesc("添加采集设备")]
    public async Task AddAsync(CollectDevice input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0)
            throw Oops.Bah($"存在重复的名称:{input.Name}");
        await InsertAsync(input);//添加数据
        CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除

    }

    /// <inheritdoc/>
    [OperDesc("复制采集设备")]
    public async Task CopyDevAsync(IEnumerable<CollectDevice> input)
    {
        var newDevs = input.Adapt<List<CollectDevice>>();
        newDevs.ForEach(a =>
        {
            a.Id = YitIdHelper.NextId();
            a.Name = $"Copy-{a.Name}-{a.Id}";
        });

        var result = await InsertRangeAsync(newDevs);//添加数据
        CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除

    }
    /// <inheritdoc/>
    [OperDesc("复制采集设备与变量")]
    public async Task CopyDevAndVarAsync(IEnumerable<CollectDevice> input)
    {
        var variableService = App.GetService<IVariableService>();
        List<DeviceVariable> variables = new();
        var newDevs = input.Adapt<List<CollectDevice>>();
        foreach (var item in newDevs)
        {
            var newId = YitIdHelper.NextId();
            var deviceVariables = await Context.Queryable<DeviceVariable>().Where(a => a.DeviceId == item.Id).ToListAsync();
            deviceVariables.ForEach(b =>
            {
                b.Id = YitIdHelper.NextId();
                b.DeviceId = newId;
                b.Name = $"Copy-{b.Name}-{b.Id}";
            });
            variables.AddRange(deviceVariables);
            item.Id = newId;
            item.Name = $"Copy-{item.Name}-{newId}";
        }

        var result = await itenant.UseTranAsync(async () =>
        {
            await InsertRangeAsync(newDevs);//添加数据
            await Context.Insertable(variables).ExecuteCommandAsync();//添加数据
        });

        if (result.IsSuccess)
        {
            CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除
        }
        else
        {
            throw Oops.Oh(result.ErrorMessage);
        }


    }

    /// <inheritdoc/>
    public long? GetIdByName(string name)
    {
        var data = GetCacheList(false);
        return data.FirstOrDefault(it => it.Name == name)?.Id;
    }
    /// <inheritdoc/>
    public string GetNameById(long id)
    {
        var data = GetCacheList(false);
        return data.FirstOrDefault(it => it.Id == id)?.Name;
    }
    /// <inheritdoc/>
    public List<DeviceTree> GetTree()
    {
        var data = GetCacheList(false);
        var trees = data.GetTree();
        return trees;
    }

    /// <inheritdoc/>
    [OperDesc("删除采集设备")]
    public async Task DeleteAsync(params long[] input)
    {
        //获取所有ID
        if (input.Length > 0)
        {
            var result = await DeleteByIdsAsync(input.Cast<object>().ToArray());
            var variableService = App.GetService<IVariableService>();
            await Context.Deleteable<DeviceVariable>(it => input.Contains(it.DeviceId)).ExecuteCommandAsync();
            variableService.DeleteVariableFromCache();
            if (result)
            {
                CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("编辑采集设备")]
    public async Task EditAsync(CollectDeviceEditInput input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0 && account_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}");

        if (await Context.Updateable(input.Adapt<CollectDevice>()).ExecuteCommandAsync() > 0)//修改数据
            CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<CollectDevice>> PageAsync(CollectDevicePageInput input)
    {
        var query = GetPage(input);
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }
    /// <inheritdoc/>
    private ISugarQueryable<CollectDevice> GetPage(CollectDevicePageInput input)
    {
        long? pluginid = 0;
        if (!string.IsNullOrEmpty(input.PluginName))
        {
            pluginid = _driverPluginService.GetCacheList(false).FirstOrDefault(it => it.AssembleName.Contains(input.PluginName))?.Id;
        }
        ISugarQueryable<CollectDevice> query = Context.Queryable<CollectDevice>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(!string.IsNullOrEmpty(input.DeviceGroup), u => u.DeviceGroup == input.DeviceGroup)
         .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginId == (pluginid ?? 0));
        for (int i = 0; i < input.SortField.Count; i++)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }





    /// <inheritdoc/>
    public CollectDevice GetDeviceById(long Id)
    {
        var data = GetCacheList();
        return data.FirstOrDefault(it => it.Id == Id);
    }
    /// <inheritdoc/>
    public List<CollectDevice> GetCacheList(bool isMapster = true)
    {
        //先从Cache拿
        var collectDevice = CacheStatic.Cache.Get<List<CollectDevice>>(ThingsGatewayCacheConst.Cache_CollectDevice, isMapster);
        if (collectDevice == null)
        {
            collectDevice = Context.Queryable<CollectDevice>().ToList();
            if (collectDevice != null)
            {
                //插入Cache
                CacheStatic.Cache.Set(ThingsGatewayCacheConst.Cache_CollectDevice, collectDevice, isMapster);
            }
        }
        return collectDevice;
    }

    /// <inheritdoc/>
    public async Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntimeAsync(long devId = 0)
    {
        if (devId == 0)
        {
            var devices = GetCacheList(false).Where(a => a.Enable).ToList();
            var runtime = devices.Adapt<List<CollectDeviceRunTime>>().ToDictionary(a => a.Id);
            var variableService = App.GetService<IVariableService>();
            var collectVariableRunTimes = await variableService.GetDeviceVariableRuntimeAsync();
            ConcurrentDictionary<long, DriverPlugin> driverPlugins = new(_driverPluginService.GetCacheList(false).ToDictionary(a => a.Id));
            runtime.Values.ParallelForEach(device =>
           {
               driverPlugins.TryGetValue(device.PluginId, out var driverPlugin);
               device.PluginName = driverPlugin?.AssembleName;
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
            var variableService = App.GetService<IVariableService>();
            if (runtime == null) return new() { runtime };
            var pluginName = _driverPluginService.GetNameById(device.PluginId);
            var collectVariableRunTimes = await variableService.GetDeviceVariableRuntimeAsync(devId);
            runtime.PluginName = pluginName;
            runtime.DeviceVariableRunTimes = collectVariableRunTimes;

            collectVariableRunTimes.ParallelForEach(variable =>
           {
               variable.CollectDeviceRunTime = runtime;
               variable.DeviceName = runtime.Name;
           });
            return new() { runtime };

        }

    }

    #region 导入导出
    /// <inheritdoc/>
    public async Task<MemoryStream> ExportFileAsync(CollectDeviceInput input)
    {
        var query = GetPage(input.Adapt<CollectDevicePageInput>());
        var data = await query.ToListAsync();
        return await ExportFileAsync(data);
    }
    /// <inheritdoc/>
    [OperDesc("导出采集设备表", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<CollectDevice> devDatas = null)
    {
        devDatas ??= GetCacheList(false);

        //总数据
        Dictionary<string, object> sheets = new();
        //设备页
        List<Dictionary<string, object>> devExports = new();
        //设备附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();
        var driverPluginDicts = _driverPluginService.GetCacheList(false).ToDictionary(a => a.Id);
        var deviceDicts = devDatas.ToDictionary(a => a.Id);
        foreach (var devData in devDatas)
        {
            #region 设备sheet
            //设备页
            var data = devData.GetType().GetPropertiesWithCache().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null);
            Dictionary<string, object> devExport = new();
            foreach (var item in data)
            {
                //描述
                var desc = item.FindDisplayAttribute();
                //数据源增加
                devExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
            }
            driverPluginDicts.TryGetValue(devData.PluginId, out var driverPlugin);
            deviceDicts.TryGetValue(devData.RedundantDeviceId, out var redundantDevice);

            //设备实体没有包含插件名称，手动插入
            devExport.Add(ExportHelpers.PluginName, driverPlugin.AssembleName);
            //设备实体没有包含冗余设备名称，手动插入
            devExport.Add(ExportHelpers.RedundantDeviceName, redundantDevice?.Name);

            //添加完整设备信息
            devExports.Add(devExport);

            #endregion

            #region 插件sheet
            //插件属性
            //单个设备的行数据
            Dictionary<string, object> driverInfo = new();
            //没有包含设备名称，手动插入
            if (devData.DevicePropertys.Count > 0)
            {
                driverInfo.Add(ExportHelpers.DeviceName, devData.Name);
            }
            foreach (var item in devData.DevicePropertys ?? new())
            {
                //添加对应属性数据
                driverInfo.Add(item.Description, item.Value);
            }

            //插件名称去除首部ThingsGateway.作为表名
            var pluginName = driverPlugin.AssembleName.Replace(ExportHelpers.PluginLeftName, "");
            if (devicePropertys.ContainsKey(pluginName))
            {
                if (driverInfo.Count > 0)
                    devicePropertys[pluginName].Add(driverInfo);
            }
            else
            {
                if (driverInfo.Count > 0)
                    devicePropertys.Add(pluginName, new() { driverInfo });
            }

            #endregion
        }

        //添加设备页
        sheets.Add(ExportHelpers.CollectDeviceSheetName, devExports);
        //添加插件属性页
        foreach (var item in devicePropertys)
        {
            sheets.Add(item.Key, item.Value);
        }

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file)
    {
        _fileService.ImportVerification(file);
        using var stream = new MemoryStream();
        using var fs = file.OpenReadStream(512000000);
        await fs.CopyToAsync(stream);
        return await PreviewAsync(stream);
    }


    /// <inheritdoc/>
    public Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(MemoryStream stream)
    {
        var sheetNames = MiniExcel.GetSheetNames(stream);
        var deviceDicts = GetCacheList(false).ToDictionary(a => a.Name);
        var pluginDicts = _driverPluginService.GetCacheList(false).ToDictionary(a => a.AssembleName);

        //导入检验结果
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
        //设备页
        ImportPreviewOutput<CollectDevice> deviceImportPreview = new();
        foreach (var sheetName in sheetNames)
        {
            //单页数据
            var rows = stream.Query(useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();
            #region 采集设备sheet
            if (sheetName == ExportHelpers.CollectDeviceSheetName)
            {
                int row = 1;
                ImportPreviewOutput<CollectDevice> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                deviceImportPreview = importPreviewOutput;
                List<CollectDevice> devices = new();
                rows.ForEach(item =>
               {
                   try
                   {

                       var device = ((ExpandoObject)item).ConvertToEntity<CollectDevice>(true);
                       #region 特殊转化名称
                       //转化插件名称
                       var hasPlugin = item.TryGetValue(ExportHelpers.PluginName, out var pluginObj);

                       if (pluginObj == null || !pluginDicts.TryGetValue(pluginObj.ToString(), out var plugin))
                       {
                           //找不到对应的插件
                           importPreviewOutput.HasError = true;
                           importPreviewOutput.Results.Add((row++, false, $"{ExportHelpers.PluginName}不存在"));
                           return;
                       }
                       //转化冗余设备名称
                       var hasRedundant = item.TryGetValue(ExportHelpers.PluginName, out var redundantObj);

                       #endregion
                       //插件ID、设备ID、冗余设备ID都需要手动补录
                       device.PluginId = plugin.Id;
                       if (hasRedundant && deviceDicts.TryGetValue(redundantObj.ToString(), out var rendundantDevice))
                       {
                           device.RedundantDeviceId = rendundantDevice.Id;
                       }
                       device.Id = deviceDicts.TryGetValue(device.Name, out var collectDevice) ? collectDevice.Id : YitIdHelper.NextId();

                       devices.Add(device);
                       importPreviewOutput.Results.Add((row++, true, "成功"));
                       return;
                   }
                   catch (Exception ex)
                   {

                       importPreviewOutput.HasError = true;
                       importPreviewOutput.Results.Add((row++, false, ex.Message));
                       return;
                   }
               });
                importPreviewOutput.Data = devices.ToDictionary(a => a.Name);

            }
            #endregion
            else
            {
                int row = 1;
                ImportPreviewOutput<string> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                //插件属性需加上前置名称
                //var newName = ExportHelpers.PluginLeftName + sheetName;
                var newName = sheetName;
                var pluginId = _driverPluginService.GetIdByName(newName);
                if (pluginId == null)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add((row++, false, $"插件{newName}不存在"));
                    continue;
                }

                var driverPlugin = _driverPluginService.GetDriverPluginById(pluginId.Value);
                var pluginSingletonService = App.GetService<PluginSingletonService>();
                var driver = (DriverBase)pluginSingletonService.GetDriver(driverPlugin);
                var propertys = driver.DriverPropertys.GetType().GetPropertiesWithCache()
    .Where(a => a.GetCustomAttribute<DevicePropertyAttribute>() != null)
    .ToDictionary(a => a.FindDisplayAttribute(a => a.GetCustomAttribute<DevicePropertyAttribute>()?.Description));
                rows.ForEach(item =>
               {
                   try
                   {

                       List<DependencyProperty> devices = new();
                       foreach (var keyValuePair in item)
                       {
                           if (propertys.TryGetValue(keyValuePair.Key, out var propertyInfo))
                           {
                               devices.Add(new()
                               {
                                   PropertyName = propertyInfo.Name,
                                   Description = keyValuePair.Key.ToString(),
                                   Value = keyValuePair.Value?.ToString()
                               });
                           }

                       }
                       //转化插件名称

                       var value = item[ExportHelpers.DeviceName];

                       deviceImportPreview.Data[value.ToString()].DevicePropertys = devices;
                       importPreviewOutput.Results.Add((row++, true, "成功"));
                       return;
                   }
                   catch (Exception ex)
                   {
                       importPreviewOutput.HasError = true;
                       importPreviewOutput.Results.Add((row++, false, ex.Message));
                       return;
                   }
               });

            }
        }


        return Task.FromResult(ImportPreviews);
    }

    /// <inheritdoc/>
    [OperDesc("导入采集设备表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var collectDevices = new List<CollectDevice>();
        foreach (var item in input)
        {
            if (item.Key == ExportHelpers.CollectDeviceSheetName)
            {
                var collectDeviceImports = ((ImportPreviewOutput<CollectDevice>)item.Value).Data;
                collectDevices = collectDeviceImports.Values.Adapt<List<CollectDevice>>();
                break;
            }
        }
        await Context.Storageable(collectDevices).ExecuteCommandAsync();
        CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除

    }

    #endregion
}