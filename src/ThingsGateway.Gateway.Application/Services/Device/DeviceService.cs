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

using Furion.FriendlyException;

using Mapster;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

using MiniExcelLibs;

using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;

using ThingsGateway.Cache;
using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <inheritdoc cref="IDeviceService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class DeviceService : DbRepository<Device>, IDeviceService
{
    protected readonly IFileService _fileService;
    protected readonly IServiceScope _serviceScope;
    protected readonly ISimpleCacheService _simpleCacheService;
    protected readonly IImportExportService _importExportService;

    /// <inheritdoc cref="IDeviceService"/>
    public DeviceService(
    IServiceScopeFactory serviceScopeFactory,
    IFileService fileService,
    IImportExportService importExportService,
    ISimpleCacheService simpleCacheService
        )
    {
        _fileService = fileService;
        _serviceScope = serviceScopeFactory.CreateScope();
        _simpleCacheService = simpleCacheService;
        _importExportService = importExportService;
    }

    [OperDesc("添加设备")]
    public async Task AddAsync(DeviceAddInput input)
    {
        var model_Id = GetIdByName(input.Name);
        if (model_Id > 0 && model_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}"); //缓存不要求全部数据，最后会由数据库约束报错
        await InsertAsync(input);//添加数据
        DeleteDeviceFromRedis();
    }

    [OperDesc("复制设备", IsRecordPar = false)]
    public async Task CopyAsync(IEnumerable<Device> input, int count)
    {
        List<Device> newDevs = new();

        for (int i = 0; i < count; i++)
        {
            var newDev = input.Adapt<List<Device>>();

            newDev.ForEach(a =>
            {
                a.Id = YitIdHelper.NextId();
                a.Name = $"{Regex.Replace(a.Name, @"\d", "")}{a.Id}";
            });
            newDevs.AddRange(newDev);
        }

        await Context.Fastest<Device>().PageSize(50000).BulkCopyAsync(newDevs);
        DeleteDeviceFromRedis();
    }

    [OperDesc("删除设备")]
    public async Task DeleteAsync(List<BaseIdInput> input)
    {
        var ids = input.Select(a => a.Id).ToList();
        var variableService = _serviceScope.ServiceProvider.GetService<IVariableService>();
        variableService.NewContent = NewContent;
        //事务
        var result = await NewContent.UseTranAsync(async () =>
        {
            await Context.Deleteable<Device>().Where(a => ids.Contains(a.Id)).ExecuteCommandAsync();
            await variableService.DeleteByDeviceIdAsync(ids);
        });
        if (result.IsSuccess)//如果成功了
        {
            DeleteDeviceFromRedis();
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    [OperDesc("清空设备")]
    public async Task ClearAsync(PluginTypeEnum pluginType)
    {
        var variableService = _serviceScope.ServiceProvider.GetService<IVariableService>();
        variableService.NewContent = NewContent;
        //事务
        var result = await NewContent.UseTranAsync(async () =>
        {
            await Context.Deleteable<Device>().Where(a => a.PluginType == pluginType).ExecuteCommandAsync();
            if (pluginType == PluginTypeEnum.Collect)
                await variableService.ClearAsync();
        });
        if (result.IsSuccess)//如果成功了
        {
            DeleteDeviceFromRedis();
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    [OperDesc("删除设备")]
    public async Task DeleteByChannelIdAsync(List<BaseIdInput> input)
    {
        var ids = input.Select(a => a.Id).ToList();
        var variableService = _serviceScope.ServiceProvider.GetService<IVariableService>();
        variableService.NewContent = NewContent;
        //事务
        var result = await NewContent.UseTranAsync(async () =>
        {
            var deviceIds = await Context.Queryable<Device>().Where(a => ids.Contains(a.ChannelId)).Select(a => a.Id).ToListAsync();
            await Context.Deleteable<Device>().Where(a => deviceIds.Contains(a.Id)).ExecuteCommandAsync();
            await variableService.DeleteByDeviceIdAsync(deviceIds);
        });
        if (result.IsSuccess)//如果成功了
        {
            DeleteDeviceFromRedis();
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc />
    public void DeleteDeviceFromRedis()
    {
        _simpleCacheService.Remove(ThingsGatewayCacheConst.Cache_Device);//删除设备缓存
    }

    [OperDesc("编辑设备")]
    public async Task EditAsync(DeviceEditInput input)
    {
        var model_Id = GetIdByName(input.Name);
        if (model_Id > 0 && model_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}");//缓存不要求全部数据，最后会由数据库约束报错

        if (await Context.Updateable(input.Adapt<Device>()).ExecuteCommandAsync() > 0)//修改数据
            DeleteDeviceFromRedis();
    }

    [OperDesc("编辑设备", IsRecordPar = false)]
    public async Task EditAsync(List<Device> input)
    {
        if (await Context.Updateable(input).ExecuteCommandAsync() > 0)//修改数据
            DeleteDeviceFromRedis();
    }

    public Device? GetDeviceById(long id)
    {
        var data = GetCacheList();
        return data?.FirstOrDefault(x => x.Id == id);
    }

    public List<Device> GetCacheList()
    {
        var device = _simpleCacheService.HashGetAll<Device>(ThingsGatewayCacheConst.Cache_Device);
        if (device == null || device.Count == 0)
        {
            var data = GetList();
            _simpleCacheService.HashSet(ThingsGatewayCacheConst.Cache_Device, data.ToDictionary(a => a.Id.ToString()));
            return data;
        }
        return device.Values.ToList();
    }

    public long? GetIdByName(string name)
    {
        var data = GetCacheList();
        return data?.FirstOrDefault(x => x.Name == name)?.Id;
    }

    public string? GetNameById(long id)
    {
        var data = GetCacheList();
        return data?.FirstOrDefault(x => x.Id == id)?.Name;
    }

    public Task<SqlSugarPagedList<Device>> PageAsync(DevicePageInput input)
    {
        var query = GetPage(input);
        return query.ToPagedListAsync(input.Current, input.Size);//分页
    }

    /// <inheritdoc/>
    private ISugarQueryable<Device> GetPage(DevicePageInput input)
    {
        ISugarQueryable<Device> query = Context.Queryable<Device>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(input.ChannelId != null, u => u.ChannelId == input.ChannelId)
         .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginName == input.PluginName)
         .Where(u => u.PluginType == input.PluginType);
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    public async Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntimeAsync(long? devId = null)
    {
        if (devId == null)
        {
            var devices = GetCacheList().Where(a => a.Enable && a.PluginType == PluginTypeEnum.Collect);
            var channelService = _serviceScope.ServiceProvider.GetService<IChannelService>();
            var channels = channelService.GetCacheList().Where(a => a.Enable);
            devices = devices.Where(a => channels.Select(a => a.Id).Contains(a.ChannelId));
            var runtime = devices.Adapt<List<CollectDeviceRunTime>>().ToDictionary(a => a.Id);
            var variableService = _serviceScope.ServiceProvider.GetService<IVariableService>();
            var collectVariableRunTimes = await variableService.GetVariableRuntimeAsync();
            runtime.Values.ParallelForEach(device =>
            {
                device.Channel = channels.FirstOrDefault(a => a.Id == device.ChannelId);
                device.VariableRunTimes = collectVariableRunTimes.Where(a => a.DeviceId == device.Id).ToList();
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
            var device = GetCacheList().FirstOrDefault(a => a.Enable && a.PluginType == PluginTypeEnum.Collect && a.Id == devId);
            if (device == null)
            {
                return new List<CollectDeviceRunTime>() { };
            }
            var channelService = _serviceScope.ServiceProvider.GetService<IChannelService>();
            var channels = channelService.GetCacheList().Where(a => a.Enable);
            if (!channels.Select(a => a.Id).Contains(device.ChannelId))
            {
                return new List<CollectDeviceRunTime>() { };
            }

            var runtime = device.Adapt<CollectDeviceRunTime>();
            var variableService = _serviceScope.ServiceProvider.GetService<IVariableService>();
            var collectVariableRunTimes = await variableService.GetVariableRuntimeAsync(devId);
            runtime.VariableRunTimes = collectVariableRunTimes;
            runtime.Channel = channels.FirstOrDefault(a => a.Id == runtime.ChannelId);

            collectVariableRunTimes.ParallelForEach(variable =>
            {
                variable.CollectDeviceRunTime = runtime;
                variable.DeviceName = runtime.Name;
            });
            return new List<CollectDeviceRunTime>() { runtime };
        }
    }

    public async Task<List<DeviceRunTime>> GetBusinessDeviceRuntimeAsync(long? devId = null)
    {
        await Task.CompletedTask;
        if (devId == null)
        {
            var devices = GetCacheList().Where(a => a.Enable && a.PluginType == PluginTypeEnum.Business);
            var channelService = _serviceScope.ServiceProvider.GetService<IChannelService>();
            var channels = channelService.GetCacheList().Where(a => a.Enable);
            devices = devices.Where(a => channels.Select(a => a.Id).Contains(a.ChannelId));

            var runtime = devices.Adapt<List<DeviceRunTime>>();
            runtime.ParallelForEach(device =>
            {
                device.Channel = channels.FirstOrDefault(a => a.Id == device.ChannelId);
            });
            return runtime;
        }
        else
        {
            var device = GetCacheList().FirstOrDefault(a => a.Enable && a.PluginType == PluginTypeEnum.Business && a.Id == devId);
            if (device == null)
            {
                return new List<DeviceRunTime>() { };
            }
            var channelService = _serviceScope.ServiceProvider.GetService<IChannelService>();
            var channels = channelService.GetCacheList().Where(a => a.Enable);
            if (!channels.Select(a => a.Id).Contains(device.ChannelId))
            {
                return new List<DeviceRunTime>() { };
            }
            var runtime = device.Adapt<CollectDeviceRunTime>();
            runtime.Channel = channels.FirstOrDefault(a => a.Id == runtime.ChannelId);
            return new List<DeviceRunTime>() { runtime };
        }
    }

    #region 导出

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <returns></returns>
    [OperDesc("导出设备", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(PluginTypeEnum input)
    {
        //导出
        var data = GetCacheList().Where(a => a.PluginType == input)?.OrderBy(a => a.PluginType);
        return await Export(data, input);
    }

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [OperDesc("导出设备", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(DeviceInput input)
    {
        var data = (await GetPage(input.Adapt<DevicePageInput>()).ExportIgnoreColumns().ToListAsync())?.OrderBy(a => a.PluginType);
        return await Export(data, input.PluginType);
    }

    private async Task<FileStreamResult> Export(IEnumerable<Device>? data, PluginTypeEnum pluginType)
    {
        string fileName;
        Dictionary<string, object> sheets;
        ExportCore(data, pluginType, out fileName, out sheets);

        return await _importExportService.ExportAsync<Device>(sheets, fileName, false);
    }

    /// <summary>
    /// 导出文件
    /// </summary>
    [OperDesc("导出设备", IsRecordPar = false)]
    public async Task<MemoryStream> ExportMemoryStream(IEnumerable<Device>? data, PluginTypeEnum pluginType, string channelName = null)
    {
        string fileName;
        Dictionary<string, object> sheets;
        ExportCore(data, pluginType, out fileName, out sheets);
        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        return memoryStream;
    }

    private void ExportCore(IEnumerable<Device>? data, PluginTypeEnum pluginType, out string fileName, out Dictionary<string, object> sheets, string channelName = null)
    {
        if (data == null || !data.Any())
        {
            data = new List<Device>();
        }
        fileName = pluginType == PluginTypeEnum.Collect ? "CollectDevice" : "BusinessDevice";
        var deviceDicts = GetCacheList().ToDictionary(a => a.Id);
        var channelDicts = _serviceScope.ServiceProvider.GetService<IChannelService>().GetCacheList().ToDictionary(a => a.Id);
        //总数据
        sheets = new();
        //设备页
        List<Dictionary<string, object>> deviceExports = new();
        //设备附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();

        #region 列名称

        var type = typeof(Device);
        var propertyInfos = type.GetProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null).OrderBy(
           a =>
           {
               return a.GetCustomAttribute<DataTableAttribute>()?.Order ?? 999999;
           }
           );

        #endregion 列名称

        foreach (var device in data)
        {
            Dictionary<string, object> devExport = new();
            deviceDicts.TryGetValue(device.RedundantDeviceId, out var redundantDevice);
            channelDicts.TryGetValue(device.ChannelId, out var channel);

            devExport.Add(ExportConst.ChannelName, channel?.Name ?? channelName);

            foreach (var item in propertyInfos)
            {
                //描述
                var desc = item.FindDisplayAttribute();
                //数据源增加
                devExport.Add(desc ?? item.Name, item.GetValue(device)?.ToString());
            }

            //设备实体没有包含冗余设备名称，手动插入
            devExport.Add(ExportConst.RedundantDeviceName, redundantDevice?.Name);

            //添加完整设备信息
            deviceExports.Add(devExport);

            #region 插件sheet

            //插件属性
            //单个设备的行数据
            Dictionary<string, object> driverInfo = new();
            //没有包含设备名称，手动插入
            if (device.DevicePropertys.Count > 0)
            {
                driverInfo.Add(ExportConst.DeviceName, device.Name);
            }
            foreach (var item in device.DevicePropertys ?? new())
            {
                //添加对应属性数据
                driverInfo.Add(item.Description, item.Value);
            }

            var pluginName = device.PluginName.GetFileNameAndTypeName();
            if (devicePropertys.ContainsKey(pluginName.Item2))
            {
                if (driverInfo.Count > 0)
                    devicePropertys[pluginName.Item2].Add(driverInfo);
            }
            else
            {
                if (driverInfo.Count > 0)
                    devicePropertys.Add(pluginName.Item2, new() { driverInfo });
            }

            #endregion 插件sheet
        }
        //添加设备页
        sheets.Add(ExportConst.DeviceName, deviceExports);

        //HASH
        foreach (var item in devicePropertys)
        {
            HashSet<string> allKeys = new();

            foreach (var dict in item.Value)
            {
                foreach (var key in dict.Keys)
                {
                    allKeys.Add(key);
                }
            }
            foreach (var dict in item.Value)
            {
                foreach (var key in allKeys)
                {
                    if (!dict.ContainsKey(key))
                    {
                        // 添加缺失的键，并设置默认值
                        dict.Add(key, null);
                    }
                }
            }

            sheets.Add(item.Key, item.Value);
        }
    }

    #endregion 导出

    #region 导入

    /// <inheritdoc/>
    [OperDesc("导入设备表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var collectDevices = new List<Device>();
        foreach (var item in input)
        {
            if (item.Key == ExportConst.DeviceName)
            {
                var collectDeviceImports = ((ImportPreviewOutput<Device>)item.Value).Data;
                collectDevices = new List<Device>(collectDeviceImports.Values);
                break;
            }
        }
        var upData = collectDevices.Where(a => a.IsUp).ToList();
        var insertData = collectDevices.Where(a => !a.IsUp).ToList();
        await Context.Fastest<Device>().PageSize(100000).BulkCopyAsync(insertData);
        await Context.Fastest<Device>().PageSize(100000).BulkUpdateAsync(upData);
        DeleteDeviceFromRedis();
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await _importExportService.UploadFileAsync(browserFile);
        try
        {
            var sheetNames = MiniExcel.GetSheetNames(path);
            var deviceDicts = GetCacheList().ToDictionary(a => a.Name);
            var channelService = _serviceScope.ServiceProvider.GetService<IChannelService>();
            var channelDicts = channelService.GetCacheList().ToDictionary(a => a.Name);

            //导入检验结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            //设备页
            ImportPreviewOutput<Device> deviceImportPreview = new();
            var pluginService = _serviceScope.ServiceProvider.GetService<IPluginService>();
            var driverPluginFullNameDict = pluginService.GetList().SelectMany(a => a.Children).ToDictionary(a => a.FullName);
            var driverPluginNameDict = pluginService.GetList().SelectMany(a => a.Children).ToDictionary(a => a.Name);
            foreach (var sheetName in sheetNames)
            {
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();

                #region 采集设备sheet

                if (sheetName == ExportConst.DeviceName)
                {
                    int row = 1;
                    ImportPreviewOutput<Device> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);
                    deviceImportPreview = importPreviewOutput;
                    List<Device> devices = new();

                    rows.ForEach(item =>
                    {
                        try
                        {
                            var device = ((ExpandoObject)item).ConvertToEntity<Device>(true);
                            if (device == null)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, "无法识别任何信息"));
                                return;
                            }

                            #region 特殊转化名称

                            //转化冗余设备名称
                            var hasRedundant = item.TryGetValue(ExportConst.RedundantDeviceName, out var redundantObj);
                            var hasChannel = item.TryGetValue(ExportConst.ChannelName, out var channelObj);

                            #endregion 特殊转化名称

                            //设备ID、冗余设备ID都需要手动补录
                            if (hasRedundant && redundantObj != null)
                            {
                                if (deviceDicts.TryGetValue(redundantObj.ToString(), out var rendundantDevice))
                                    device.RedundantDeviceId = rendundantDevice.Id;
                                else
                                {
                                    importPreviewOutput.HasError = true;
                                    importPreviewOutput.Results.Add((row++, false, "冗余设备错误"));
                                    return;
                                }
                            }
                            else
                            {
                                if (device.IsRedundant)
                                {
                                    importPreviewOutput.HasError = true;
                                    importPreviewOutput.Results.Add((row++, false, "冗余设备错误"));
                                    return;
                                }
                            }
                            if (hasChannel && channelObj != null)
                            {
                                if (channelDicts.TryGetValue(channelObj.ToString(), out var channel))
                                    device.ChannelId = channel.Id;
                                else
                                {
                                    importPreviewOutput.HasError = true;
                                    importPreviewOutput.Results.Add((row++, false, "通道错误"));
                                    return;
                                }
                            }
                            else
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, "通道错误"));
                                return;
                            }

                            if (deviceDicts.TryGetValue(device.Name, out var collectDevice))
                            {
                                device.Id = collectDevice.Id;
                                device.IsUp = true;
                            }
                            else
                            {
                                device.Id = YitIdHelper.NextId();
                                device.IsUp = false;
                            }

                            devices.Add(device);
                            importPreviewOutput.Results.Add((row++, true, null));
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

                #endregion 采集设备sheet

                else
                {
                    if (deviceImportPreview.Data == null || deviceImportPreview.Data.Count == 0)
                        return ImportPreviews;
                    int row = 1;
                    ImportPreviewOutput<string> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);
                    //插件属性需加上前置名称
                    _ = driverPluginNameDict.TryGetValue(sheetName, out var driverPluginType);
                    if (driverPluginType == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add((row++, false, $"插件{sheetName}不存在"));
                        continue;
                    }

                    var driver = pluginService.GetDriver(driverPluginType.FullName);
                    var propertys = driver.DriverPropertys.GetType().GetProperties()
        .Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null)
        .ToDictionary(a => a.FindDisplayAttribute(a => a.GetCustomAttribute<DynamicPropertyAttribute>()?.Description));
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
                                        Name = propertyInfo.Name,
                                        Description = keyValuePair.Key.ToString(),
                                        Value = keyValuePair.Value?.ToString()
                                    });
                                }
                            }
                            //转化插件名称

                            var value = item[ExportConst.DeviceName];

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

            return ImportPreviews;
        }
        finally
        {
            FileUtility.Delete(path);
        }
    }

    #endregion 导入
}