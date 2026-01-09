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

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

using MiniExcelLibs;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using ThingsGateway.Common.Extension;
using ThingsGateway.Common.Extension.Generic;
using ThingsGateway.FriendlyException;

namespace ThingsGateway.Gateway.Application;

internal sealed class DeviceService : BaseService<Device>, IDeviceService
{
    private readonly IChannelService _channelService;
    private readonly IPluginService _pluginService;

    public DeviceService()
    {
        _channelService = App.RootServices.GetRequiredService<IChannelService>();
        _pluginService = App.RootServices.GetRequiredService<IPluginService>();
    }

    /// <inheritdoc/>
    [OperDesc("CopyDevice", localizerType: typeof(Device), isRecordPar: false)]
    public async Task<bool> CopyAsync(Dictionary<Device, List<Variable>> devices)
    {
        using var db = GetDB();

        //事务
        var result = await db.UseTranAsync(async () =>
        {
            var device = devices.Keys.ToList();
            ManageHelper.CheckDeviceCount(device.Count);
            foreach (var item in device)
            {
                item.RedundantEnable = false;
                item.RedundantDeviceId = null;
            }
            await db.Insertable(device).ExecuteCommandAsync().ConfigureAwait(false);

            var variable = devices.SelectMany(a => a.Value).ToList();
            ManageHelper.CheckVariableCount(variable.Count);

            await db.Insertable(variable).ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            return true;
        }
        else
        {
            //写日志
            throw new(result.Message, result.Exception);
        }
    }

    public async Task UpdateLogAsync(long channelId, TouchSocket.Core.LogLevel logLevel)
    {
        using var db = GetDB();

        //事务
        var result = await db.UseTranAsync(async () =>
        {
            //更新数据库

            await db.Updateable<Device>().SetColumns(it => new Device() { LogLevel = logLevel }).Where(a => a.Id == channelId).ExecuteCommandAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {

        }
        else
        {
            //写日志
            throw new(result.Message, result.Exception);
        }
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
            var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
            var data = models
                            .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
             .WhereIf(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
             .ToList();
            var result = (await db.Updateable(data).UpdateColumns(differences.Select(a => a.Key).ToArray()).ExecuteCommandAsync().ConfigureAwait(false)) > 0;

            return result;
        }
        else
        {
            return true;
        }
    }

    [OperDesc("DeleteDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task DeleteByChannelIdAsync(IEnumerable<long> ids, ISqlOrmClient db)
    {
        var IdhashSet = ids.ToHashSet();
        var variableService = App.RootServices.GetRequiredService<IVariableService>();
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            var data = GlobalData.IdDevices
                          .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
             .WhereIf(dataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId)
             .Where(a => IdhashSet.Contains(a.Value.ChannelId))
            .Select(a => a.Value.Id).ToList();
            await db.Deleteable<Device>(a => data.Contains(a.Id)).ExecuteCommandAsync().ConfigureAwait(false);
            await variableService.DeleteByDeviceIdAsync(data, db).ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {

        }
        else
        {
            //写日志
            throw new(result.Message, result.Exception);
        }
    }

    [OperDesc("DeleteDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task<bool> DeleteDeviceAsync(IEnumerable<long> ids)
    {
        var IdhashSet = ids.ToHashSet();
        var variableService = App.RootServices.GetRequiredService<IVariableService>();
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);

        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            await db.Deleteable<Device>().Where(a => IdhashSet.Contains(a.Id))
                          .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
             .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .ExecuteCommandAsync().ConfigureAwait(false);
            await variableService.DeleteByDeviceIdAsync(ids, db).ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {

            return true;
        }
        else
        {
            //写日志
            throw new(result.Message, result.Exception);
        }
    }

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public async Task<List<Device>> GetFromDBAsync(Expression<Func<Device, bool>> expression = null, ISqlOrmClient db = null)
    {
        db ??= GetDB();
        var devices = await db.Queryable<Device>().WhereIF(expression != null, expression).OrderBy(a => a.Id).ToListAsync().ConfigureAwait(false);

        return devices;
    }

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="exportFilter">查询条件</param>
    public async Task<QueryData<Device>> PageAsync(GatewayExportFilter exportFilter)
    {
        var whereQuery = await GetWhereQueryFunc(exportFilter).ConfigureAwait(false);

        return await QueryAsync(exportFilter.QueryPageOptions, whereQuery
       , exportFilter.FilterKeyValueAction).ConfigureAwait(false);
    }
    private async Task<Func<ISqlQueryable<Device>, ISqlQueryable<Device>>> GetWhereQueryFunc(GatewayExportFilter exportFilter)
    {
        HashSet<long>? channel = null;
        if (!exportFilter.PluginName.IsNullOrWhiteSpace())
        {
            channel = (GlobalData.IdChannels).Where(a => a.Value.PluginName == exportFilter.PluginName).Select(a => a.Value.Id).ToHashSet();
        }
        if (exportFilter.PluginType != null)
        {
            var pluginInfo = GlobalData.PluginService.GetPluginList(exportFilter.PluginType).Select(a => a.FullName).ToHashSet();
            channel = (GlobalData.IdChannels).Where(a => pluginInfo.Contains(a.Value.PluginName)).Select(a => a.Value.Id).ToHashSet();
        }
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        var whereQuery = (ISqlQueryable<Device> a) => a
     .WhereIF(channel != null, a => channel.Contains(a.ChannelId))
     .WhereIF(exportFilter.DeviceId != null, a => a.Id == exportFilter.DeviceId)
     .WhereIF(!exportFilter.QueryPageOptions.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(exportFilter.QueryPageOptions.SearchText!))
              .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
         .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId);
        return whereQuery;
    }

    private async Task<Func<IEnumerable<Device>, IEnumerable<Device>>> GetWhereEnumerableFunc(GatewayExportFilter exportFilter)
    {
        HashSet<long>? channel = null;
        if (!exportFilter.PluginName.IsNullOrWhiteSpace())
        {
            channel = (GlobalData.IdChannels).Where(a => a.Value.PluginName == exportFilter.PluginName).Select(a => a.Value.Id).ToHashSet();
        }
        if (exportFilter.PluginType != null)
        {
            var pluginInfo = GlobalData.PluginService.GetPluginList(exportFilter.PluginType).Select(a => a.FullName).ToHashSet();
            channel = (GlobalData.IdChannels).Where(a => pluginInfo.Contains(a.Value.PluginName)).Select(a => a.Value.Id).ToHashSet();
        }
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        var whereQuery = (IEnumerable<Device> a) => a
     .WhereIF(channel != null, a => channel.Contains(a.ChannelId))
     .WhereIF(exportFilter.DeviceId != null, a => a.Id == exportFilter.DeviceId)
     .WhereIF(!exportFilter.QueryPageOptions.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(exportFilter.QueryPageOptions.SearchText!))
              .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
         .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId);
        return whereQuery;
    }

    /// <summary>
    /// 保存设备
    /// </summary>
    /// <param name="input">设备</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveDevice", localizerType: typeof(Device))]
    public async Task<bool> SaveDeviceAsync(Device input, ItemChangedType type)
    {
        if (GlobalData.ReadOnlyDevices.TryGetValue(input.Name, out var device))
        {
            if (device.Id != input.Id)
            {
                throw Oops.Bah(Localizer["NameDump", device.Name]);
            }
        }
        if (type == ItemChangedType.Update)
            await GlobalData.SysUserService.CheckApiDataScopeAsync(input.CreateOrgId, input.CreateUserId).ConfigureAwait(false);
        else
            ManageHelper.CheckDeviceCount(1);

        if (input.RedundantEnable && GlobalData.IsRedundantEnable(input.RedundantDeviceId ?? 0))
            throw Oops.Bah($"Redundancy configuration error, backup device has been planned into another redundancy group");

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {

            return true;
        }
        return false;
    }

    [OperDesc("SaveDevice", localizerType: typeof(Device), isRecordPar: false)]
    public async Task<bool> BatchSaveDeviceAsync(List<Device> input, ItemChangedType type)
    {
        if (type == ItemChangedType.Update)
            await GlobalData.SysUserService.CheckApiDataScopeAsync(input.Select(a => a.CreateOrgId), input.Select(a => a.CreateUserId)).ConfigureAwait(false);
        else
            ManageHelper.CheckDeviceCount(input.Count);

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {

            return true;
        }
        return false;
    }

    #region 导出

    /// <summary>
    /// 导出文件
    /// </summary>
    /// <returns></returns>
    [OperDesc("ExportDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task<Dictionary<string, object>> ExportDeviceAsync(GatewayExportFilter exportFilter)
    {
        //导出
        var devices = await GetAsyncEnumerableData(exportFilter).ConfigureAwait(false);
        var plugins = await GetAsyncEnumerableData(exportFilter).ConfigureAwait(false);
        var devicesSql = await GetEnumerableData(exportFilter).ConfigureAwait(false);
        var deviceDicts = GlobalData.IdDevices;
        var channelDicts = GlobalData.IdChannels;
        var pluginSheetNames = (await devicesSql.Select(a => a.ChannelId).ToListAsync().ConfigureAwait(false)).Select(a =>
        {
            channelDicts.TryGetValue(a, out var channel);
            var pluginKey = channel?.PluginName;
            return pluginKey;
        }).ToHashSet();

        var sheets = DeviceServiceHelpers.ExportSheets(devices, plugins, deviceDicts, channelDicts, pluginSheetNames); // IEnumerable 延迟执行

        return sheets;
    }
    private async Task<IAsyncEnumerable<Device>> GetAsyncEnumerableData(GatewayExportFilter exportFilter)
    {
        var whereQuery = await GetEnumerableData(exportFilter).ConfigureAwait(false);
        return whereQuery.ToAsyncEnumerable();
    }
    private async Task<ISqlQueryable<Device>> GetEnumerableData(GatewayExportFilter exportFilter)
    {
        var db = GetDB();
        var whereQuery = await GetWhereQueryFunc(exportFilter).ConfigureAwait(false);

        return GetQuery(db, exportFilter.QueryPageOptions, whereQuery, exportFilter.FilterKeyValueAction);
    }

    /// <summary>
    /// 导出文件
    /// </summary>
    public Dictionary<string, object> ExportDictionary(List<Device>? models, string channelName = null, string plugin = null)
    {
        var deviceDicts = GlobalData.IdDevices;
        var channelDicts = GlobalData.IdChannels;
        var pluginSheetNames = models.Select(a => a.ChannelId).Select(a =>
        {
            channelDicts.TryGetValue(a, out var channel);
            var pluginKey = channel?.PluginName ?? plugin;
            return pluginKey;
        }).ToHashSet();

        var sheets = DeviceServiceHelpers.ExportSheets(models, deviceDicts, channelDicts, pluginSheetNames, channelName);
        return sheets;
    }

    #endregion 导出

    #region 导入

    /// <inheritdoc/>
    [OperDesc("ImportDevice", isRecordPar: false, localizerType: typeof(Device))]
    public Task<HashSet<long>> ImportDeviceAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {

        IEnumerable<Device> devices = new List<Device>();
        foreach (var item in input)
        {
            if (item.Key == GatewayExportString.DeviceName)
            {
                var deviceImports = ((ImportPreviewOutput<Device>)item.Value).Data;
                devices = deviceImports.Select(a => a.Value);
                break;
            }
        }
        var upData = devices.Where(a => a.IsUp).ToList();
        var insertData = devices.Where(a => !a.IsUp).ToList();
        return ImportDeviceAsync(upData, insertData);

    }
    /// <inheritdoc/>
    [OperDesc("ImportDevice", isRecordPar: false, localizerType: typeof(Device))]
    public async Task<HashSet<long>> ImportDeviceAsync(List<Device> upData, List<Device> insertData)
    {

        ManageHelper.CheckDeviceCount(insertData.Count);

        using var db = GetDB();
        if (GlobalData.HardwareJob.HardwareInfo.AvailableMemory < 2048)
        {
            await db.BulkCopyAsync(insertData, 10000).ConfigureAwait(false);
            await db.BulkUpdateAsync(upData, 10000).ConfigureAwait(false);
        }
        else
        {

            await db.BulkCopyAsync(insertData, 200000).ConfigureAwait(false);
            await db.BulkUpdateAsync(upData, 200000).ConfigureAwait(false);
        }

        return upData.Select(a => a.Id).Concat(insertData.Select(a => a.Id)).ToHashSet();
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await browserFile.StorageLocal().ConfigureAwait(false); // 上传文件并获取文件路径

        return await PreviewAsync(path).ConfigureAwait(false);

    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IFormFile browserFile)
    {
        var path = await browserFile.StorageLocal().ConfigureAwait(false); // 上传文件并获取文件路径

        return await PreviewAsync(path).ConfigureAwait(false);

    }
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(string path)
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        try
        {
            // 获取 Excel 文件中所有工作表的名称
            var sheetNames = MiniExcel.GetSheetNames(path);

            // 获取所有设备，并将设备名称作为键构建设备字典
            var deviceDicts = GlobalData.Devices;

            // 获取所有通道，并将通道名称作为键构建通道字典
            var channelDicts = GlobalData.Channels;

            // 导入检验结果的预览字典，键为名称，值为导入预览对象
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

            // 设备页的导入预览对象
            ImportPreviewOutput<Device> deviceImportPreview = new();

            // 获取所有驱动程序，并将驱动程序名称作为键构建字典
            var driverPluginNameDict = _pluginService.GetPluginList().DistinctBy(a => a.Name).ToDictionary(a => a.Name);
            NonBlockingDictionary<string, (Type, Dictionary<string, PropertyInfo>, Dictionary<string, PropertyInfo>)> propertysDict = new();
            foreach (var sheetName in sheetNames)
            {
#pragma warning disable CA1849
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();
#pragma warning restore CA1849

                SetDeviceData(dataScope, deviceDicts, channelDicts, ImportPreviews, ref deviceImportPreview, driverPluginNameDict, propertysDict, sheetName, rows);
            }

            return ImportPreviews;
        }
        finally
        {
            FileHelper.DeleteFile(path);
        }

    }
    public void SetDeviceData(HashSet<long>? dataScope, IReadOnlyDictionary<string, DeviceRuntime> deviceDicts, IReadOnlyDictionary<string, ChannelRuntime> channelDicts, Dictionary<string, ImportPreviewOutputBase> ImportPreviews, ref ImportPreviewOutput<Device> deviceImportPreview, Dictionary<string, PluginInfo> driverPluginNameDict, NonBlockingDictionary<string, (Type, Dictionary<string, PropertyInfo>, Dictionary<string, PropertyInfo>)> propertysDict, string sheetName, IEnumerable<IDictionary<string, object>> rows)
    {
        #region 采集设备sheet
        string ImportNullError = Localizer["ImportNullError"];
        string RedundantDeviceError = Localizer["RedundantDeviceError"];
        string ChannelError = Localizer["ChannelError"];

        string PluginNotNull = Localizer["PluginNotNull"];
        string DeviceNotNull = Localizer["DeviceNotNull"];

        if (sheetName == GatewayExportString.DeviceName)
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
                                        .ToDictionary(a => type.GetPropertyDisplayName(a.Name), a => (a, TouchSocket.Core.ReflectionExtension.IsNullableType(a)));



            // 遍历每一行数据
            LinqHelper.ForEach(rows, item =>
            {
                try
                {
                    // 尝试将导入的项转换为 Device 对象
                    var device = item.ConvertToEntity<Device>(deviceProperties);

                    // 如果转换失败，则添加错误信息到导入预览结果并返回
                    if (device == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ImportNullError));
                        return;
                    }

                    // 转换冗余设备名称
                    var hasRedundant = item.TryGetValue(GatewayExportString.RedundantDeviceName, out var redundantObj);
                    var hasChannel = item.TryGetValue(GatewayExportString.ChannelName, out var channelObj);

                    // 设备ID、冗余设备ID都需要手动补录
                    if (hasRedundant && redundantObj != null)
                    {
                        if (deviceDicts.TryGetValue(redundantObj.ToString(), out var redundantDevice))
                            device.RedundantDeviceId = redundantDevice.Id;
                        else
                        {
                            // 如果找不到对应的冗余设备，则添加错误信息到导入预览结果并返回
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, RedundantDeviceError));
                            return;
                        }
                    }
                    else
                    {
                        // 如果冗余设备未启用，则添加错误信息到导入预览结果并返回
                        if (device.RedundantEnable)
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, RedundantDeviceError));
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
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ChannelError));
                            return;
                        }
                    }
                    else
                    {
                        // 如果未提供通道信息，则添加错误信息到导入预览结果并返回
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ChannelError));
                        return;
                    }

                    // 进行设备对象属性的验证
                    var validationContext = new ValidationContext(device);
                    var validationResults = new List<ValidationResult>();
                    validationContext.ValidateProperty(validationResults);

                    // 构建验证结果的错误信息
                    using ValueStringBuilder stringBuilder = new();
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
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, stringBuilder.ToString()));
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
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, "Operation not permitted"));
                    }
                    else
                    {
                        devices.Add(device);
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), true, null));
                    }
                    return;
                }
                catch (Exception ex)
                {
                    // 捕获异常并添加错误信息到导入预览结果
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ex.Message));
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
                return;

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
                importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, Localizer["NotNull", sheetName]));
                return;
            }

            if (propertysDict.TryGetValue(driverPluginType.FullName, out var propertys))
            {
            }
            else
            {
                try
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
                catch
                {
                }
            }

            // 遍历每一行数据
            foreach (var item in rows)
            {
                try
                {
                    if (propertys.Item1 == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, PluginNotNull));
                        continue;
                    }

                    // 获取设备名称
                    if (!item.TryGetValue(GatewayExportString.DeviceName, out var deviceName))
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, DeviceNotNull));
                        continue;
                    }

                    // 转化插件名称
                    var value = item[GatewayExportString.DeviceName]?.ToString();

                    // 检查设备名称是否存在于设备导入预览数据中，如果不存在，则添加错误信息到导入预览结果并继续下一轮循环
                    var hasDevice = deviceImportPreview.Data.ContainsKey(value);
                    if (!hasDevice)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, Localizer["NotNull", value]));
                        continue;
                    }

                    // 尝试将导入的项转换为对象
                    var pluginProp = item.ConvertToEntity(propertys.Item1, propertys.Item3);

                    // 如果转换失败，则添加错误信息到导入预览结果并返回
                    if (pluginProp == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ImportNullError));
                        return;
                    }

                    // 检查属性的验证结果
                    var validationContext = new ValidationContext(pluginProp);
                    var validationResults = new List<ValidationResult>();
                    validationContext.ValidateProperty(validationResults);

                    // 构建验证结果的错误信息
                    using ValueStringBuilder stringBuilder = new();
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
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, stringBuilder.ToString()));
                        return;
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
                    importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), true, null));
                    continue;
                }
                catch (Exception ex)
                {
                    // 捕获异常并添加错误信息到导入预览结果并返回
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ex.Message));
                    return;
                }
            }
        }
    }

    #endregion 导入
}
