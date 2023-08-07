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

using Mapster;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

using ThingsGateway.Admin.Application;
using ThingsGateway.Application.Extensions;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.Application;

/// <inheritdoc cref="IVariableService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class VariableService : DbRepository<DeviceVariable>, IVariableService
{
    private readonly ICollectDeviceService _collectDeviceService;
    private readonly IUploadDeviceService _uploadDeviceService;
    private readonly IDriverPluginService _driverPluginService;
    private readonly FileService _fileService;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <inheritdoc cref="IVariableService"/>
    public VariableService(
        ICollectDeviceService collectDeviceService,
        FileService fileService,
        IUploadDeviceService uploadDeviceService,
        IDriverPluginService driverPluginService,
    IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _fileService = fileService;
        _collectDeviceService = collectDeviceService;
        _uploadDeviceService = uploadDeviceService;
        _driverPluginService = driverPluginService;
    }

    /// <inheritdoc/>
    [OperDesc("添加变量")]
    public async Task AddAsync(DeviceVariable input)
    {
        await InsertAsync(input);//添加数据
    }
    /// <inheritdoc/>
    [OperDesc("添加变量")]
    public async Task AddBatchAsync(List<DeviceVariable> input)
    {
        await InsertRangeAsync(input);
    }

    /// <inheritdoc/>
    [OperDesc("删除变量")]
    public async Task DeleteAsync(params long[] input)
    {
        //获取所有ID
        if (input.Length > 0)
        {
            var result = await DeleteByIdsAsync(input.Cast<object>().ToArray());
            if (result)
            {
                DeleteVariableFromCache();
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("清空设备变量")]
    public async Task ClearDeviceVariableAsync()
    {
        var result = await Context.Deleteable<DeviceVariable>(a => !a.IsMemoryVariable).ExecuteCommandAsync() > 0;
        if (result)
        {
            DeleteVariableFromCache();
        }
    }
    /// <inheritdoc/>
    [OperDesc("清空中间变量")]
    public async Task ClearMemoryVariableAsync()
    {
        var result = await Context.Deleteable<DeviceVariable>(a => a.IsMemoryVariable).ExecuteCommandAsync() > 0;
        if (result)
        {
            DeleteVariableFromCache();
        }
    }

    /// <inheritdoc />
    public void DeleteVariableFromCache()
    {
    }

    /// <inheritdoc/>
    [OperDesc("编辑变量")]
    public async Task EditAsync(DeviceVariable input)
    {
        if (await Context.Updateable(input).ExecuteCommandAsync() > 0)//修改数据
            DeleteVariableFromCache();
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<DeviceVariable>> PageAsync(VariablePageInput input)
    {
        var query = GetPage(input);
        long? uploadDevid = null;
        if (!string.IsNullOrEmpty(input.UploadDeviceName))
        {
            uploadDevid = _uploadDeviceService.GetIdByName(input.UploadDeviceName);
        }
        if (!string.IsNullOrEmpty(input.UploadDeviceName))
        {
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size, a => a.VariablePropertys.ContainsKey(uploadDevid ?? 0));//分页
            return pageInfo;

        }
        else
        {
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

    }

    /// <inheritdoc/>
    private ISugarQueryable<DeviceVariable> GetPage(VariablePageInput input)
    {
        long? devid = 0;

        if (!string.IsNullOrEmpty(input.DeviceName))
        {
            devid = _collectDeviceService.GetIdByName(input.DeviceName);
        }

        var query = Context.Queryable<DeviceVariable>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(!string.IsNullOrEmpty(input.VariableAddress), u => u.VariableAddress.Contains(input.VariableAddress))
         .WhereIF(!string.IsNullOrEmpty(input.DeviceName), u => u.DeviceId == (devid ?? 0))
         .Where(u => u.IsMemoryVariable == input.IsMemoryVariable);
        for (int i = 0; i < input.SortField.Count; i++)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序


        return query;
    }


    /// <inheritdoc/>
    public async Task<List<DeviceVariableRunTime>> GetDeviceVariableRuntimeAsync(long devId = 0)
    {
        if (devId == 0)
        {
            var deviceVariables = await GetListAsync(a => a.IsMemoryVariable != true);
            var runtime = deviceVariables.Adapt<List<DeviceVariableRunTime>>();
            return runtime;
        }
        else
        {
            var deviceVariables = await GetListAsync(a => a.DeviceId == devId && (a.IsMemoryVariable != true));
            var runtime = deviceVariables.Adapt<List<DeviceVariableRunTime>>();
            return runtime;
        }

    }
    /// <inheritdoc/>
    public async Task<List<DeviceVariableRunTime>> GetMemoryVariableRuntimeAsync()
    {
        var deviceVariables = await GetListAsync(a => a.IsMemoryVariable == true);
        var runtime = deviceVariables.Adapt<List<DeviceVariableRunTime>>();
        return runtime;
    }

    #region 导入导出
    /// <inheritdoc/>
    public async Task<MemoryStream> ExportFileAsync(MemoryVariableInput input)
    {
        var query = GetPage(input.Adapt<VariablePageInput>());
        long? uploadDevid = null;
        if (!string.IsNullOrEmpty(input.UploadDeviceName))
        {
            uploadDevid = _uploadDeviceService.GetIdByName(input.UploadDeviceName);
        }
        if (!string.IsNullOrEmpty(input.UploadDeviceName))
        {
            var data = await query.ToListAsync();
            if (input.IsMemoryVariable == true)
            {
                return await MemoryVariableExportFileAsync(data.Where(a => a.VariablePropertys.ContainsKey(uploadDevid ?? 0)).ToList().Adapt<List<MemoryVariable>>());
            }
            else
            {
                return await ExportFileAsync(data.Where(a => a.VariablePropertys.ContainsKey(uploadDevid ?? 0)).ToList());
            }

        }
        else
        {
            var data = await query.ToListAsync();
            if (input.IsMemoryVariable == true)
            {
                return await MemoryVariableExportFileAsync(data.Adapt<List<MemoryVariable>>());
            }
            else
            {
                return await ExportFileAsync(data);
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("导出内存变量表", IsRecordPar = false)]
    public async Task<MemoryStream> MemoryVariableExportFileAsync(List<MemoryVariable> deviceVariables = null)
    {
        deviceVariables ??= (await GetListAsync(a => a.IsMemoryVariable == true)).Adapt<List<MemoryVariable>>();

        //总数据
        Dictionary<string, object> sheets = new();
        //变量页
        ConcurrentList<Dictionary<string, object>> devExports = new();
        //变量附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        ConcurrentDictionary<string, List<Dictionary<string, object>>> devicePropertys = new();
        var upDeviceDicts = _uploadDeviceService.GetCacheList(false).ToDictionary(a => a.Id);
        var collectDeviceDicts = _collectDeviceService.GetCacheList(false).ToDictionary(a => a.Id);
        var driverPluginDicts = _driverPluginService.GetCacheList(false).ToDictionary(a => a.Id);
        deviceVariables.ParallelForEach(devData =>
        {
            #region 变量sheet
            //变量页
            var data = devData.GetType().GetPropertiesWithCache().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null);
            Dictionary<string, object> variableExport = new();

            foreach (var item in data)
            {
                //描述
                var desc = item.FindDisplayAttribute();
                //数据源增加
                variableExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
            }

            //添加完整变量信息
            devExports.Add(variableExport);

            #endregion
            #region 上传插件属性
            foreach (var item in devData.VariablePropertys ?? new())
            {
                //插件属性
                //单个设备的行数据
                Dictionary<string, object> driverInfo = new();
                var has = upDeviceDicts.TryGetValue(item.Key, out var uploadDevice);
                if (!has)
                    continue;
                driverInfo.Add(ExportHelpers.UploadDeviceSheetName, uploadDevice?.Name);
                //没有包含变量名称，手动插入
                driverInfo.Add(ExportHelpers.DeviceVariableSheetName, devData.Name);
                foreach (var item1 in item.Value)
                {
                    //添加对应属性数据
                    driverInfo.Add(item1.Description, item1.Value);
                }

                if (uploadDevice != null)
                {
                    //插件名称去除首部ThingsGateway.作为表名
                    var pluginName = driverPluginDicts[uploadDevice.PluginId].AssembleName.Replace(ExportHelpers.PluginLeftName, "");
                    if (devicePropertys.ContainsKey(pluginName))
                    {
                        devicePropertys[pluginName].Add(driverInfo);
                    }
                    else
                    {
                        devicePropertys.TryAdd(pluginName, new() { driverInfo });
                    }
                }


            }
            #endregion

        });

        //添加变量页
        sheets.Add(ExportHelpers.DeviceVariableSheetName, devExports);
        //添加插件属性页
        foreach (var item in devicePropertys)
        {
            sheets.Add(item.Key, item.Value);
        }

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        return memoryStream;
    }

    /// <inheritdoc/>
    [OperDesc("导出采集变量表", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<DeviceVariable> deviceVariables = null)
    {
        deviceVariables ??= await GetListAsync(a => !a.IsMemoryVariable);

        //总数据
        Dictionary<string, object> sheets = new();
        //变量页
        ConcurrentList<Dictionary<string, object>> devExports = new();
        //变量附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        ConcurrentDictionary<string, List<Dictionary<string, object>>> devicePropertys = new();
        var upDeviceDicts = _uploadDeviceService.GetCacheList(false).ToDictionary(a => a.Id);
        var collectDeviceDicts = _collectDeviceService.GetCacheList(false).ToDictionary(a => a.Id);
        var driverPluginDicts = _driverPluginService.GetCacheList(false).ToDictionary(a => a.Id);
        deviceVariables.ParallelForEach(devData =>
   {
       #region 变量sheet
       //变量页
       var data = devData.GetType().GetPropertiesWithCache().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null).OrderBy(
           a =>
           {
               return a.GetCustomAttribute<DataTableAttribute>()?.Order ?? 999999;
           }
           );
       Dictionary<string, object> variableExport = new();
       //变量实体没有包含设备名称，手动插入
       variableExport.Add(ExportHelpers.DeviceName, collectDeviceDicts[devData.DeviceId].Name);

       foreach (var item in data)
       {
           //描述
           var desc = item.FindDisplayAttribute();
           //数据源增加
           variableExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
       }

       //添加完整变量信息
       devExports.Add(variableExport);

       #endregion
       #region 上传插件属性
       foreach (var item in devData.VariablePropertys ?? new())
       {
           //插件属性
           //单个设备的行数据
           Dictionary<string, object> driverInfo = new();
           var has = upDeviceDicts.TryGetValue(item.Key, out var uploadDevice);
           if (!has)
               continue;
           driverInfo.Add(ExportHelpers.UploadDeviceSheetName, uploadDevice?.Name);
           //没有包含变量名称，手动插入
           driverInfo.Add(ExportHelpers.DeviceVariableSheetName, devData.Name);
           foreach (var item1 in item.Value)
           {
               //添加对应属性数据
               driverInfo.Add(item1.Description, item1.Value);
           }

           if (uploadDevice != null)
           {
               //插件名称去除首部ThingsGateway.作为表名
               var pluginName = driverPluginDicts[uploadDevice.PluginId].AssembleName.Replace(ExportHelpers.PluginLeftName, "");
               if (devicePropertys.ContainsKey(pluginName))
               {
                   devicePropertys[pluginName].Add(driverInfo);
               }
               else
               {
                   devicePropertys.TryAdd(pluginName, new() { driverInfo });
               }
           }


       }
       #endregion

   });

        //添加变量页
        sheets.Add(ExportHelpers.DeviceVariableSheetName, devExports);
        //添加插件属性页
        foreach (var item in devicePropertys)
        {
            sheets.Add(item.Key, item.Value);
        }

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        return memoryStream;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, ImportPreviewOutputBase>> MemoryVariablePreviewAsync(IBrowserFile file)
    {
        _fileService.ImportVerification(file);
        using var fs = new MemoryStream();
        using var stream = file.OpenReadStream(512000000);
        await stream.CopyToAsync(fs);
        var sheetNames = MiniExcel.GetSheetNames(fs);

        var dbVariables = await Context.Queryable<DeviceVariable>().Select(it => new { it.Id, it.Name }).ToListAsync();

        //转为字典，提高查找效率
        var dbVariableDicts = dbVariables.ToDictionary(a => a.Name);
        //导入检验结果
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
        //设备页
        ImportPreviewOutput<DeviceVariable> deviceImportPreview = new();
        foreach (var sheetName in sheetNames)
        {
            //单页数据
            var rows = fs.Query(useHeaderRow: true, sheetName: sheetName, configuration: new OpenXmlConfiguration { EnableSharedStringCache = false })
                .Cast<IDictionary<string, object>>();

            if (sheetName == ExportHelpers.DeviceVariableSheetName)
            {
                int row = 1;
                ImportPreviewOutput<DeviceVariable> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                deviceImportPreview = importPreviewOutput;

                //线程安全
                List<DeviceVariable> variables = new();
                rows.ParallelForEach(item =>
               {
                   try
                   {
                       var variable = ((ExpandoObject)item).ConvertToEntity<DeviceVariable>(true);

                       //变量ID都需要手动补录
                       variables.Add(variable);
                       if (dbVariableDicts.TryGetValue(variable.Name, out var dbvar1))
                       {
                           variable.Id = dbvar1.Id;
                           variable.IsUp = true;
                       }
                       else
                       {
                           variable.Id = YitIdHelper.NextId();
                           variable.IsUp = false;
                       }
                       importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, "成功"));
                   }
                   catch (Exception ex)
                   {
                       importPreviewOutput.HasError = true;
                       importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, ex.Message));
                   }
               });

                importPreviewOutput.Data = variables.OrderBy(a => a.Id).ToDictionary(a => a.Name);

            }
            else
            {
                int row = 1;
                ImportPreviewOutput<string> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);

                //插件属性需加上前置名称
                //var newName = ExportHelpers.PluginLeftName + sheetName;
                var newName = sheetName;
                var pluginId = _driverPluginService.GetIdByName(newName);

                try
                {
                    if (pluginId == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, $"插件{newName}不存在"));
                        continue;
                    }

                    var driverPlugin = _driverPluginService.GetDriverPluginById(pluginId.Value);
                    using var serviceScope = _scopeFactory.CreateScope();
                    var pluginSingletonService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
                    var driver = (UpLoadBase)pluginSingletonService.GetDriver(driverPlugin);
                    var propertys = driver.VariablePropertys.GetType().GetPropertiesWithCache()
                        .Where(a => a.GetCustomAttribute<VariablePropertyAttribute>() != null)
                        .ToDictionary(a => a.FindDisplayAttribute(a => a.GetCustomAttribute<VariablePropertyAttribute>()?.Description));

                    var cacheUpdeviceDicts = _uploadDeviceService.GetCacheList(false).ToDictionary(a => a.Name);
                    rows.ParallelForEach(item =>
                    {
                        try
                        {

                            List<DependencyProperty> dependencyProperties = new();
                            foreach (var keyValuePair in item)
                            {
                                if (propertys.TryGetValue(keyValuePair.Key, out var propertyInfo))
                                {
                                    dependencyProperties.Add(new()
                                    {
                                        PropertyName = propertyInfo.Name,
                                        Description = keyValuePair.Key.ToString(),
                                        Value = keyValuePair.Value?.ToString()
                                    });
                                }
                            }
                            //转化插件名称
                            item.TryGetValue(ExportHelpers.DeviceVariableSheetName, out var variableNameObj);
                            item.TryGetValue(ExportHelpers.UploadDeviceSheetName, out var uploadDevName);
                            var variableName = variableNameObj?.ToString();

                            if (uploadDevName != null)
                            {
                                cacheUpdeviceDicts.TryGetValue(uploadDevName.ToString(), out var uploadDevice);

                                var has = deviceImportPreview.Data.TryGetValue(variableName, out var deviceVariable);
                                if (has)
                                {
                                    if (uploadDevice != null)
                                    {
                                        deviceVariable.VariablePropertys?.AddOrUpdate(uploadDevice.Id, a => dependencyProperties, (a, b) => dependencyProperties);
                                        importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, "成功"));
                                    }
                                    else
                                    {
                                        importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{uploadDevName}不存在"));
                                    }
                                }
                                else
                                {
                                    importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{uploadDevName}不存在"));
                                }
                            }
                            else
                            {
                                importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{uploadDevName}不存在"));
                            }
                        }
                        catch (Exception ex)
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, ex.Message));
                        }
                    });
                }
                catch (Exception ex)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, ex.Message));
                }
            }

        }



        return ImportPreviews;
    }


    /// <inheritdoc/>
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file)
    {
        _fileService.ImportVerification(file);
        using var fs = new MemoryStream();
        using var stream = file.OpenReadStream(512000000);
        await stream.CopyToAsync(fs);
        var sheetNames = MiniExcel.GetSheetNames(fs);

        var dbVariables = await Context.Queryable<DeviceVariable>().Select(it => new { it.Id, it.Name }).ToListAsync();
        //转为字典，提高查找效率
        var dbVariableDicts = dbVariables.ToDictionary(a => a.Name);
        //检验结果
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
        //设备页
        ImportPreviewOutput<DeviceVariable> deviceImportPreview = new();
        foreach (var sheetName in sheetNames)
        {
            //单页数据
            var rows = fs.Query(useHeaderRow: true, sheetName: sheetName, configuration: new OpenXmlConfiguration { EnableSharedStringCache = false })
                .Cast<IDictionary<string, object>>();

            if (sheetName == ExportHelpers.DeviceVariableSheetName)
            {
                int row = 0;
                ImportPreviewOutput<DeviceVariable> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                deviceImportPreview = importPreviewOutput;
                var cacheDeviceDicts = _collectDeviceService.GetCacheList(false).ToDictionary(a => a.Name);
                //线程安全
                var variables = new ConcurrentList<DeviceVariable>();
                //并行注意线程安全
                rows.ParallelForEach(item =>
               {
                   try
                   {
                       var variable = ((ExpandoObject)item).ConvertToEntity<DeviceVariable>(true);

                       //转化设备名称
                       item.TryGetValue(ExportHelpers.DeviceName, out var value);
                       var deviceName = value?.ToString();
                       cacheDeviceDicts.TryGetValue(deviceName, out var device);
                       var deviceId = device?.Id;
                       if (deviceId == null)
                       {
                           //找不到对应的设备
                           importPreviewOutput.HasError = true;
                           importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, $"{deviceName}设备不存在"));
                           return;
                       }
                       else
                       {
                           //变量ID和设备ID都需要手动补录
                           variable.DeviceId = deviceId.Value;
                           if (dbVariableDicts.TryGetValue(variable.Name, out var dbvar1))
                           {
                               variable.Id = dbvar1.Id;
                               variable.IsUp = true;
                           }
                           else
                           {
                               variable.Id = YitIdHelper.NextId();
                               variable.IsUp = false;
                           }
                       }

                       variables.Add(variable);
                       importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, "成功"));
                   }
                   catch (Exception ex)
                   {
                       importPreviewOutput.HasError = true;
                       importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, ex.Message));
                   }
               });

                importPreviewOutput.Data = variables.OrderBy(a => a.Id).ToDictionary(a => a.Name);

            }
            else
            {
                int row = 1;
                ImportPreviewOutput<string> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);

                //插件属性需加上前置名称
                //var newName = ExportHelpers.PluginLeftName + sheetName;
                var newName = sheetName;
                var pluginId = _driverPluginService.GetIdByName(newName);

                try
                {
                    if (pluginId == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, $"插件{newName}不存在"));
                        continue;
                    }

                    var driverPlugin = _driverPluginService.GetDriverPluginById(pluginId.Value);
                    using var serviceScope = _scopeFactory.CreateScope();
                    var pluginSingletonService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
                    var driver = (UpLoadBase)pluginSingletonService.GetDriver(driverPlugin);
                    var propertys = driver.VariablePropertys.GetType().GetPropertiesWithCache()
                        .Where(a => a.GetCustomAttribute<VariablePropertyAttribute>() != null)
                        .ToDictionary(a => a.FindDisplayAttribute(a => a.GetCustomAttribute<VariablePropertyAttribute>()?.Description));

                    var cacheUpdeviceDicts = _uploadDeviceService.GetCacheList(false).ToDictionary(a => a.Name);
                    rows.ParallelForEach(item =>
                       {
                           try
                           {

                               List<DependencyProperty> dependencyProperties = new();
                               foreach (var keyValuePair in item)
                               {
                                   if (propertys.TryGetValue(keyValuePair.Key, out var propertyInfo))
                                   {
                                       dependencyProperties.Add(new()
                                       {
                                           PropertyName = propertyInfo.Name,
                                           Description = keyValuePair.Key.ToString(),
                                           Value = keyValuePair.Value?.ToString()
                                       });
                                   }
                               }
                               //转化插件名称
                               item.TryGetValue(ExportHelpers.DeviceVariableSheetName, out var variableNameObj);
                               item.TryGetValue(ExportHelpers.UploadDeviceSheetName, out var uploadDevName);
                               var variableName = variableNameObj?.ToString();

                               if (uploadDevName != null)
                               {
                                   cacheUpdeviceDicts.TryGetValue(uploadDevName.ToString(), out var uploadDevice);

                                   var has = deviceImportPreview.Data.TryGetValue(variableName, out var deviceVariable);
                                   if (has)
                                   {
                                       if (uploadDevice != null)
                                       {
                                           deviceVariable.VariablePropertys?.AddOrUpdate(uploadDevice.Id, a => dependencyProperties, (a, b) => dependencyProperties);
                                           importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, "成功"));
                                       }
                                       else
                                       {
                                           importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{uploadDevName}不存在"));
                                       }
                                   }
                                   else
                                   {
                                       importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{uploadDevName}不存在"));
                                   }
                               }
                               else
                               {
                                   importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{uploadDevName}不存在"));
                               }
                           }
                           catch (Exception ex)
                           {
                               importPreviewOutput.HasError = true;
                               importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, ex.Message));
                           }
                       });
                }
                catch (Exception ex)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, ex.Message));
                }
            }
        }



        return ImportPreviews;
    }

    /// <inheritdoc/>
    [OperDesc("导入变量表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        Dictionary<string, DeviceVariable> variableImports = new();
        foreach (var item in input)
        {
            if (item.Key == ExportHelpers.DeviceVariableSheetName)
            {
                variableImports = ((ImportPreviewOutput<DeviceVariable>)item.Value).Data;
                break;
            }
        }

        var upData = variableImports.Values.Where(a => a.IsUp).ToList();
        var insertData = variableImports.Values.Where(a => !a.IsUp).ToList();
        await Context.Fastest<DeviceVariable>().PageSize(100000).BulkCopyAsync(insertData);
        await Context.Fastest<DeviceVariable>().PageSize(100000).BulkUpdateAsync(upData);

        DeleteVariableFromCache();
    }



    #endregion

}