#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.FriendlyException;
using Furion.LinqBuilder;

using Microsoft.AspNetCore.Components.Forms;

using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <inheritdoc cref="IVariableService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class VariableService : DbRepository<DeviceVariable>, IVariableService
{
    private readonly SysCacheService _sysCacheService;
    private readonly ICollectDeviceService _collectDeviceService;
    private readonly IUploadDeviceService _uploadDeviceService;
    private readonly IDriverPluginService _driverPluginService;
    private readonly FileService _fileService;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <inheritdoc cref="IVariableService"/>
    public VariableService(SysCacheService sysCacheService,
        ICollectDeviceService collectDeviceService, FileService fileService,
        IUploadDeviceService uploadDeviceService, IDriverPluginService driverPluginService,
    IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _sysCacheService = sysCacheService;
        _fileService = fileService;
        _collectDeviceService = collectDeviceService;
        _uploadDeviceService = uploadDeviceService;
        _driverPluginService = driverPluginService;
    }

    /// <inheritdoc/>
    [OperDesc("添加变量")]
    public async Task AddAsync(DeviceVariable input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0)
            throw Oops.Bah($"存在重复的名称:{input.Name}");
        var result = await InsertReturnEntityAsync(input);//添加数据
    }

    /// <inheritdoc/>
    public long GetIdByName(string name, bool onlyCache = false)
    {
        //先从Cache拿
        var id = _sysCacheService.Get<long>(ThingsGatewayCacheConst.Cache_DeviceVariableName, name);
        if (id == 0 && !onlyCache)
        {
            //单查获取对应ID
            id = Context.Queryable<DeviceVariable>().Where(it => it.Name == name).Select(it => it.Id).First();
            if (id != 0)
            {
                //插入Cache
                _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableName, name, id);
            }
        }
        return id;
    }
    /// <inheritdoc/>
    public string GetNameById(long Id, bool onlyCache = true)
    {
        //先从Cache拿
        var name = _sysCacheService.Get<string>(ThingsGatewayCacheConst.Cache_DeviceVariableId, Id.ToString());
        if (name.IsNullOrEmpty() && !onlyCache)
        {
            //单查获取用户账号对应ID
            name = Context.Queryable<DeviceVariable>().Where(it => it.Id == Id).Select(it => it.Name).First();
            if (!name.IsNullOrEmpty())
            {
                //插入Cache
                _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableId, Id.ToString(), name);
            }
        }
        return name;
    }


    /// <inheritdoc/>
    [OperDesc("删除变量")]
    public async Task DeleteAsync(List<BaseIdInput> input)
    {
        //获取所有ID
        var ids = input.Select(it => it.Id).ToList();
        if (ids.Count > 0)
        {
            var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
            if (result)
            {
                DeleteVariableFromCache(ids);
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("清空设备变量")]
    public async Task ClearDeviceVariableAsync()
    {
        var result = await Context.Deleteable<DeviceVariable>(a => a.IsMemoryVariable != true || a.IsMemoryVariable == null).ExecuteCommandAsync() > 0;
        if (result)
        {
            DeleteVariableFromCache(null);
        }
    }
    /// <inheritdoc/>
    [OperDesc("清空中间变量")]
    public async Task ClearMemoryVariableAsync()
    {
        var result = await Context.Deleteable<DeviceVariable>(a => a.IsMemoryVariable == true).ExecuteCommandAsync() > 0;
        if (result)
        {
            DeleteVariableFromCache(null);
        }
    }

    /// <inheritdoc />
    public void DeleteVariableFromCache(long id)
    {
        DeleteVariableFromCache(new List<long> { id });
    }

    /// <inheritdoc />
    public void DeleteVariableFromCache(List<long> ids = null)
    {
        _sysCacheService.RemoveByPrefixKey(ThingsGatewayCacheConst.Cache_DeviceVariableGroup);
        if (ids == null)
        {
            _sysCacheService.RemoveByPrefixKey(ThingsGatewayCacheConst.Cache_DeviceVariableId);
            _sysCacheService.RemoveByPrefixKey(ThingsGatewayCacheConst.Cache_DeviceVariableName);
            return;
        }
        var variableIds = ids.Select(it => it.ToString()).ToArray();//id转string列表
        foreach (var item in variableIds)
        {
            string name = _sysCacheService.Get<string>(ThingsGatewayCacheConst.Cache_DeviceVariableId, item);
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_DeviceVariableId, item);
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_DeviceVariableName, name);
        }
    }

    /// <inheritdoc/>
    [OperDesc("编辑变量")]
    public async Task EditAsync(DeviceVariable input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0 && account_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}");

        if (await Context.Updateable(input).ExecuteCommandAsync() > 0)//修改数据
            DeleteVariableFromCache(input.Id);
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
         .Where(u => u.IsMemoryVariable == input.IsMemoryVariable || (u.IsMemoryVariable == null && input.IsMemoryVariable != true))
         .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
         .OrderBy(u => u.Id)//排序
         .Select((u) => new DeviceVariable { Id = u.Id.SelectAll() })
            ;

        return query;
    }


    /// <inheritdoc/>
    public async Task<List<DeviceVariableRunTime>> GetDeviceVariableRuntimeAsync(long devId = 0)
    {
        if (devId == 0)
        {
            var deviceVariables = await GetListAsync(a => a.IsMemoryVariable != true || a.IsMemoryVariable == null);
            var runtime = deviceVariables.Adapt<List<DeviceVariableRunTime>>();
            return runtime;
        }
        else
        {
            var deviceVariables = await GetListAsync(a => a.DeviceId == devId && (a.IsMemoryVariable != true || a.IsMemoryVariable == null));
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
    public async Task<MemoryStream> ExportFileAsync(VariablePageInput input)
    {
        var query = GetPage(input);
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
    [OperDesc("导出变量表", IsRecordPar = false)]
    public async Task<MemoryStream> MemoryVariableExportFileAsync(List<MemoryVariable> devDatas = null)
    {
        devDatas ??= (await GetListAsync(a => a.IsMemoryVariable == true)).Adapt<List<MemoryVariable>>();

        //总数据
        Dictionary<string, object> sheets = new Dictionary<string, object>();
        //变量页
        List<Dictionary<string, object>> devExports = new();
        //变量附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();
        await devDatas.ForeachAsync(devData =>
        {
            #region 变量sheet
            //变量页
            var data = devData.GetType().GetAllProps().Where(a => a.GetCustomAttribute<ExcelAttribute>() != null);
            Dictionary<string, object> devExport = new();

            foreach (var item in data)
            {
                //描述
                var desc = ObjectExtensions.FindDisplayAttribute(item);
                //数据源增加
                devExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
            }


            //添加完整变量信息
            devExports.Add(devExport);

            #endregion

            return Task.CompletedTask;
        });

        //添加设备页
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
    [OperDesc("导出变量表", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<DeviceVariable> devDatas = null)
    {
        devDatas ??= await GetListAsync(a => a.IsMemoryVariable != true || a.IsMemoryVariable == null);

        //总数据
        Dictionary<string, object> sheets = new Dictionary<string, object>();
        //变量页
        List<Dictionary<string, object>> devExports = new();
        //变量附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();
        await devDatas.ForeachAsync(devData =>
    {
        #region 变量sheet
        //变量页
        var data = devData.GetType().GetAllProps().Where(a => a.GetCustomAttribute<ExcelAttribute>() != null);
        Dictionary<string, object> devExport = new();
        //变量实体没有包含设备名称，手动插入
        devExport.Add(ExportHelpers.DeviceName, _collectDeviceService.GetNameById(devData.DeviceId));

        foreach (var item in data)
        {
            //描述
            var desc = ObjectExtensions.FindDisplayAttribute(item);
            //数据源增加
            devExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
        }


        //添加完整变量信息
        devExports.Add(devExport);

        #endregion
        #region 上传插件属性
        foreach (var item in devData.VariablePropertys ?? new())
        {
            //插件属性
            //单个设备的行数据
            Dictionary<string, object> driverInfo = new Dictionary<string, object>();
            var uploadDevice = _uploadDeviceService.GetDeviceById(item.Key);
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
                var pluginName = _driverPluginService.GetNameById(uploadDevice.PluginId).Replace(ExportHelpers.PluginLeftName, "");
                if (devicePropertys.ContainsKey(pluginName))
                {
                    devicePropertys[pluginName].Add(driverInfo);
                }
                else
                {
                    devicePropertys.Add(pluginName, new() { driverInfo });
                }
            }


        }
        #endregion

        return Task.CompletedTask;
    });

        //添加设备页
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

        var deviceVariables = await GetListAsync();
        foreach (var item in deviceVariables)
        {
            _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableName, item.Name, item.Id);
            _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableId, item.Id.ToString(), item.Name);
        }

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


                List<DeviceVariable> devices = new List<DeviceVariable>();
                await rows.ForeachAsync(item =>
                {
                    try
                    {
                        var device = ((ExpandoObject)item).ConvertToEntity<DeviceVariable>(true);

                        //var hasDup = rows.HasDuplicateElements<DeviceVariable>(nameof(DeviceVariable.Name), device.Name);
                        //var hasName = GetIdByName(device.Name) > 0;
                        //if (hasDup || hasName)
                        //{
                        //    importPreviewOutput.HasError = true;
                        //    importPreviewOutput.Results.Add((false, "名称重复"));
                        //    return Task.CompletedTask;
                        //}
                        //变量ID都需要手动补录
                        devices.Add(device);
                        device.Id = this.GetIdByName(device.Name, true) == 0 ? YitIdHelper.NextId() : this.GetIdByName(device.Name, true);
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

        var deviceVariables = await GetListAsync();
        foreach (var item in deviceVariables)
        {
            _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableName, item.Name, item.Id);
            _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableId, item.Id.ToString(), item.Name);
        }

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


                List<DeviceVariable> devices = new List<DeviceVariable>();
                await rows.ForeachAsync(item =>
                {
                    try
                    {
                        var device = ((ExpandoObject)item).ConvertToEntity<DeviceVariable>(true);

                        //var hasDup = rows.HasDuplicateElements<DeviceVariable>(nameof(DeviceVariable.Name), device.Name);
                        //var hasName = GetIdByName(device.Name) > 0;
                        //if (hasDup || hasName)
                        //{
                        //    importPreviewOutput.HasError = true;
                        //    importPreviewOutput.Results.Add((false, "名称重复"));
                        //    return Task.CompletedTask;
                        //}
                        //转化设备名称
                        var deviceName = item.FirstOrDefault(a => a.Key == ExportHelpers.DeviceName).Value;
                        if (_collectDeviceService.GetIdByName(deviceName?.ToString()) == null)
                        {
                            //找不到对应的设备
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add((row++, false, $"{deviceName}设备不存在"));
                            return Task.CompletedTask;
                        }
                        else
                        {
                            //变量ID和设备ID都需要手动补录
                            device.DeviceId = _collectDeviceService.GetIdByName(deviceName.ToString()).ToLong();
                            device.Id = this.GetIdByName(device.Name, true) == 0 ? YitIdHelper.NextId() : this.GetIdByName(device.Name, true);
                        }

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
            else
            {
                int row = 1;
                ImportPreviewOutput<string> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);

                //插件属性需加上前置名称
                var newName = ExportHelpers.PluginLeftName + sheetName;
                var pluginId = _driverPluginService.GetIdByName(newName);

                try
                {
                    if (pluginId == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add((row++, false, $"插件{newName}不存在"));
                        continue;
                    }

                    var driverPlugin = _driverPluginService.GetDriverPluginById(pluginId.Value);
                    using var serviceScope = _scopeFactory.CreateScope();
                    var pluginSingletonService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
                    var driver = (UpLoadBase)pluginSingletonService.GetDriver(YitIdHelper.NextId(), driverPlugin);
                    var propertys = driver.VariablePropertys.GetType().GetAllProps().Where(a => a.GetCustomAttribute<VariablePropertyAttribute>() != null);
                    await rows.ForeachAsync(item =>
                        {
                            try
                            {

                                List<DependencyProperty> devices = new List<DependencyProperty>();
                                foreach (var item1 in item)
                                {
                                    var propertyInfo = propertys.FirstOrDefault(p => p.FindDisplayAttribute(a => a.GetCustomAttribute<VariablePropertyAttribute>()?.Description) == item1.Key);
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
                                //转化插件名称
                                var variableName = item.FirstOrDefault(a => a.Key == ExportHelpers.DeviceVariableSheetName).Value?.ToString();
                                var uploadDevName = item.FirstOrDefault(a => a.Key == ExportHelpers.UploadDeviceSheetName).Value?.ToString();

                                var uploadDevice = _uploadDeviceService.GetCacheList().FirstOrDefault(a => a.Name == uploadDevName);

                                if (deviceImportPreview.Data?.Any(it => it.Name == variableName) == true && uploadDevice != null)
                                {
                                    var id = this.GetIdByName(variableName, true);
                                    var deviceId = id != 0 ? id : deviceImportPreview.Data.FirstOrDefault(it => it.Name == variableName).Id;
                                    deviceImportPreview?.Data?.FirstOrDefault(a => a.Id == deviceId)?.VariablePropertys?.AddOrUpdate(uploadDevice.Id, devices);
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
                catch (Exception ex)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add((row++, false, ex.Message));
                }
            }
        }



        return ImportPreviews;
    }

    /// <inheritdoc/>
    [OperDesc("导入变量表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var collectDevices = new List<DeviceVariable>();
        foreach (var item in input)
        {
            if (item.Key == ExportHelpers.DeviceVariableSheetName)
            {
                var collectDeviceImports = ((ImportPreviewOutput<DeviceVariable>)item.Value).Data;
                collectDevices = collectDeviceImports.Adapt<List<DeviceVariable>>();
                break;
            }
        }
        if (Context.CurrentConnectionConfig.DbType == DbType.Sqlite
                     || Context.CurrentConnectionConfig.DbType == DbType.SqlServer
                     || Context.CurrentConnectionConfig.DbType == DbType.MySql
                     || Context.CurrentConnectionConfig.DbType == DbType.PostgreSQL
                     )
        {
            //大量数据插入/更新
            var x = await Context.Storageable(collectDevices).ToStorageAsync();
            await x.BulkCopyAsync();//不存在插入
            await x.BulkUpdateAsync();//存在更新
        }
        else
        {
            //其他数据库使用普通插入/更新
            await Context.Storageable(collectDevices).ExecuteCommandAsync();
        }
        DeleteVariableFromCache();

    }



    #endregion

}