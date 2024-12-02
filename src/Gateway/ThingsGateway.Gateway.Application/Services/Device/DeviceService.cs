//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using MiniExcelLibs;

using SqlSugar;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Reflection;
using System.Text;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation.Extension.Dynamic;
using ThingsGateway.FriendlyException;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

internal sealed class DeviceService : BaseService<Device>, IDeviceService
{
    private readonly IChannelService _channelService;
    private readonly IPluginService _pluginService;
    private readonly IDispatchService<Device> _dispatchService;
    private ISysUserService _sysUserService;
    private ISysUserService SysUserService
    {
        get
        {
            if (_sysUserService == null)
            {
                _sysUserService = App.GetService<ISysUserService>();
            }
            return _sysUserService;
        }
    }
    public DeviceService(
    IDispatchService<Device> dispatchService
        )
    {
        _channelService = App.RootServices.GetRequiredService<IChannelService>();
        _pluginService = App.RootServices.GetRequiredService<IPluginService>();
        _dispatchService = dispatchService;
    }

    /// <inheritdoc/>
    [OperDesc("SaveDevice", localizerType: typeof(Device), isRecordPar: false)]
    public async Task<bool> BatchEditAsync(IEnumerable<Device> models, Device oldModel, Device model)
    {
        var differences = models.GetDiffProperty(oldModel, model);
        differences.Remove(nameof(Device.DevicePropertys));

        if (differences?.Count > 0)
        {
            using var db = GetDB();

            var result = (await db.Updateable(models.ToList()).UpdateColumns(differences.Select(a => a.Key).ToArray()).ExecuteCommandAsync().ConfigureAwait(false)) > 0;
            if (result)
                DeleteDeviceFromCache();
            return result;
        }
        else
        {
            return true;
        }
    }

    [OperDesc("ClearDevice", localizerType: typeof(Device))]
    public async Task ClearDeviceAsync(PluginTypeEnum pluginType)
    {
        var variableService = App.RootServices.GetRequiredService<IVariableService>();
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            var data = GetAll()
            .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
             .WhereIf(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
             .Where(a => a.PluginType == pluginType)
             .Select(a => a.Id).ToList();
            await db.Deleteable<Device>(data).ExecuteCommandAsync().ConfigureAwait(false);
            if (pluginType == PluginTypeEnum.Collect)
                await variableService.DeleteByDeviceIdAsync(data, db).ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            DeleteDeviceFromCache();
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    [OperDesc("DeleteDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task DeleteByChannelIdAsync(IEnumerable<long> ids, SqlSugarClient db)
    {
        var variableService = App.RootServices.GetRequiredService<IVariableService>();
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            var data = GetAll().Where(a => ids.Contains(a.ChannelId))
              .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
             .WhereIf(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .Select(a => a.Id).ToList();
            await db.Deleteable<Device>(data).ExecuteCommandAsync().ConfigureAwait(false);
            await variableService.DeleteByDeviceIdAsync(data, db).ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            DeleteDeviceFromCache();
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    [OperDesc("DeleteDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task<bool> DeleteDeviceAsync(IEnumerable<long> ids)
    {
        var variableService = App.RootServices.GetRequiredService<IVariableService>();

        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            await db.Deleteable<Device>().Where(a => ids.Contains(a.Id)).ExecuteCommandAsync().ConfigureAwait(false);
            await variableService.DeleteByDeviceIdAsync(ids, db).ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            DeleteDeviceFromCache();
            return true;
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc />
    public void DeleteDeviceFromCache()
    {
        App.CacheService.Remove(ThingsGatewayCacheConst.Cache_Device);//删除设备缓存
        _dispatchService.Dispatch(new());
    }

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public List<Device> GetAll()
    {
        var key = ThingsGatewayCacheConst.Cache_Device;
        var devices = App.CacheService.Get<List<Device>>(key);
        if (devices == null)
        {
            using var db = GetDB();
            devices = db.Queryable<Device>().ToList();
            App.CacheService.Set(key, devices);
        }
        return devices;
    }
    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public async Task<List<Device>> GetAllByOrgAsync()
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return GetAll()
              .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.CreateOrgId))
              .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId).ToList();
    }
    public async Task<List<DeviceRunTime>> GetBusinessDeviceRuntimeAsync(long? devId = null)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        if (devId == null)
        {
            var devices = GetAll().Where(a => a.Enable && a.PluginType == PluginTypeEnum.Business);
            var channels = _channelService.GetAll().Where(a => a.Enable);
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
            var device = GetAll().FirstOrDefault(a => a.Enable && a.PluginType == PluginTypeEnum.Business && a.Id == devId);
            if (device == null)
            {
                return new List<DeviceRunTime>() { };
            }
            var channels = _channelService.GetAll().Where(a => a.Enable);
            if (!channels.Select(a => a.Id).Contains(device.ChannelId))
            {
                return new List<DeviceRunTime>() { };
            }
            var runtime = device.Adapt<DeviceRunTime>();
            runtime.Channel = channels.FirstOrDefault(a => a.Id == runtime.ChannelId);
            return new List<DeviceRunTime>() { runtime };
        }
    }

    public async Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntimeAsync(long? devId = null)
    {
        if (devId == null)
        {
            var variableService = App.RootServices.GetRequiredService<IVariableService>();
            var devices = GetAll().Where(a => a.Enable && a.PluginType == PluginTypeEnum.Collect);
            var channels = _channelService.GetAll().Where(a => a.Enable);
            devices = devices.Where(a => channels.Select(a => a.Id).Contains(a.ChannelId));
            var runtime = devices.Adapt<List<CollectDeviceRunTime>>().ToDictionary(a => a.Id);
            var collectVariableRunTimes = await variableService.GetVariableRuntimeAsync().ConfigureAwait(false);
            runtime.Values.ParallelForEach(device =>
            {
                device.Channel = channels.FirstOrDefault(a => a.Id == device.ChannelId);

                device.VariableRunTimes = collectVariableRunTimes.Where(a => a.DeviceId == device.Id).ToDictionary(a => a.Name);
            });

            collectVariableRunTimes.ParallelForEach(variable =>
            {
                if (runtime.TryGetValue(variable.DeviceId.Value, out var device))
                {
                    variable.CollectDeviceRunTime = device;
                    variable.DeviceName = device.Name;
                }
            });
            return runtime.Values.ToList();
        }
        else
        {
            var device = GetAll().FirstOrDefault(a => a.Enable && a.PluginType == PluginTypeEnum.Collect && a.Id == devId);
            if (device == null)
            {
                return new List<CollectDeviceRunTime>() { };
            }
            var channels = _channelService.GetAll().Where(a => a.Enable);
            if (!channels.Select(a => a.Id).Contains(device.ChannelId))
            {
                return new List<CollectDeviceRunTime>() { };
            }

            var runtime = device.Adapt<CollectDeviceRunTime>();
            var variableService = App.RootServices.GetRequiredService<IVariableService>();
            var collectVariableRunTimes = await variableService.GetVariableRuntimeAsync(devId).ConfigureAwait(false);
            runtime.VariableRunTimes = collectVariableRunTimes.ToDictionary(a => a.Name);
            runtime.Channel = channels.FirstOrDefault(a => a.Id == runtime.ChannelId);

            collectVariableRunTimes.ParallelForEach(variable =>
            {
                variable.CollectDeviceRunTime = runtime;
                variable.DeviceName = runtime.Name;
            });
            return new List<CollectDeviceRunTime>() { runtime };
        }
    }

    public Device? GetDeviceById(long id)
    {
        var data = GetAll();
        return data?.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="pluginType">查询条件</param>
    /// <param name="filterKeyValueAction">查询条件</param>
    public async Task<QueryData<Device>> PageAsync(QueryPageOptions option, PluginTypeEnum pluginType, FilterKeyValueAction filterKeyValueAction = null)
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return await QueryAsync(option, a => a
         .WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText!)).Where(a => a.PluginType == pluginType)
         .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
         .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
       , filterKeyValueAction).ConfigureAwait(false);

    }

    /// <summary>
    /// 保存设备
    /// </summary>
    /// <param name="input">设备</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveDevice", localizerType: typeof(Device))]
    public async Task<bool> SaveDeviceAsync(Device input, ItemChangedType type)
    {
        CheckInput(input);

        if (type == ItemChangedType.Update)
            await SysUserService.CheckApiDataScopeAsync(input.CreateOrgId, input.CreateUserId).ConfigureAwait(false);

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {
            DeleteDeviceFromCache();
            return true;
        }
        return false;
    }

    private void CheckInput(Device input)
    {

        if (input.RedundantEnable && input.RedundantDeviceId == null)
        {
            throw Oops.Bah(Localizer["RedundantDeviceNotNull"]);
        }
    }

    #region API查询

    public async Task<SqlSugarPagedList<Device>> PageAsync(DevicePageInput input)
    {
        using var db = GetDB();
        var query = await GetPageAsync(db, input).ConfigureAwait(false);
        return await query.ToPagedListAsync(input.Current, input.Size).ConfigureAwait(false);//分页
    }

    /// <inheritdoc/>
    private async Task<ISugarQueryable<Device>> GetPageAsync(SqlSugarClient db, DevicePageInput input)
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        ISugarQueryable<Device> query = db.Queryable<Device>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
         .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
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

    #endregion API查询

    #region 导出

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <returns></returns>
    [OperDesc("ExportDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task<Dictionary<string, object>> ExportDeviceAsync(QueryPageOptions options, PluginTypeEnum pluginType, FilterKeyValueAction filterKeyValueAction = null)
    {
        //导出
        var data = await PageAsync(options, pluginType, filterKeyValueAction).ConfigureAwait(false);
        string fileName;
        Dictionary<string, object> sheets;
        ExportCore(data.Items, pluginType, out fileName, out sheets);
        return sheets;
    }

    /// <summary>
    /// 导出文件
    /// </summary>
    [OperDesc("ExportDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task<MemoryStream> ExportMemoryStream(IEnumerable<Device>? data, PluginTypeEnum pluginType, string channelName = null)
    {
        string fileName;
        Dictionary<string, object> sheets;
        ExportCore(data, pluginType, out fileName, out sheets, channelName);
        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets).ConfigureAwait(false);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    private void ExportCore(IEnumerable<Device>? data, PluginTypeEnum pluginType, out string fileName, out Dictionary<string, object> sheets, string channelName = null)
    {
        if (data == null || !data.Any())
        {
            data = new List<Device>();
        }
        fileName = pluginType == PluginTypeEnum.Collect ? "CollectDevice" : "BusinessDevice";
        var deviceDicts = GetAll().ToDictionary(a => a.Id);
        var channelDicts = _channelService.GetAll().ToDictionary(a => a.Id);
        //总数据
        sheets = new();
        //设备页
        List<Dictionary<string, object>> deviceExports = new();
        //设备附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();
        ConcurrentDictionary<string, (object, Dictionary<string, PropertyInfo>)> propertysDict = new();

        #region 列名称

        var type = typeof(Device);
        var propertyInfos = type.GetRuntimeProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null)
             .OrderBy(
            a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue; ;
                if (order < 0)
                {
                    order = order + 10000000;
                }
                else if (order == 0)
                {
                    order = 10000000;
                }
                return order;
            }
            )
            ;

        #endregion 列名称

        foreach (var device in data)
        {
            Dictionary<string, object> devExport = new();
            deviceDicts.TryGetValue(device.RedundantDeviceId ?? 0, out var redundantDevice);
            channelDicts.TryGetValue(device.ChannelId, out var channel);

            devExport.Add(ExportString.ChannelName, channel?.Name ?? channelName);

            foreach (var item in propertyInfos)
            {
                //描述
                var desc = type.GetPropertyDisplayName(item.Name);
                //数据源增加
                devExport.Add(desc ?? item.Name, item.GetValue(device)?.ToString());
            }

            //设备实体没有包含冗余设备名称，手动插入
            devExport.Add(ExportString.RedundantDeviceName, redundantDevice?.Name);

            //添加完整设备信息
            deviceExports.Add(devExport);

            #region 插件sheet

            //插件属性
            //单个设备的行数据
            Dictionary<string, object> driverInfo = new();

            var propDict = device.DevicePropertys;
            if (propertysDict.TryGetValue(device.PluginName, out var propertys))
            {
            }
            else
            {
                var driverProperties = _pluginService.GetDriver(device.PluginName).DriverProperties;
                propertys.Item1 = driverProperties;
                var driverPropertyType = driverProperties.GetType();
                propertys.Item2 = driverPropertyType.GetRuntimeProperties()
.Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null)
.ToDictionary(a => driverPropertyType.GetPropertyDisplayName(a.Name, a => a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description), a => a);

                propertysDict.TryAdd(device.PluginName, propertys);
            }

            if (propertys.Item2.Count > 0)
            {
                //没有包含设备名称，手动插入
                driverInfo.Add(ExportString.DeviceName, device.Name);
            }
            //根据插件的配置属性项生成列，从数据库中获取值或者获取属性默认值
            foreach (var item in propertys.Item2)
            {
                if (propDict.TryGetValue(item.Value.Name, out var dependencyProperty))
                {
                    driverInfo.Add(item.Key, dependencyProperty);
                }
                else
                {
                    //添加对应属性数据
                    driverInfo.Add(item.Key, ThingsGatewayStringConverter.Default.Serialize(null, item.Value.GetValue(propertys.Item1)));
                }
            }

            var pluginName = PluginServiceUtil.GetFileNameAndTypeName(device.PluginName);
            if (devicePropertys.ContainsKey(pluginName.TypeName))
            {
                if (driverInfo.Count > 0)
                    devicePropertys[pluginName.TypeName].Add(driverInfo);
            }
            else
            {
                if (driverInfo.Count > 0)
                    devicePropertys.Add(pluginName.TypeName, new() { driverInfo });
            }

            #endregion 插件sheet
        }
        //添加设备页
        sheets.Add(ExportString.DeviceName, deviceExports);

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
    [OperDesc("ImportDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task ImportDeviceAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var collectDevices = new List<Device>();
        foreach (var item in input)
        {
            if (item.Key == ExportString.DeviceName)
            {
                var collectDeviceImports = ((ImportPreviewOutput<Device>)item.Value).Data;
                collectDevices = new List<Device>(collectDeviceImports.Values);
                break;
            }
        }
        var upData = collectDevices.Where(a => a.IsUp).ToList();
        var insertData = collectDevices.Where(a => !a.IsUp).ToList();
        using var db = GetDB();
        await db.Fastest<Device>().PageSize(100000).BulkCopyAsync(insertData).ConfigureAwait(false);
        await db.Fastest<Device>().PageSize(100000).BulkUpdateAsync(upData).ConfigureAwait(false);
        DeleteDeviceFromCache();
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await browserFile.StorageLocal().ConfigureAwait(false); // 上传文件并获取文件路径

        try
        {
            var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
            // 获取 Excel 文件中所有工作表的名称
            var sheetNames = MiniExcel.GetSheetNames(path);

            // 获取所有设备，并将设备名称作为键构建设备字典
            var deviceDicts = GetAll().ToDictionary(a => a.Name);

            // 获取所有通道，并将通道名称作为键构建通道字典
            var channelDicts = _channelService.GetAll().ToDictionary(a => a.Name);

            // 导入检验结果的预览字典，键为名称，值为导入预览对象
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

            // 设备页的导入预览对象
            ImportPreviewOutput<Device> deviceImportPreview = new();

            // 获取所有驱动程序，并将驱动程序的完整名称作为键构建字典
            var driverPluginFullNameDict = _pluginService.GetList().ToDictionary(a => a.FullName);

            // 获取所有驱动程序，并将驱动程序名称作为键构建字典
            var driverPluginNameDict = _pluginService.GetList().DistinctBy(a => a.Name).ToDictionary(a => a.Name);
            ConcurrentDictionary<string, (Type, Dictionary<string, PropertyInfo>, Dictionary<string, PropertyInfo>)> propertysDict = new();
            foreach (var sheetName in sheetNames)
            {
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();

                #region 采集设备sheet

                if (sheetName == ExportString.DeviceName)
                {
                    // 初始化行数
                    int row = 1;

                    // 创建导入预览输出对象，并将其添加到导入预览集合中
                    ImportPreviewOutput<Device> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);

                    // 为设备导入预览对象赋值
                    deviceImportPreview = importPreviewOutput;

                    // 创建设备列表
                    List<Device> devices = new();
                    var type = typeof(Device);
                    // 获取目标类型的所有属性，并根据是否需要过滤 IgnoreExcelAttribute 进行筛选
                    var deviceProperties = type.GetRuntimeProperties().Where(a => (a.GetCustomAttribute<IgnoreExcelAttribute>() == null) && a.CanWrite)
                                                .ToDictionary(a => type.GetPropertyDisplayName(a.Name));

                    // 遍历每一行数据
                    rows.ForEach(item =>
                    {
                        try
                        {
                            // 尝试将导入的项转换为 Device 对象
                            var device = (item as ExpandoObject)?.ConvertToEntity<Device>(deviceProperties);

                            // 如果转换失败，则添加错误信息到导入预览结果并返回
                            if (device == null)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, Localizer["ImportNullError"]));
                                return;
                            }

                            // 转换冗余设备名称
                            var hasRedundant = item.TryGetValue(ExportString.RedundantDeviceName, out var redundantObj);
                            var hasChannel = item.TryGetValue(ExportString.ChannelName, out var channelObj);

                            // 设备ID、冗余设备ID都需要手动补录
                            if (hasRedundant && redundantObj != null)
                            {
                                if (deviceDicts.TryGetValue(redundantObj.ToString(), out var redundantDevice))
                                    device.RedundantDeviceId = redundantDevice.Id;
                                else
                                {
                                    // 如果找不到对应的冗余设备，则添加错误信息到导入预览结果并返回
                                    importPreviewOutput.HasError = true;
                                    importPreviewOutput.Results.Add((row++, false, Localizer["RedundantDeviceError"]));
                                    return;
                                }
                            }
                            else
                            {
                                // 如果冗余设备未启用，则添加错误信息到导入预览结果并返回
                                if (device.RedundantEnable)
                                {
                                    importPreviewOutput.HasError = true;
                                    importPreviewOutput.Results.Add((row++, false, Localizer["RedundantDeviceError"]));
                                    return;
                                }
                            }

                            // 检查是否提供了通道信息，如果是，则尝试将其转换为通道对象并关联到设备
                            if (hasChannel && channelObj != null)
                            {
                                if (channelDicts.TryGetValue(channelObj.ToString(), out var channel))
                                    device.ChannelId = channel.Id;
                                else
                                {
                                    // 如果找不到对应的通道信息，则添加错误信息到导入预览结果并返回
                                    importPreviewOutput.HasError = true;
                                    importPreviewOutput.Results.Add((row++, false, Localizer["ChannelError"]));
                                    return;
                                }
                            }
                            else
                            {
                                // 如果未提供通道信息，则添加错误信息到导入预览结果并返回
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, Localizer["ChannelError"]));
                                return;
                            }

                            // 进行设备对象属性的验证
                            var validationContext = new ValidationContext(device);
                            var validationResults = new List<ValidationResult>();
                            validationContext.ValidateProperty(validationResults);

                            // 构建验证结果的错误信息
                            StringBuilder stringBuilder = new();
                            foreach (var validationResult in validationResults.Where(v => !string.IsNullOrEmpty(v.ErrorMessage)))
                            {
                                foreach (var memberName in validationResult.MemberNames)
                                {
                                    stringBuilder.Append(validationResult.ErrorMessage!);
                                }
                            }

                            // 如果有验证错误，则添加错误信息到导入预览结果并返回
                            if (stringBuilder.Length > 0)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, stringBuilder.ToString()));
                                return;
                            }

                            // 检查设备名称是否已存在于设备字典中，如果存在，则更新设备ID；否则，生成新的设备ID
                            if (deviceDicts.TryGetValue(device.Name, out var existingDevice))
                            {
                                device.Id = existingDevice.Id;
                                device.CreateOrgId = existingDevice.CreateOrgId;
                                device.CreateUserId = existingDevice.CreateUserId;
                                device.IsUp = true;
                            }
                            else
                            {
                                device.Id = CommonUtils.GetSingleId();
                                device.IsUp = false;
                                device.CreateOrgId = UserManager.OrgId;
                                device.CreateUserId = UserManager.UserId;
                            }

                            // 将设备添加到设备列表中，并添加成功信息到导入预览结果
                            if (device.IsUp && ((dataScope != null && dataScope?.Count > 0 && !dataScope.Contains(device.CreateOrgId)) || dataScope?.Count == 0 && device.CreateUserId != UserManager.UserId))
                            {
                                importPreviewOutput.Results.Add((row++, false, "Operation not permitted"));
                            }
                            else
                            {
                                devices.Add(device);
                                importPreviewOutput.Results.Add((row++, true, null));
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            // 捕获异常并添加错误信息到导入预览结果
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add((row++, false, ex.Message));
                            return;
                        }
                    });

                    // 将设备列表转换为字典，并赋值给导入预览输出对象的 Data 属性
                    importPreviewOutput.Data = devices.ToDictionary(a => a.Name);
                }

                #endregion 采集设备sheet

                else
                {
                    // 如果设备导入预览数据为空或者数量为0，则直接返回导入预览集合
                    if (deviceImportPreview.Data == null || deviceImportPreview.Data.Count == 0)
                        return ImportPreviews;

                    // 初始化行数
                    int row = 1;

                    // 创建导入预览输出对象
                    ImportPreviewOutput<string> importPreviewOutput = new();

                    // 将导入预览输出对象添加到导入预览集合中
                    ImportPreviews.Add(sheetName, importPreviewOutput);

                    // 插件属性需加上前置名称
                    _ = driverPluginNameDict.TryGetValue(sheetName, out var driverPluginType);

                    // 如果未找到驱动插件类型，则添加错误信息到导入预览结果并继续下一轮循环
                    if (driverPluginType == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add((row++, false, Localizer["NotNull", sheetName]));
                        continue;
                    }

                    if (propertysDict.TryGetValue(driverPluginType.FullName, out var propertys))
                    {
                    }
                    else
                    {
                        // 获取驱动插件实例
                        var driver = _pluginService.GetDriver(driverPluginType.FullName);
                        var type = driver.DriverProperties.GetType();

                        propertys.Item1 = type;

                        propertys.Item2 = type.GetRuntimeProperties()
                            .Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null && a.CanWrite)
                            .ToDictionary(a => type.GetPropertyDisplayName(a.Name, a => a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description));

                        // 获取目标类型的所有属性，并根据是否需要过滤 IgnoreExcelAttribute 进行筛选
                        var properties = propertys.Item1.GetRuntimeProperties().Where(a => (a.GetCustomAttribute<IgnoreExcelAttribute>() == null) && a.CanWrite)
                                        .ToDictionary(a => propertys.Item1.GetPropertyDisplayName(a.Name, a => a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description));

                        propertys.Item3 = properties;
                        propertysDict.TryAdd(driverPluginType.FullName, propertys);
                    }

                    // 遍历每一行数据
                    foreach (var item in rows)
                    {
                        try
                        {
                            // 获取设备名称
                            if (!item.TryGetValue(ExportString.DeviceName, out var deviceName))
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, Localizer["DeviceNotNull"]));
                                continue;
                            }

                            // 转化插件名称
                            var value = item[ExportString.DeviceName]?.ToString();

                            // 检查设备名称是否存在于设备导入预览数据中，如果不存在，则添加错误信息到导入预览结果并继续下一轮循环
                            var hasDevice = deviceImportPreview.Data.ContainsKey(value);
                            if (!hasDevice)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, Localizer["NotNull", value]));
                                continue;
                            }

                            // 尝试将导入的项转换为对象
                            var pluginProp = (item as ExpandoObject)?.ConvertToEntity(propertys.Item1, propertys.Item3);

                            // 如果转换失败，则添加错误信息到导入预览结果并返回
                            if (pluginProp == null)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, Localizer["ImportNullError"]));
                                return ImportPreviews;
                            }

                            // 检查属性的验证结果
                            var validationContext = new ValidationContext(pluginProp);
                            var validationResults = new List<ValidationResult>();
                            validationContext.ValidateProperty(validationResults);

                            // 构建验证结果的错误信息
                            StringBuilder stringBuilder = new();
                            foreach (var validationResult in validationResults.Where(v => !string.IsNullOrEmpty(v.ErrorMessage)))
                            {
                                foreach (var memberName in validationResult.MemberNames)
                                {
                                    stringBuilder.Append(validationResult.ErrorMessage!);
                                }
                            }

                            // 如果有验证错误，则添加错误信息到导入预览结果并返回
                            if (stringBuilder.Length > 0)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, stringBuilder.ToString()));
                                return ImportPreviews;
                            }

                            // 将动态属性映射到设备属性字典中
                            Dictionary<string, string> devices = new();
                            foreach (var keyValuePair in item)
                            {
                                if (propertys.Item2.TryGetValue(keyValuePair.Key, out var propertyInfo))
                                {
                                    devices.Add(propertyInfo.Name, keyValuePair.Value?.ToString());
                                }
                            }

                            // 更新设备导入预览数据中对应设备的属性信息，并添加成功信息到导入预览结果
                            deviceImportPreview.Data[value].DevicePropertys = devices;
                            importPreviewOutput.Results.Add((row++, true, null));
                            continue;
                        }
                        catch (Exception ex)
                        {
                            // 捕获异常并添加错误信息到导入预览结果并返回
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add((row++, false, ex.Message));
                            return ImportPreviews;
                        }
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
