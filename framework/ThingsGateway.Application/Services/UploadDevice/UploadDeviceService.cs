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

/// <inheritdoc cref="IUploadDeviceService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class UploadDeviceService : DbRepository<UploadDevice>, IUploadDeviceService
{
    private readonly IDriverPluginService _driverPluginService;
    private readonly IFileService _fileService;

    /// <inheritdoc cref="IUploadDeviceService"/>
    public UploadDeviceService(
         IDriverPluginService driverPluginService,
        IFileService fileService
   )
    {
        _fileService = fileService;
        _driverPluginService = driverPluginService;
    }

    /// <inheritdoc/>
    [OperDesc("添加上传设备")]
    public async Task AddAsync(UploadDevice input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0)
            throw Oops.Bah($"存在重复的名称:{input.Name}");
        await InsertAsync(input);//添加数据
        CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_UploadDevice);//cache删除
    }

    /// <inheritdoc/>
    [OperDesc("复制上传设备")]
    public async Task CopyDevAsync(IEnumerable<UploadDevice> input)
    {
        var newDevs = input.Adapt<List<UploadDevice>>();
        newDevs.ForEach(a =>
        {
            var newId = YitIdHelper.NextId();
            a.Id = newId;
            a.Name = "Copy-" + a.Name + "-" + newId.ToString();
        });

        var result = await InsertRangeAsync(newDevs);//添加数据
        CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_UploadDevice);//cache删除

    }


    /// <inheritdoc/>
    [OperDesc("删除上传设备")]
    public async Task DeleteAsync(params long[] input)
    {
        //获取所有ID
        if (input.Length > 0)
        {
            var result = await DeleteByIdsAsync(input.Cast<object>().ToArray());
            if (result)
            {
                CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_UploadDevice);//cache删除
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("编辑上传设备")]
    public async Task EditAsync(UploadDeviceEditInput input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0 && account_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}");

        if (await Context.Updateable(input.Adapt<UploadDevice>()).ExecuteCommandAsync() > 0)//修改数据
            CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_UploadDevice);//cache删除
    }



    /// <inheritdoc cref="IUploadDeviceService"/>
    public List<UploadDevice> GetCacheList(bool isMapster = true)
    {
        //先从Cache拿
        var uploadDevice = CacheStatic.Cache.Get<List<UploadDevice>>(ThingsGatewayCacheConst.Cache_UploadDevice, isMapster);
        if (uploadDevice == null)
        {
            uploadDevice = Context.Queryable<UploadDevice>().ToList();
            if (uploadDevice != null)
            {
                //插入Cache
                CacheStatic.Cache.Set(ThingsGatewayCacheConst.Cache_UploadDevice, uploadDevice, isMapster);
            }
        }
        return uploadDevice;
    }

    /// <inheritdoc/>
    public UploadDevice GetDeviceById(long Id)
    {
        var data = GetCacheList();
        return data.FirstOrDefault(it => it.Id == Id);
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
    public List<UploadDeviceRunTime> GetUploadDeviceRuntime(long devId = 0)
    {
        ConcurrentDictionary<long, DriverPlugin> driverPlugins = new(_driverPluginService.GetCacheList(false).ToDictionary(a => a.Id));
        if (devId == 0)
        {
            var devices = GetCacheList(false).Where(a => a.Enable).ToList();
            var runtime = devices.Adapt<List<UploadDeviceRunTime>>();
            foreach (var device in runtime)
            {
                driverPlugins.TryGetValue(device.PluginId, out var driverPlugin);
                device.PluginName = driverPlugin?.AssembleName;
            }
            return runtime;
        }
        else
        {
            var devices = GetCacheList(false).Where(a => a.Enable).ToList();
            devices = devices.Where(it => it.Id == devId).ToList();
            var runtime = devices.Adapt<List<UploadDeviceRunTime>>();
            foreach (var device in runtime)
            {
                driverPlugins.TryGetValue(device.PluginId, out var driverPlugin);
                device.PluginName = driverPlugin?.AssembleName;
            }
            return runtime;

        }

    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<UploadDevice>> PageAsync(UploadDevicePageInput input)
    {
        var query = GetPage(input);
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }
    /// <inheritdoc/>
    private ISugarQueryable<UploadDevice> GetPage(UploadDevicePageInput input)
    {
        long? pluginid = 0;
        if (!string.IsNullOrEmpty(input.PluginName))
        {
            pluginid = _driverPluginService.GetCacheList(false).FirstOrDefault(it => it.AssembleName.Contains(input.PluginName))?.Id;
        }
        var query = Context.Queryable<UploadDevice>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginId == (pluginid ?? 0))
         .WhereIF(!string.IsNullOrEmpty(input.DeviceGroup), u => u.DeviceGroup == input.DeviceGroup);
        for (int i = 0; i < input.SortField.Count; i++)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }
    #region 导入导出
    /// <inheritdoc/>
    public async Task<MemoryStream> ExportFileAsync(UploadDeviceInput input)
    {
        var query = GetPage(input.Adapt<UploadDevicePageInput>());
        var data = await query.ToListAsync();
        return await ExportFileAsync(data);
    }
    /// <inheritdoc/>
    [OperDesc("导出上传设备表", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<UploadDevice> devDatas = null)
    {
        devDatas ??= GetCacheList(false);

        //总数据
        Dictionary<string, object> sheets = new();
        //设备页
        List<Dictionary<string, object>> devExports = new();
        //设备附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();

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
            //设备实体没有包含插件名称，手动插入
            devExport.Add(ExportHelpers.PluginName, _driverPluginService.GetNameById(devData.PluginId));

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
            var pluginName = _driverPluginService.GetNameById(devData.PluginId).Replace(ExportHelpers.PluginLeftName, "");
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
        sheets.Add(ExportHelpers.UploadDeviceSheetName, devExports);
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
    [OperDesc("导入上传设备表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var uploadDevices = new List<UploadDevice>();
        foreach (var item in input)
        {
            if (item.Key == ExportHelpers.UploadDeviceSheetName)
            {
                var uploadDeviceImports = ((ImportPreviewOutput<UploadDevice>)item.Value).Data;
                uploadDevices = uploadDeviceImports.Values.Adapt<List<UploadDevice>>();
                break;
            }
        }
        await Context.Storageable(uploadDevices).ExecuteCommandAsync();
        CacheStatic.Cache.Remove(ThingsGatewayCacheConst.Cache_UploadDevice);//cache删除

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
        ImportPreviewOutput<UploadDevice> deviceImportPreview = new();
        foreach (var sheetName in sheetNames)
        {
            //单页数据
            var rows = (stream.Query(useHeaderRow: true, sheetName: sheetName)).Cast<IDictionary<string, object>>();
            #region 上传设备sheet
            if (sheetName == ExportHelpers.UploadDeviceSheetName)
            {
                int row = 1;
                ImportPreviewOutput<UploadDevice> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                deviceImportPreview = importPreviewOutput;

                List<UploadDevice> devices = new();
                rows.ForEach(item =>
               {
                   try
                   {
                       var device = ((ExpandoObject)item).ConvertToEntity<UploadDevice>(true);

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
                       #endregion
                       //插件ID、设备ID都需要手动补录
                       device.PluginId = plugin.Id;
                       device.Id = deviceDicts.TryGetValue(device.Name, out var uploadDevice) ? uploadDevice.Id : YitIdHelper.NextId();

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
                var driver = pluginSingletonService.GetDriver(driverPlugin);
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
                        //转化设备名称
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
    #endregion
}