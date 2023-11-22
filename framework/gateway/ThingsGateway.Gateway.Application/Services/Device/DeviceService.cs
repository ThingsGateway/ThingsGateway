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

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

using MiniExcelLibs;

using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Gateway.Core.Extensions;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

[Injection(Proxy = typeof(OperDispatchProxy))]
public abstract class DeviceService<T> : DbRepository<T> where T : Device, new()
{
    protected readonly IFileService _fileService;
    protected readonly IServiceScope _serviceScope;
    /// <inheritdoc cref="CollectDeviceService"/>
    public DeviceService(
    IServiceScopeFactory serviceScopeFactory,
    IFileService fileService
        )
    {
        _fileService = fileService;
        _serviceScope = serviceScopeFactory.CreateScope();
    }

    /// <inheritdoc/>
    [OperDesc("添加设备")]
    public async Task AddAsync(T input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0)
            throw Oops.Bah($"存在重复的名称:{input.Name}");
        await InsertAsync(input);//添加数据
        RemoveCache();
    }


    /// <inheritdoc/>
    [OperDesc("复制设备")]
    public async Task CopyDevAsync(IEnumerable<T> input)
    {
        var newDevs = input.Adapt<List<T>>();

        newDevs.ForEach(a =>
        {
            a.Id = YitIdHelper.NextId();
            a.Name = $"{Regex.Replace(a.Name, @"\d", "")}{a.Id}";
        });

        var result = await InsertRangeAsync(newDevs);//添加数据
        RemoveCache();

    }
    static string GetUniqueName(string name, List<string> existingNames)
    {
        if (existingNames.Contains(name))
        {
            int number;
            if (int.TryParse(name[^1].ToString(), out number))  // 判断最后一个字符是否是数字
            {
                return name[..^1] + (number + 1);  // 如果是数字结尾，数字加1
            }
            else
            {
                return name + "1";  // 如果不是数字结尾，直接在末尾加上"1"
            }
        }
        else
        {
            return name;  // 如果名称不重复，直接返回原名称
        }
    }
    /// <inheritdoc/>
    [OperDesc("删除设备")]
    public async Task DeleteAsync(params long[] input)
    {
        //获取所有ID
        if (input.Length > 0)
        {
            var result = await DeleteByIdsAsync(input.Cast<object>().ToArray());
            var variableService = _serviceScope.ServiceProvider.GetService<VariableService>();
            await Context.Deleteable<DeviceVariable>(it => input.Contains(it.DeviceId)).ExecuteCommandAsync();
            variableService.DeleteVariableFromCache();
            if (result)
            {
                RemoveCache();
            }
        }
    }

    /// <inheritdoc/>
    [OperDesc("编辑设备")]
    public async Task EditAsync(DeviceEditInput input)
    {
        var account_Id = GetIdByName(input.Name);
        if (account_Id > 0 && account_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}");

        if (await Context.Updateable(input.Adapt<T>()).ExecuteCommandAsync() > 0)//修改数据
            RemoveCache();
    }

    /// <inheritdoc/>
    public List<T> GetCacheList(bool isMapster)
    {
        //先从Cache拿
        var collectDevice = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<List<T>>(ThingsGatewayCacheConst.Cache_CollectDevice, isMapster);
        if (collectDevice == null)
        {
            collectDevice = Context.Queryable<T>().ToList();
            if (collectDevice != null)
            {
                //插入Cache
                _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(ThingsGatewayCacheConst.Cache_CollectDevice, collectDevice, isMapster);
            }
        }
        return collectDevice;
    }



    /// <inheritdoc/>
    public T GetDeviceById(long Id)
    {
        var data = GetCacheList(true);
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
    public List<DeviceTree> GetTree()
    {
        var data = GetCacheList(false);
        var trees = data.GetTree();
        return trees;
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<T>> PageAsync(DevicePageInput input)
    {
        var query = GetPage(input);
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    public virtual void RemoveCache()
    {
        _serviceScope.ServiceProvider.GetService<MemoryCache>().Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除
    }
    /// <inheritdoc/>
    private ISugarQueryable<T> GetPage(DevicePageInput input)
    {
        ISugarQueryable<T> query = Context.Queryable<T>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(!string.IsNullOrEmpty(input.DeviceGroup), u => u.DeviceGroup == input.DeviceGroup)
         .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginName == input.PluginName);
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }
    #region 导入导出
    /// <inheritdoc/>
    public async Task<MemoryStream> ExportFileAsync(DeviceInput input)
    {
        var query = GetPage(input.Adapt<DevicePageInput>());
        var data = await query.ToListAsync();
        return await ExportFileAsync(data);
    }

    /// <inheritdoc/>
    [OperDesc("导出采集设备表", IsRecordPar = false)]
    public async Task<MemoryStream> ExportFileAsync(List<T> devDatas = null)
    {
        devDatas ??= GetCacheList(false);

        //总数据
        Dictionary<string, object> sheets = new();
        //设备页
        List<Dictionary<string, object>> devExports = new();
        //设备附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();
        var deviceDicts = devDatas.ToDictionary(a => a.Id);
        foreach (var devData in devDatas)
        {
            #region 设备sheet
            //设备页
            var data = devData.GetType().GetProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null);
            Dictionary<string, object> devExport = new();
            foreach (var item in data)
            {
                //描述
                var desc = item.FindDisplayAttribute();
                //数据源增加
                devExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
            }
            deviceDicts.TryGetValue(devData.RedundantDeviceId, out var redundantDevice);

            ////设备实体没有包含插件名称，手动插入
            //devExport.Add(ExportHelpers.PluginName, devData.PluginName);
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
            var pluginName = devData.PluginName.GetFileNameAndTypeName();
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

            #endregion
        }

        //添加设备页
        sheets.Add(ExportHelpers.CollectDeviceSheetName, devExports);


        //添加插件属性页
        foreach (var item in devicePropertys)
        {
            HashSet<string> allKeys = new HashSet<string>();
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

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    /// <inheritdoc/>
    [OperDesc("导入采集设备表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var collectDevices = new List<T>();
        foreach (var item in input)
        {
            if (item.Key == ExportHelpers.CollectDeviceSheetName)
            {
                var collectDeviceImports = ((ImportPreviewOutput<T>)item.Value).Data;
                collectDevices = collectDeviceImports.Values.Adapt<List<T>>();
                break;
            }
        }
        await Context.Storageable(collectDevices).ExecuteCommandAsync();
        RemoveCache();

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
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(MemoryStream stream)
    {
        var sheetNames = MiniExcel.GetSheetNames(stream);
        var deviceDicts = GetCacheList(false).ToDictionary(a => a.Name);

        //导入检验结果
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
        //设备页
        ImportPreviewOutput<T> deviceImportPreview = new();
        var driverPluginService = _serviceScope.ServiceProvider.GetService<DriverPluginService>();
        var driverPluginFullNameDict = driverPluginService.GetAllDriverPlugin().SelectMany(a => a.Children).ToDictionary(a => a.FullName);
        var driverPluginNameDict = driverPluginService.GetAllDriverPlugin().SelectMany(a => a.Children).ToDictionary(a => a.Name);
        foreach (var sheetName in sheetNames)
        {
            //单页数据
            var rows = stream.Query(useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();
            #region 采集设备sheet
            if (sheetName == ExportHelpers.CollectDeviceSheetName)
            {
                int row = 1;
                ImportPreviewOutput<T> importPreviewOutput = new();
                ImportPreviews.Add(sheetName, importPreviewOutput);
                deviceImportPreview = importPreviewOutput;
                List<T> devices = new();

                rows.ForEach(item =>
                {
                    try
                    {
                        var device = ((ExpandoObject)item).ConvertToEntity<T>(true);
                        #region 特殊转化名称
                        ////转化插件名称
                        //var hasPlugin = item.TryGetValue(ExportHelpers.PluginName, out var pluginObj);

                        //if (pluginObj == null || !driverPluginFullNameDict.TryGetValue(pluginObj.ToString(), out var plugin))
                        //{
                        //    //找不到对应的插件
                        //    importPreviewOutput.HasError = true;
                        //    importPreviewOutput.Results.Add((row++, false, $"{ExportHelpers.PluginName}不存在"));
                        //    return;
                        //}
                        //转化冗余设备名称
                        var hasRedundant = item.TryGetValue(ExportHelpers.RedundantDeviceName, out var redundantObj);

                        #endregion
                        //设备ID、冗余设备ID都需要手动补录
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
                _ = driverPluginNameDict.TryGetValue(sheetName, out var driverPluginType);
                if (driverPluginType == null)
                {
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add((row++, false, $"插件{sheetName}不存在"));
                    continue;
                }

                var driver = driverPluginService.GetDriver(driverPluginType.FullName);
                var propertys = driver.DriverPropertys.GetType().GetProperties()
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


        return await Task.FromResult(ImportPreviews);
    }
    #endregion
}