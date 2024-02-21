//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

using MiniExcelLibs;

using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <inheritdoc cref="IVariableService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class VariableService : DbRepository<Variable>, IVariableService
{
    protected readonly IFileService _fileService;
    protected readonly IServiceScope _serviceScope;
    protected readonly IImportExportService _importExportService;

    /// <inheritdoc cref="IVariableService"/>
    public VariableService(
    IServiceScopeFactory serviceScopeFactory,
    IFileService fileService,
    IImportExportService importExportService
        )
    {
        _fileService = fileService;
        _serviceScope = serviceScopeFactory.CreateScope();
        _importExportService = importExportService;
    }

    [OperDesc("添加变量")]
    public Task AddAsync(VariableAddInput input)
    {
        return InsertAsync(input);//添加数据
    }

    [OperDesc("复制变量", IsRecordPar = false)]
    public Task CopyAsync(IEnumerable<Variable> input, int count)
    {
        List<Variable> newDevs = new();

        for (int i = 0; i < count; i++)
        {
            var newDev = input.Adapt<List<Variable>>();

            newDev.ForEach(a =>
            {
                a.Id = YitIdHelper.NextId();
                a.Name = $"{Regex.Replace(a.Name, @"\d", "")}{a.Id}";
            });
            newDevs.AddRange(newDev);
        }

        return InsertRangeAsync(newDevs);//添加数据
    }

    [OperDesc("删除变量")]
    public Task DeleteAsync(List<BaseIdInput> input)
    {
        var ids = input.Select(a => a.Id).ToList();
        return Context.Deleteable<Variable>().Where(a => ids.Contains(a.Id)).ExecuteCommandAsync();
    }

    [OperDesc("删除变量")]
    public Task DeleteByDeviceIdAsync(List<long> input)
    {
        return Context.Deleteable<Variable>().Where(a => input.Contains(a.DeviceId)).ExecuteCommandAsync();
    }

    [OperDesc("编辑变量")]
    public Task EditAsync(VariableEditInput input)
    {
        return Context.Updateable(input.Adapt<Variable>()).ExecuteCommandAsync();//修改数据
    }

    public Task<SqlSugarPagedList<Variable>> PageAsync(VariablePageInput input)
    {
        var query = GetPage(input);
        return query.ToPagedListAsync(input.Current, input.Size);//分页
    }

    /// <inheritdoc/>
    private ISugarQueryable<Variable> GetPage(VariablePageInput input)
    {
        ISugarQueryable<Variable> query = Context.Queryable<Variable>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(!string.IsNullOrEmpty(input.RegisterAddress), u => u.RegisterAddress.Contains(input.RegisterAddress))
         .WhereIF(input.DeviceId > 0, u => u.DeviceId == input.DeviceId)
         .WhereIF(input.BusinessDeviceId > 0, u => SqlFunc.JsonLike(u.VariablePropertys, input.BusinessDeviceId.ToString()));

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    /// <inheritdoc/>
    [OperDesc("添加变量")]
    public Task AddBatchAsync(List<VariableAddInput> input)
    {
        return InsertRangeAsync(input.Adapt<List<Variable>>());
    }

    /// <inheritdoc/>
    [OperDesc("清空变量")]
    public Task ClearAsync()
    {
        return Context.Deleteable<Variable>().ExecuteCommandAsync();
    }

    public async Task<List<VariableRunTime>> GetVariableRuntimeAsync(long? devId = null)
    {
        if (devId == null)
        {
            var deviceVariables = await GetListAsync(a => a.DeviceId > 0 && a.Enable);
            var runtime = deviceVariables.Adapt<List<VariableRunTime>>();
            return runtime;
        }
        else
        {
            var deviceVariables = await GetListAsync(a => a.DeviceId == devId && a.Enable);
            var runtime = deviceVariables.Adapt<List<VariableRunTime>>();
            return runtime;
        }
    }

    #region 导出

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <returns></returns>
    [OperDesc("导出变量", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync()
    {
        //导出
        var data = (await GetListAsync());
        return await Export(data);
    }

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [OperDesc("导出设备", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(VariableInput input)
    {
        var data = (await GetPage(input.Adapt<VariablePageInput>()).ExportIgnoreColumns().ToListAsync());
        return await Export(data);
    }

    private async Task<FileStreamResult> Export(IEnumerable<Variable> data)
    {
        var fileName = "Variable";
        Dictionary<string, object> sheets = ExportCore(data);
        return await _importExportService.ExportAsync<Variable>(sheets, fileName, false);
    }

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [OperDesc("导出设备", IsRecordPar = false)]
    public async Task<MemoryStream> ExportMemoryStream(IEnumerable<Variable> data, string deviceName = null)
    {
        Dictionary<string, object> sheets = ExportCore(data, deviceName);

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        return memoryStream;
    }

    private Dictionary<string, object> ExportCore(IEnumerable<Variable> data, string deviceName = null)
    {
        if (data == null || !data.Any())
        {
            data = new List<Variable>();
        }
        var deviceDicts = _serviceScope.ServiceProvider.GetService<IDeviceService>().GetCacheList().ToDictionary(a => a.Id);
        var driverPluginDicts = _serviceScope.ServiceProvider.GetService<IPluginService>().GetList(PluginTypeEnum.Business).SelectMany(a => a.Children).ToDictionary(a => a.FullName);
        //总数据
        Dictionary<string, object> sheets = new();
        //变量页
        List<Dictionary<string, object>> variableExports = new();
        //变量附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();

        #region 列名称

        var type = typeof(Variable);
        var propertyInfos = type.GetProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null).OrderBy(
           a =>
           {
               return a.GetCustomAttribute<DataTableAttribute>()?.Order ?? 999999;
           }
           );

        #endregion 列名称

        foreach (var variable in data)
        {
            Dictionary<string, object> varExport = new();
            deviceDicts.TryGetValue(variable.DeviceId, out var device);

            //设备实体没有包含设备名称，手动插入
            varExport.Add(ExportConst.DeviceName, device?.Name ?? deviceName);
            foreach (var item in propertyInfos)
            {
                //描述
                var desc = item.FindDisplayAttribute();
                //数据源增加
                varExport.Add(desc ?? item.Name, item.GetValue(variable)?.ToString());
            }

            //添加完整设备信息
            variableExports.Add(varExport);

            #region 插件sheet

            foreach (var item in variable.VariablePropertys ?? new())
            {
                //插件属性
                //单个设备的行数据
                Dictionary<string, object> driverInfo = new();
                var has = deviceDicts.TryGetValue(item.Key, out var businessDevice);
                if (!has)
                    continue;
                //没有包含设备名称，手动插入
                driverInfo.Add(ExportConst.BusinessDeviceSheetName, businessDevice.Name);
                driverInfo.Add(ExportConst.VariableName, variable.Name);

                foreach (var item1 in item.Value)
                {
                    //添加对应属性数据
                    driverInfo.Add(item1.Description, item1.Value);
                }

                if (!driverPluginDicts.ContainsKey(businessDevice.PluginName))
                    continue;

                var pluginName = businessDevice.PluginName.GetFileNameAndTypeName();
                lock (devicePropertys)
                {
                    if (devicePropertys.ContainsKey(pluginName.Item2))
                    {
                        if (driverInfo.Count > 0)
                            devicePropertys[pluginName.Item2].Add(driverInfo);
                    }
                    else
                    {
                        if (driverInfo.Count > 0)
                            devicePropertys.TryAdd(pluginName.Item2, new() { driverInfo });
                    }
                }
            }

            #endregion 插件sheet
        }
        //添加设备页
        sheets.Add(ExportConst.VariableName, variableExports);

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

        return sheets;
    }

    #endregion 导出

    #region 导入

    /// <inheritdoc/>
    [OperDesc("导入设备表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var variables = new List<Variable>();
        foreach (var item in input)
        {
            if (item.Key == ExportConst.VariableName)
            {
                var variableImports = ((ImportPreviewOutput<Variable>)item.Value).Data;
                variables = new List<Variable>(variableImports.Values);
                break;
            }
        }
        var upData = variables.Where(a => a.IsUp).ToList();
        var insertData = variables.Where(a => !a.IsUp).ToList();
        await Context.Fastest<Variable>().PageSize(100000).BulkCopyAsync(insertData);
        await Context.Fastest<Variable>().PageSize(100000).BulkUpdateAsync(upData);
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await _importExportService.UploadFileAsync(browserFile);
        try
        {
            var sheetNames = MiniExcel.GetSheetNames(path);
            var deviceService = _serviceScope.ServiceProvider.GetService<IDeviceService>();
            var deviceDicts = deviceService.GetCacheList().ToDictionary(a => a.Name);

            var dbVariables = await Context.Queryable<Variable>().Select(it => new { it.Id, it.Name }).ToListAsync();
            //转为字典，提高查找效率
            var dbVariableDicts = dbVariables.ToDictionary(a => a.Name);

            //导入检验结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            //设备页
            ImportPreviewOutput<Variable> deviceImportPreview = new();
            var pluginService = _serviceScope.ServiceProvider.GetService<IPluginService>();
            var driverPluginFullNameDict = pluginService.GetList().SelectMany(a => a.Children).ToDictionary(a => a.FullName);
            var driverPluginNameDict = pluginService.GetList().SelectMany(a => a.Children).ToDictionary(a => a.Name);

            foreach (var sheetName in sheetNames)
            {
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();

                #region 变量sheet

                if (sheetName == ExportConst.VariableName)
                {
                    int row = 0;
                    ImportPreviewOutput<Variable> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);
                    deviceImportPreview = importPreviewOutput;

                    //线程安全
                    var variables = new ConcurrentList<Variable>();
                    //并行注意线程安全
                    rows.ParallelForEach(item =>
                    {
                        try
                        {
                            var variable = ((ExpandoObject)item).ConvertToEntity<Variable>(true);

                            //转化设备名称
                            item.TryGetValue(ExportConst.DeviceName, out var value);
                            var deviceName = value?.ToString();
                            deviceDicts.TryGetValue(deviceName, out var device);
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

                #endregion 变量sheet

                else
                {
                    int row = 1;
                    ImportPreviewOutput<string> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);

                    _ = driverPluginNameDict.TryGetValue(sheetName, out var driverPluginType);
                    try
                    {
                        if (driverPluginType == null)
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), false, $"插件{sheetName}不存在"));
                            continue;
                        }
                        var proprtys = driverPluginType.FullName;
                        var propertys = pluginService.GetVariablePropertyTypes(driverPluginType.FullName);
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
                                            Name = propertyInfo.Name,
                                            Description = keyValuePair.Key.ToString(),
                                            Value = keyValuePair.Value?.ToString()
                                        });
                                    }
                                }
                                //转化插件名称
                                item.TryGetValue(ExportConst.VariableName, out var variableNameObj);
                                item.TryGetValue(ExportConst.BusinessDeviceSheetName, out var businessDevName);
                                var variableName = variableNameObj?.ToString();

                                if (businessDevName != null)
                                {
                                    deviceDicts.TryGetValue(businessDevName.ToString(), out var uploadDevice);

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
                                            importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{businessDevName}不存在"));
                                        }
                                    }
                                    else
                                    {
                                        importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{businessDevName}不存在"));
                                    }
                                }
                                else
                                {
                                    importPreviewOutput.Results.Add((Interlocked.Add(ref row, 1), true, $"上传设备{businessDevName}不存在"));
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
        finally
        {
            FileUtility.Delete(path);
        }
    }

    #endregion 导入
}