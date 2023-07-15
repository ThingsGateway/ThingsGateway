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

using Microsoft.AspNetCore.Components.Forms;

using MiniExcelLibs;

using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Web.Foundation;

/// <inheritdoc cref="IUploadDeviceService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class UploadDeviceService : DbRepository<UploadDevice>, IUploadDeviceService
{
    private readonly SysCacheService _sysCacheService;
    private readonly IDriverPluginService _driverPluginService;
    private readonly IFileService _fileService;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <inheritdoc cref="IUploadDeviceService"/>
    public UploadDeviceService(SysCacheService sysCacheService
        , IDriverPluginService driverPluginService, IFileService fileService,
    IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _fileService = fileService;
        _sysCacheService = sysCacheService;

        _driverPluginService = driverPluginService;
    }

    /// <inheritdoc/>
    [OperDesc("添加上传设备")]
    public async Task AddAsync(UploadDevice input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0)
            throw Oops.Bah($"存在重复的名称:{input.Name}");
        var result = await InsertReturnEntityAsync(input);//添加数据
        _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除

    }

    /// <inheritdoc/>
    [OperDesc("复制上传设备")]
    public async Task CopyDevAsync(IEnumerable<UploadDevice> input)
    {
        var newId = YitIdHelper.NextId();
        var newDevs = input.Adapt<List<UploadDevice>>();
        newDevs.ForEach(a =>
        {
            a.Id = newId;
            a.Name = "Copy-" + a.Name + "-" + newId.ToString();
        });

        var result = await InsertRangeAsync(newDevs);//添加数据
        _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除

    }


    /// <inheritdoc/>
    public long? GetIdByName(string name)
    {
        var data = GetCacheList();
        return data.FirstOrDefault(it => it.Name == name)?.Id;
    }
    /// <inheritdoc/>
    public string GetNameById(long id)
    {
        var data = GetCacheList();
        return data.FirstOrDefault(it => it.Id == id)?.Name;
    }

    /// <inheritdoc/>
    [OperDesc("删除上传设备")]
    public async Task DeleteAsync(List<BaseIdInput> input)
    {
        //获取所有ID
        var ids = input.Select(it => it.Id).ToList();
        if (ids.Count > 0)
        {
            var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
            if (result)
            {
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除
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
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除
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
            pluginid = _driverPluginService.GetCacheList().FirstOrDefault(it => it.AssembleName.Contains(input.PluginName))?.Id;
        }
        var query = Context.Queryable<UploadDevice>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginId == (pluginid ?? 0))
         .WhereIF(!string.IsNullOrEmpty(input.DeviceGroup), u => u.DeviceGroup == input.DeviceGroup)
         .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
         .OrderBy(u => u.Id)//排序
         .Select((u) => new UploadDevice { Id = u.Id.SelectAll() })
            ;
        return query;
    }


    /// <inheritdoc/>
    public async Task<MemoryStream> ExportFileAsync(UploadDevicePageInput input)
    {
        var query = GetPage(input);
        var data = await query.ToListAsync();
        return await ExportFileAsync(data);
    }
    /// <inheritdoc/>
    public UploadDevice GetDeviceById(long Id)
    {
        var data = GetCacheList();
        return data.FirstOrDefault(it => it.Id == Id);
    }
    /// <inheritdoc cref="IUploadDeviceService"/>
    public List<UploadDevice> GetCacheList()
    {
        //先从Cache拿
        var collectDevice = _sysCacheService.Get<List<UploadDevice>>(ThingsGatewayCacheConst.Cache_UploadDevice, "");
        if (collectDevice == null)
        {
            collectDevice = Context.Queryable<UploadDevice>().ToList();
            if (collectDevice != null)
            {
                //插入Cache
                _sysCacheService.Set(ThingsGatewayCacheConst.Cache_UploadDevice, "", collectDevice);
            }
        }
        return collectDevice;
    }

    /// <inheritdoc/>
    public List<UploadDeviceRunTime> GetUploadDeviceRuntime(long devId = 0)
    {
        if (devId == 0)
        {
            var devices = GetCacheList().Where(a => a.Enable).ToList();
            var runtime = devices.Adapt<List<UploadDeviceRunTime>>();
            foreach (var device in runtime)
            {
                var pluginName = _driverPluginService.GetNameById(device.PluginId);
                device.PluginName = pluginName;
            }
            return runtime;
        }
        else
        {
            var devices = GetCacheList().Where(a => a.Enable).ToList();
            devices = devices.Where(it => it.Id == devId).ToList();
            var runtime = devices.Adapt<List<UploadDeviceRunTime>>();
            foreach (var device in runtime)
            {
                var pluginName = _driverPluginService.GetNameById(device.PluginId);
                device.PluginName = pluginName;
            }
            return runtime;

        }

    }

    #region 导入导出

    /// <inheritdoc/>
    [OperDesc("导出上传设备表", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<UploadDevice> devDatas = null)
    {
        devDatas ??= GetCacheList();

        //总数据
        Dictionary<string, object> sheets = new Dictionary<string, object>();
        //设备页
        List<Dictionary<string, object>> devExports = new();
        //设备附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();

        foreach (var devData in devDatas)
        {
            #region 设备sheet
            //设备页
            var data = devData.GetType().GetAllProps().Where(a => a.GetCustomAttribute<ExcelAttribute>() != null);
            Dictionary<string, object> devExport = new();
            foreach (var item in data)
            {
                //描述
                var desc = ObjectExtensions.FindDisplayAttribute(item);
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
            Dictionary<string, object> driverInfo = new Dictionary<string, object>();
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
        return memoryStream;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file)
    {
        _fileService.ImportVerification(file);
        using var fs = new MemoryStream();
        using var stream = file.OpenReadStream(512000000);
        await stream.CopyToAsync(fs);
        var sheetNames = MiniExcel.GetSheetNames(fs);

        //导入检验结果
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
        //设备页
        ImportPreviewOutput<UploadDevice> deviceImportPreview = new();
        foreach (var sheetName in sheetNames)
        {
            //单页数据
            var rows = (await fs.QueryAsync(useHeaderRow: true, sheetName: sheetName)).Cast<IDictionary<string, object>>();
            #region 上传设备sheet
            if (sheetName == ExportHelpers.UploadDeviceSheetName)
            {
                int row = 1;
                ImportPreviewOutput<UploadDevice> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                deviceImportPreview = importPreviewOutput;

                List<UploadDevice> devices = new List<UploadDevice>();
                await rows.ForeachAsync(item =>
                {
                    try
                    {
                        var device = ((ExpandoObject)item).ConvertToEntity<UploadDevice>(true);
                        //var hasDup = rows.HasDuplicateElements<DeviceVariable>(nameof(UploadDevice.Name), device.Name);
                        //var hasName = GetIdByName(device.Name) > 0;
                        //if (hasDup || hasName)
                        //{
                        //    importPreviewOutput.HasError = true;
                        //    importPreviewOutput.Results.Add((false, "名称重复"));
                        //    return Task.CompletedTask;
                        //}
                        #region 特殊转化名称
                        //转化插件名称
                        var pluginName = item.FirstOrDefault(a => a.Key == ExportHelpers.PluginName).Value;
                        if (_driverPluginService.GetIdByName(pluginName?.ToString()) == null)
                        {
                            //找不到对应的插件
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add((row++, false, $"{ExportHelpers.PluginName}不存在"));
                            return Task.CompletedTask;
                        }
                        #endregion
                        //插件ID、设备ID都需要手动补录
                        device.PluginId = _driverPluginService.GetIdByName(pluginName?.ToString()).ToLong();
                        device.Id = this.GetIdByName(device.Name) ?? YitIdHelper.NextId();

                        devices.Add(device);
                        importPreviewOutput.Results.Add((row++, true, "成功"));
                        return Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add((row++, false, ex.Message));
                        return Task.CompletedTask;
                    }
                });
                importPreviewOutput.Data = devices;

            }
            #endregion
            else
            {
                int row = 1;
                ImportPreviewOutput<string> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                //插件属性需加上前置名称
                var newName = ExportHelpers.PluginLeftName + sheetName;
                var pluginId = _driverPluginService.GetIdByName(newName);
                if (pluginId == null)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add((row++, false, $"插件{newName}不存在"));

                    continue;
                }

                var driverPlugin = _driverPluginService.GetDriverPluginById(pluginId.Value);
                using var serviceScope = _scopeFactory.CreateScope();
                var pluginSingletonService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
                var driver = (DriverBase)pluginSingletonService.GetDriver(YitIdHelper.NextId(), driverPlugin);
                var propertys = driver.DriverPropertys.GetType().GetAllProps().Where(a => a.GetCustomAttribute<DevicePropertyAttribute>() != null);
                await rows.ForeachAsync(item =>
            {
                try
                {


                    List<DependencyProperty> devices = new List<DependencyProperty>();
                    foreach (var item1 in item)
                    {
                        var propertyInfo = propertys.FirstOrDefault(p => p.FindDisplayAttribute(a => a.GetCustomAttribute<DevicePropertyAttribute>()?.Description) == item1.Key);
                        if (propertyInfo == null)
                        {
                            //不存在时不报错
                        }
                        else
                        {
                            devices.Add(new()
                            {
                                PropertyName = propertyInfo.Name,
                                Description = item1.Key.ToString(),
                                Value = item1.Value?.ToString()
                            });
                        }

                    }
                    //转化设备名称
                    var deviceName = item.FirstOrDefault(a => a.Key == ExportHelpers.DeviceName).Value;
                    if (deviceImportPreview.Data?.Any(it => it.Name == deviceName.ToString()) == true)
                    {
                        var deviceId = this.GetIdByName(deviceName.ToString()) ?? deviceImportPreview.Data.FirstOrDefault(it => it.Name == deviceName.ToString()).Id;
                        deviceImportPreview.Data.FirstOrDefault(a => a.Id == deviceId).DevicePropertys = devices;
                    }
                    importPreviewOutput.Data.Add(string.Empty);
                    importPreviewOutput.Results.Add((row++, true, "成功"));

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add((row++, false, ex.Message));
                    return Task.CompletedTask;
                }
            });

            }
        }



        return ImportPreviews;
    }

    /// <inheritdoc/>
    [OperDesc("导入上传设备表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var collectDevices = new List<UploadDevice>();
        foreach (var item in input)
        {
            if (item.Key == ExportHelpers.UploadDeviceSheetName)
            {
                var collectDeviceImports = ((ImportPreviewOutput<UploadDevice>)item.Value).Data;
                collectDevices = collectDeviceImports.Adapt<List<UploadDevice>>();
                break;
            }
        }
        await Context.Storageable(collectDevices).ExecuteCommandAsync();
        _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除

    }

    #endregion
}