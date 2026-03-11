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
using System.Reflection;
using System.Text;

using ThingsGateway.Common.Extension;
using ThingsGateway.Common.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

internal sealed class VariableService : BaseService<Variable>, IVariableService
{
    private readonly IChannelService _channelService;
    private readonly IDeviceService _deviceService;
    private readonly IPluginService _pluginService;

    /// <inheritdoc cref="IVariableService"/>
    public VariableService()
    {
        _channelService = App.RootServices.GetRequiredService<IChannelService>();
        _pluginService = App.RootServices.GetRequiredService<IPluginService>();
        _deviceService = App.RootServices.GetRequiredService<IDeviceService>();
    }

    #region 测试

    public async Task<(List<Channel>, List<Device>, List<Variable>)> InsertTestDataAsync(int variableCount, int deviceCount, string slaveUrl = "127.0.0.1:502", bool businessEnable = false)
    {
        if (string.IsNullOrWhiteSpace(slaveUrl)) slaveUrl = "127.0.0.1:502";
        if (deviceCount > variableCount) variableCount = deviceCount;
        List<Channel> newChannels = new();
        List<Device> newDevices = new();
        List<Variable> newVariables = new();
        // 计算每个设备分配的默认变量数
        var groupVariableCount = (int)Math.Ceiling((decimal)variableCount / deviceCount);

        ManageHelper.CheckChannelCount(deviceCount);
        ManageHelper.CheckDeviceCount(deviceCount);
        ManageHelper.CheckVariableCount(variableCount);



        if (businessEnable)
        {
            Channel serviceChannel = new Channel();
            Device serviceDevice = new Device();

            {
                var id = CommonUtils.GetSingleId();
                var name = $"modbusSlaveChannel{id}";
                serviceChannel.ChannelType = ChannelTypeEnum.TcpService;
                serviceChannel.Name = name;
                serviceChannel.Enable = true;
                serviceChannel.Id = id;
                serviceChannel.CreateUserId = UserManager.UserId;
                serviceChannel.CreateOrgId = UserManager.OrgId;
                serviceChannel.BindUrl = "127.0.0.1:502";
                serviceChannel.PluginName = "ThingsGateway.Plugin.Modbus.ModbusSlave";
                newChannels.Add(serviceChannel);
            }
            {
                var id = CommonUtils.GetSingleId();
                var name = $"modbusSlaveDevice{id}";
                serviceDevice.Name = name;
                serviceDevice.Id = id;
                serviceDevice.CreateUserId = UserManager.UserId;
                serviceDevice.CreateOrgId = UserManager.OrgId;
                serviceDevice.ChannelId = serviceChannel.Id;
                serviceDevice.IntervalTime = "1000";
                newDevices.Add(serviceDevice);
            }

            Channel mqttChannel = new Channel();
            Device mqttDevice = new Device();

            {
                var id = CommonUtils.GetSingleId();
                var name = $"mqttChannel{id}";
                mqttChannel.ChannelType = ChannelTypeEnum.Other;
                mqttChannel.Name = name;
                mqttChannel.Id = id;
                mqttChannel.CreateUserId = UserManager.UserId;
                mqttChannel.CreateOrgId = UserManager.OrgId;
                mqttChannel.PluginName = "ThingsGateway.Plugin.Mqtt.MqttServer";
                newChannels.Add(mqttChannel);
            }
            {
                var id = CommonUtils.GetSingleId();
                var name = $"mqttDevice{id}";
                mqttDevice.Name = name;
                mqttDevice.Id = id;
                mqttDevice.CreateUserId = UserManager.UserId;
                mqttDevice.CreateOrgId = UserManager.OrgId;
                mqttDevice.ChannelId = mqttChannel.Id;
                mqttDevice.IntervalTime = "1000";
                mqttDevice.DevicePropertys = new Dictionary<string, string>
            {
              {"IsAllVariable", "true"}
            };
                newDevices.Add(mqttDevice);
            }
        }

        for (int i = 0; i < deviceCount; i++)
        {
            Channel channel = new Channel();
            Device device = new Device();
            {
                var id = CommonUtils.GetSingleId();
                var name = $"modbusChannel{id}";
                channel.ChannelType = ChannelTypeEnum.TcpClient;
                channel.Name = name;
                channel.Id = id;
                channel.CreateUserId = UserManager.UserId;
                channel.CreateOrgId = UserManager.OrgId;
                channel.RemoteUrl = slaveUrl;
                channel.PluginName = "ThingsGateway.Plugin.Modbus.ModbusMaster";
                //动态插件属性默认
                newChannels.Add(channel);
            }
            {
                var id = CommonUtils.GetSingleId();
                var name = $"modbusDevice{id}";
                device.Name = name;
                device.Id = id;
                device.ChannelId = channel.Id;
                device.CreateUserId = UserManager.UserId;
                device.CreateOrgId = UserManager.OrgId;
                device.IntervalTime = "1000";
                //动态插件属性默认
                newDevices.Add(device);
            }

            // 计算当前设备应该分配的变量数量
            int currentGroupVariableCount = (i == deviceCount - 1)
                ? variableCount - (deviceCount - 1) * groupVariableCount // 最后一个设备分配剩余的变量
                : groupVariableCount;

            var addressNum = 1;

            for (int i1 = 0; i1 < currentGroupVariableCount; i1++)
            {
                if (addressNum > 65535) addressNum = 1;
                var address = $"4{addressNum}";
                addressNum++;
                var id = CommonUtils.GetSingleId();
                var name = $"modbus{address}";
                Variable variable = new Variable();
                variable.DataType = DataTypeEnum.Int16;
                variable.Name = name;
                variable.Id = id;
                variable.DeviceId = device.Id;
                variable.RegisterAddress = address;
                newVariables.Add(variable);
            }
        }


        //Channel opcuaChannel = new Channel();
        //Device opcuaDevice = new Device();

        //{
        //    var id = CommonUtils.GetSingleId();
        //    var name = $"opcuaChannel{id}";
        //    opcuaChannel.ChannelType = ChannelTypeEnum.Other;
        //    opcuaChannel.Name = name;
        //    opcuaChannel.Id = id;
        //    opcuaChannel.CreateUserId = UserManager.UserId;
        //    opcuaChannel.CreateOrgId = UserManager.OrgId;
        //    opcuaChannel.PluginName = "ThingsGateway.Plugin.OpcUa.OpcUaServer";
        //    newChannels.Add(opcuaChannel);
        //}
        //{
        //    var id = CommonUtils.GetSingleId();
        //    var name = $"opcuaDevice{id}";
        //    opcuaDevice.Name = name;
        //    opcuaDevice.Id = id;
        //    opcuaDevice.CreateUserId = UserManager.UserId;
        //    opcuaDevice.CreateOrgId = UserManager.OrgId;
        //    opcuaDevice.ChannelId = opcuaChannel.Id;
        //    opcuaDevice.IntervalTime = "1000";
        //    opcuaDevice.DevicePropertys = new Dictionary<string, string>
        //    {
        //      {"IsAllVariable", "true"}
        //    };
        //    newDevices.Add(opcuaDevice);
        //}

        using var db = GetDB();

        var result = await db.UseTranAsync(async () =>
        {
            if (GlobalData.HardwareJob.HardwareInfo.AvailableMemory < 2048)
            {
                await db.BulkCopyAsync(newChannels, 10000).ConfigureAwait(false);
                await db.BulkCopyAsync(newDevices, 10000).ConfigureAwait(false);
                await db.BulkCopyAsync(newVariables, 10000).ConfigureAwait(false);
            }
            else
            {
                await db.BulkCopyAsync(newChannels, 200000).ConfigureAwait(false);
                await db.BulkCopyAsync(newDevices, 200000).ConfigureAwait(false);
                await db.BulkCopyAsync(newVariables, 200000).ConfigureAwait(false);

            }
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {

        }
        else
        {
            throw new(result.Message, result.Exception);
        }
        return (newChannels, newDevices, newVariables);
    }


    public async Task<(List<Channel>, List<Device>, List<Variable>)> InsertTestDtuDataAsync(int deviceCount, string slaveUrl = "127.0.0.1:502")
    {
        if (string.IsNullOrWhiteSpace(slaveUrl)) slaveUrl = "127.0.0.1:502";
        List<Channel> newChannels = new();
        List<Device> newDevices = new();
        List<Variable> newVariables = new();

        ManageHelper.CheckChannelCount(deviceCount);
        ManageHelper.CheckDeviceCount(deviceCount);
        ManageHelper.CheckVariableCount(deviceCount);


        //DTU

        for (int i = 0; i < deviceCount; i++)
        {

            Channel serviceChannel = new Channel();
            Device serviceDevice = new Device();

            {
                var id = CommonUtils.GetSingleId();
                var name = $"modbusSlaveChannel{id}";
                serviceChannel.ChannelType = ChannelTypeEnum.TcpClient;
                serviceChannel.Name = name;
                serviceChannel.Enable = true;
                serviceChannel.Id = id;
                serviceChannel.CreateUserId = UserManager.UserId;
                serviceChannel.CreateOrgId = UserManager.OrgId;
                serviceChannel.RemoteUrl = "127.0.0.1:502";
                serviceChannel.DtuId = name;
                serviceChannel.Heartbeat = "ThingsGateway.Plugin.Modbus";
                serviceChannel.PluginName = "ThingsGateway.Plugin.Modbus.ModbusSlave";
                newChannels.Add(serviceChannel);
            }
            {
                var id = CommonUtils.GetSingleId();
                var name = $"modbusSlaveDevice{id}";
                serviceDevice.Name = name;
                serviceDevice.Id = id;
                serviceDevice.CreateUserId = UserManager.UserId;
                serviceDevice.CreateOrgId = UserManager.OrgId;
                serviceDevice.ChannelId = serviceChannel.Id;
                serviceDevice.IntervalTime = "1000";
                newDevices.Add(serviceDevice);
            }

        }


        //SERVICE
        var dtuids = newChannels.Select(a => a.Name).ToList();
        Channel channel = new Channel();
        {
            var id = CommonUtils.GetSingleId();
            var name = $"modbusChannel{id}";
            channel.ChannelType = ChannelTypeEnum.TcpService;
            channel.Name = name;
            channel.Id = id;
            channel.CreateUserId = UserManager.UserId;
            channel.CreateOrgId = UserManager.OrgId;
            channel.BindUrl = slaveUrl;
            channel.Heartbeat = "ThingsGateway.Plugin.Modbus";
            channel.PluginName = "ThingsGateway.Plugin.Modbus.ModbusMaster";
            //动态插件属性默认
            newChannels.Add(channel);
        }

        foreach (var item in dtuids)
        {

            Device device = new Device();
            {
                var id = CommonUtils.GetSingleId();
                var name = $"modbusDevice{id}";
                device.Name = name;
                device.Id = id;
                device.ChannelId = channel.Id;
                device.CreateUserId = UserManager.UserId;
                device.CreateOrgId = UserManager.OrgId;
                device.IntervalTime = "1000";
                device.DevicePropertys = new Dictionary<string, string>()
                {
                    {
                        nameof(CollectFoundationDtuPackPropertyBase.DtuId),item
                    }
                };
                //动态插件属性默认
                newDevices.Add(device);
            }
            {
                var address = $"400001";
                var id = CommonUtils.GetSingleId();
                var name = $"modbus{address}";
                Variable variable = new Variable();
                variable.DataType = DataTypeEnum.Int16;
                variable.Name = name;
                variable.Id = id;
                variable.DeviceId = device.Id;
                variable.RegisterAddress = address;
                newVariables.Add(variable);
            }
        }


        using var db = GetDB();

        var result = await db.UseTranAsync(async () =>
        {
            if (GlobalData.HardwareJob.HardwareInfo.AvailableMemory < 2048)
            {
                await db.BulkCopyAsync(newChannels, 10000).ConfigureAwait(false);
                await db.BulkCopyAsync(newDevices, 10000).ConfigureAwait(false);
                await db.BulkCopyAsync(newVariables, 10000).ConfigureAwait(false);
            }
            else
            {
                await db.BulkCopyAsync(newChannels, 200000).ConfigureAwait(false);
                await db.BulkCopyAsync(newDevices, 200000).ConfigureAwait(false);
                await db.BulkCopyAsync(newVariables, 200000).ConfigureAwait(false);

            }
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {

        }
        else
        {
            throw new(result.Message, result.Exception);
        }
        return (newChannels, newDevices, newVariables);
    }


    #endregion 测试

    /// <summary>
    /// 保存初始值
    /// </summary>
    public async Task UpdateInitValueAsync(List<Variable> variables)
    {
        if (variables.Count > 0)
        {
            using var db = GetDB();
            var result = await db.Updateable<Variable>(variables).UpdateColumns(a => a.InitValue).ExecuteCommandAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    [OperDesc("SaveVariable", isRecordPar: false, localizerType: typeof(Variable))]
    public async Task<bool> BatchSaveVariableAsync(List<Variable> input, ItemChangedType type)
    {
        if (type == ItemChangedType.Add)
        {
            ManageHelper.CheckVariableCount(input.Count);

            using var db = GetDB();

            var result = await db.Insertable(input).ExecuteCommandAsync().ConfigureAwait(false);

            if (result > 0)
            {

                return true;
            }
        }
        else
        {
            using var db = GetDB();

            var result = await db.Updateable(input).ExecuteCommandAsync().ConfigureAwait(false);

            if (result > 0)
            {

                return true;
            }
        }
        return false;
    }

    /// <inheritdoc/>
    [OperDesc("SaveVariable", localizerType: typeof(Variable), isRecordPar: false)]
    public async Task<bool> BatchEditAsync(IEnumerable<Variable> models, Variable oldModel, Variable model)
    {
        var differences = models.GetDiffProperty(oldModel, model);
        differences.Remove(nameof(Variable.VariablePropertys));
        if (differences?.Count > 0)
        {
            var data = models.ToList();
            await GlobalData.CheckByDeviceIds(data.Select(a => a.DeviceId)).ConfigureAwait(false);
            using var db = GetDB();

            var result = (await db.Updateable(data).UpdateColumns(differences.Select(a => a.Key).ToArray()).ExecuteCommandAsync().ConfigureAwait(false)) > 0;

            return result;
        }
        else
        {
            return true;
        }
    }

    [OperDesc("DeleteVariable", isRecordPar: false, localizerType: typeof(Variable))]
    public async Task DeleteByDeviceIdAsync(IEnumerable<long> input, ISqlClient db)
    {
        var ids = input.ToList();
        await GlobalData.CheckByDeviceIds(ids).ConfigureAwait(false);

        var result = await db.Deleteable<Variable>().Where(a => ids.Contains(a.DeviceId))
            .ExecuteCommandAsync().ConfigureAwait(false);
    }

    [OperDesc("DeleteVariable", isRecordPar: false, localizerType: typeof(Variable))]
    public async Task<bool> DeleteVariableAsync(IEnumerable<long> input)
    {
        using var db = GetDB();
        var ids = input?.ToList();
        await GlobalData.CheckByVariableIds(ids).ConfigureAwait(false);
        var result = (await db.Deleteable<Variable>().WhereIF(input != null, a => ids.Contains(a.Id))
             .ExecuteCommandAsync().ConfigureAwait(false)) > 0;

        return result;
    }

    public async Task<List<Variable>> GetByDeviceIdAsync(List<long> deviceIds)
    {
        using var db = GetDB();
        var deviceVariables = await db.Queryable<Variable>().Where(a => deviceIds.Contains(a.DeviceId)).ToListAsync().ConfigureAwait(false);
        return deviceVariables;
    }
    public async Task<List<Variable>> GetAllAsync(long? devId = null)
    {
        using var db = GetDB();
        if (devId == null)
        {
            var deviceVariables = await db.Queryable<Variable>().OrderBy(a => a.Id).ToListAsync().ConfigureAwait(false);
            return deviceVariables;
        }
        else
        {
            var deviceVariables = await db.Queryable<Variable>().Where(a => a.DeviceId == devId).OrderBy(a => a.Id).ToListAsync().ConfigureAwait(false);
            return deviceVariables;
        }
    }

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="exportFilter">查询条件</param>
    public async Task<QueryData<Variable>> PageAsync(GatewayExportFilter exportFilter)
    {
        var whereQuery = await GetWhereQueryFunc(exportFilter).ConfigureAwait(false);

        return await QueryAsync(exportFilter.QueryPageOptions, whereQuery).ConfigureAwait(false);
    }
    private async Task<Func<ISqlQueryable<Variable>, ISqlQueryable<Variable>>> GetWhereQueryFunc(GatewayExportFilter exportFilter)
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        List<long>? filterDeviceIds = null;
        if (dataScope != null)
        {
            filterDeviceIds = GlobalData.GetCurrentUserDeviceIds(dataScope).ToList();
        }
        HashSet<long>? deviceId = null;
        if (!string.IsNullOrWhiteSpace(exportFilter.PluginName))
        {
            var channel = (GlobalData.IdChannels).Where(a => a.Value.PluginName == exportFilter.PluginName).Select(a => a.Value.Id).ToHashSet();
            deviceId = (GlobalData.IdDevices).Where(a => channel.Contains(a.Value.ChannelId)).Select(a => a.Value.Id).ToHashSet();
        }
        else if (exportFilter.ChannelId != null)
        {
            deviceId = (GlobalData.IdDevices).Where(a => a.Value.ChannelId == exportFilter.ChannelId).Select(a => a.Value.Id).ToHashSet();
        }
        var whereQuery = (ISqlQueryable<Variable> a) => a
        .WhereIF(!string.IsNullOrWhiteSpace(exportFilter.QueryPageOptions.SearchText), a => a.Name.Contains(exportFilter.QueryPageOptions.SearchText!))
        .WhereIF(exportFilter.PluginType == PluginTypeEnum.Collect, a => a.DeviceId == exportFilter.DeviceId)
        .WhereIF(deviceId != null, a => deviceId.Contains(a.DeviceId))
        .Where(a => a.DeviceId != MemoryConst.MemoryDeviceId)

        .WhereIF(filterDeviceIds != null, u => filterDeviceIds.Contains(u.DeviceId))//在指定机构列表查询

        .WhereIF(exportFilter.PluginType == PluginTypeEnum.Business, u => SqlFunc.JsonLike(u.VariablePropertys, exportFilter.DeviceId.ToString()));
        return whereQuery;
    }

    private async Task<Func<IEnumerable<Variable>, IEnumerable<Variable>>> GetWhereEnumerableFunc(GatewayExportFilter exportFilter, bool sql = false)
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        List<long>? filterDeviceIds = null;
        if (dataScope != null)
        {
            filterDeviceIds = GlobalData.GetCurrentUserDeviceIds(dataScope).ToList();
        }


        HashSet<long>? deviceId = null;
        if (!string.IsNullOrWhiteSpace(exportFilter.PluginName))
        {
            var channel = (GlobalData.IdChannels).Where(a => a.Value.PluginName == exportFilter.PluginName).Select(a => a.Value.Id).ToHashSet();
            deviceId = (GlobalData.IdDevices).Where(a => channel.Contains(a.Value.ChannelId)).Select(a => a.Value.Id).ToHashSet();
        }
        else if (exportFilter.ChannelId != null)
        {
            deviceId = (GlobalData.IdDevices).Where(a => a.Value.ChannelId == exportFilter.ChannelId).Select(a => a.Value.Id).ToHashSet();
        }
        var whereQuery = (IEnumerable<Variable> a) => a
        .WhereIF(!string.IsNullOrWhiteSpace(exportFilter.QueryPageOptions.SearchText), a => a.Name.Contains(exportFilter.QueryPageOptions.SearchText!))
        .WhereIF(exportFilter.PluginType == PluginTypeEnum.Collect, a => a.DeviceId == exportFilter.DeviceId)
        .WhereIF(deviceId != null, a => deviceId.Contains(a.DeviceId))
        .Where(a => a.DeviceId != MemoryConst.MemoryDeviceId)

        .WhereIF(filterDeviceIds != null, u => filterDeviceIds.Contains(u.DeviceId))//在指定机构列表查询

        .WhereIF(sql && exportFilter.PluginType == PluginTypeEnum.Business, u => SqlFunc.JsonLike(u.VariablePropertys, exportFilter.DeviceId.ToString()))
        .WhereIF(!sql && exportFilter.PluginType == PluginTypeEnum.Business && exportFilter.DeviceId > 0, u =>
        GlobalData.IdVariables.TryGetValue(u.Id, out var runtime) &&
        GlobalData.ContainsVariable(exportFilter.DeviceId.Value, runtime)

        );
        return whereQuery;
    }

    /// <summary>
    /// 保存变量
    /// </summary>
    /// <param name="input">变量</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveVariable", localizerType: typeof(Variable))]
    public async Task<bool> SaveVariableAsync(Variable input, ItemChangedType type)
    {
        if (type == ItemChangedType.Update)
            await GlobalData.CheckByVariableId(input.Id).ConfigureAwait(false);
        else
            ManageHelper.CheckVariableCount(1);

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {

            return true;
        }
        return false;
    }



    public List<VariableRuntime> GetAllVariableRuntime()
    {
        using (var db = DbContext.GetDB<Variable>())
        {
            var deviceVariables = db.Queryable<Variable>().OrderBy(a => a.Id).ToEnumerable();
            return deviceVariables.AdaptListVariableRuntime();
        }
    }
    #region 导出

    public Dictionary<string, object> ExportDictionary(List<Variable> variables, string deviceName = null)
    {
        var deviceDicts = GlobalData.IdDevices;
        var channelDicts = GlobalData.IdChannels;
        var pluginSheetNames = variables.Where(a => a.VariablePropertys?.Count > 0).SelectMany(a => a.VariablePropertys).Select(a =>
        {
            if (deviceDicts.TryGetValue(a.Key, out var device) && channelDicts.TryGetValue(device.ChannelId, out var channel))
            {
                var pluginKey = channel?.PluginName;
                var businessBase = (BusinessBase)GlobalData.PluginService.GetDriver(pluginKey);
                return new KeyValuePair<string, VariablePropertyBase>(pluginKey, businessBase.VariablePropertys);
            }
            return new KeyValuePair<string, VariablePropertyBase>(string.Empty, null);
        }).Where(a => a.Value != null).DistinctBy(a => a.Key).ToDictionary();
        Dictionary<string, object>? sheets = VariableServiceHelpers.ExportSheets(variables, deviceDicts, channelDicts, pluginSheetNames, deviceName); // IEnumerable 延迟执行

        return sheets;
    }

    /// <summary>
    /// 导出文件
    /// </summary>
    [OperDesc("ExportVariable", isRecordPar: false, localizerType: typeof(Variable))]
    public async Task<Dictionary<string, object>> ExportVariableAsync(GatewayExportFilter exportFilter)
    {
        if (GlobalData.HardwareJob.HardwareInfo.AvailableMemory < 2048)
        {
            var whereQuery = await GetWhereEnumerableFunc(exportFilter).ConfigureAwait(false);
            //导出
            var variables = GlobalData.IdVariables.Select(a => a.Value).GetQuery(exportFilter.QueryPageOptions, whereQuery, exportFilter.FilterKeyValueAction);

            var deviceDicts = GlobalData.IdDevices;
            var channelDicts = GlobalData.IdChannels;
            var pluginSheetNames = variables.Where(a => a.VariablePropertys?.Count > 0).SelectMany(a => a.VariablePropertys).Select(a =>
            {
                if (deviceDicts.TryGetValue(a.Key, out var device) && channelDicts.TryGetValue(device.ChannelId, out var channel))
                {
                    var pluginKey = channel?.PluginName;
                    var businessBase = (BusinessBase)GlobalData.PluginService.GetDriver(pluginKey);
                    return new KeyValuePair<string, VariablePropertyBase>(pluginKey, businessBase.VariablePropertys);
                }
                return new KeyValuePair<string, VariablePropertyBase>(string.Empty, null);
            }).Where(a => a.Value != null).DistinctBy(a => a.Key).ToDictionary();

            var sheets = VariableServiceHelpers.ExportSheets(variables, deviceDicts, channelDicts, pluginSheetNames, null); // IEnumerable 延迟执行

            return sheets;
        }
        else
        {
            var whereQuery = await GetWhereEnumerableFunc(exportFilter).ConfigureAwait(false);
            //导出
            var data = GlobalData.IdVariables.Select(a => a.Value).GetQuery(exportFilter.QueryPageOptions, whereQuery, exportFilter.FilterKeyValueAction);
            //var data = (await PageAsync(exportFilter).ConfigureAwait(false));
            var sheets = VariableServiceHelpers.ExportCore(data, sortName: exportFilter.QueryPageOptions.SortName, sortOrder: exportFilter.QueryPageOptions.SortOrder);
            return sheets;
        }
    }
    private async Task<IAsyncEnumerable<Variable>> GetAsyncEnumerableData(GatewayExportFilter exportFilter)
    {
        var whereQuery = await GetEnumerableData(exportFilter).ConfigureAwait(false);
        return whereQuery.ToAsyncEnumerable();
    }
    private async Task<ISqlQueryable<Variable>> GetEnumerableData(GatewayExportFilter exportFilter)
    {
        var db = GetDB();
        var whereQuery = await GetWhereQueryFunc(exportFilter).ConfigureAwait(false);

        return GetQuery(db, exportFilter.QueryPageOptions, whereQuery, exportFilter.FilterKeyValueAction);
    }

    #endregion 导出

    #region 导入

    /// <inheritdoc/>
    [OperDesc("ImportVariable", isRecordPar: false, localizerType: typeof(Variable))]
    public Task<HashSet<long>> ImportVariableAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        IEnumerable<Variable>? variables = new List<Variable>();
        foreach (var item in input)
        {
            if (item.Key == GatewayExportString.VariableName)
            {
                var variableImports = ((ImportPreviewOutput<Dictionary<string, Variable>>)item.Value).Data;
                variables = variableImports.SelectMany(a => a.Value.Select(a => a.Value));
                break;
            }
        }
        var upData = variables.Where(a => a.IsUp).ToList();
        var insertData = variables.Where(a => !a.IsUp).ToList();
        return ImportVariableAsync(upData, insertData);
    }

    [OperDesc("ImportVariable", isRecordPar: false, localizerType: typeof(Variable))]
    public async Task<HashSet<long>> ImportVariableAsync(List<Variable> upData, List<Variable> insertData)
    {
        ManageHelper.CheckVariableCount(insertData.Count);
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
        // 上传文件并获取文件路径
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);

        try
        {
            // 获取Excel文件中所有工作表的名称
            var sheetNames = MiniExcel.GetSheetNames(path);

            // 获取所有设备的字典，以设备名称作为键
            var deviceDicts = GlobalData.Devices;

            // 存储导入检验结果的字典
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

            // 设备页导入预览输出
            ImportPreviewOutput<Dictionary<string, Variable>> deviceImportPreview = new();

            var driverPluginNameDict = _pluginService.GetPluginList().ToDictionary(a => a.Name);
            NonBlockingDictionary<string, (Type, Dictionary<string, PropertyInfo>, Dictionary<string, PropertyInfo>)> propertysDict = new();

            // 遍历每个工作表
            foreach (var sheetName in sheetNames)
            {
                // 获取当前工作表的所有行数据
#pragma warning disable CA1849
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();
#pragma warning restore CA1849

                deviceImportPreview = SetVariableData(dataScope, deviceDicts, ImportPreviews, deviceImportPreview, driverPluginNameDict, propertysDict, sheetName, rows);
            }

            return ImportPreviews;
        }
        finally
        {
            // 最终清理：删除临时上传的文件
            FileHelper.DeleteFile(path);
        }
    }

    public ImportPreviewOutput<Dictionary<string, Variable>> SetVariableData(HashSet<long>? dataScope, IReadOnlyDictionary<string, DeviceRuntime> deviceDicts, Dictionary<string, ImportPreviewOutputBase> ImportPreviews, ImportPreviewOutput<Dictionary<string, Variable>> deviceImportPreview, Dictionary<string, PluginInfo> driverPluginNameDict, NonBlockingDictionary<string, (Type, Dictionary<string, PropertyInfo>, Dictionary<string, PropertyInfo>)> propertysDict, string sheetName, IEnumerable<IDictionary<string, object>> rows)
    {

        List<long>? filterDeviceIds = null;
        if (dataScope != null)
        {
            filterDeviceIds = GlobalData.GetCurrentUserDeviceIds(dataScope).ToList();
        }

        string ImportNullError = Localizer["ImportNullError"];
        string RedundantDeviceError = Localizer["RedundantDeviceError"];

        string PluginNotNull = Localizer["PluginNotNull"];
        string DeviceNotNull = Localizer["DeviceNotNull"];
        string VariableNotNull = Localizer["VariableNotNull"];


        // 变量页处理
        if (sheetName == GatewayExportString.VariableName)
        {
            int row = 1;
            ImportPreviewOutput<Dictionary<string, Variable>> importPreviewOutput = new();
            ImportPreviews.Add(sheetName, importPreviewOutput);
            deviceImportPreview = importPreviewOutput;

            // 线程安全的变量列表
            var variables = new ConcurrentList<Variable>();
            var type = typeof(Variable);
            // 获取目标类型的所有属性，并根据是否需要过滤 IgnoreExcelAttribute 进行筛选
            var variableProperties = type.GetRuntimeProperties().Where(a => (a.GetCustomAttribute<IgnoreExcelAttribute>() == null) && a.CanWrite)
                                        .ToDictionary(a => type.GetPropertyDisplayName(a.Name), a => (a, a.IsNullableType()));

            // 并行处理每一行数据
            rows.ParallelForEachStreamed((item, state, index) =>
            {
                try
                {
                    // 尝试将行数据转换为 Variable 对象
                    var variable = item.ConvertToEntity<Variable>(variableProperties);
                    variable.Row = index;

                    // 获取设备名称并查找对应的设备
                    item.TryGetValue(GatewayExportString.DeviceName, out var value);
                    var deviceName = value?.ToString();
                    deviceDicts.TryGetValue(deviceName, out var device);
                    var deviceId = device?.Id;

                    // 如果找不到对应的设备，则添加错误信息到导入预览结果并返回
                    if (deviceId == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, Localizer["NotNull", deviceName]));
                        return;
                    }
                    // 手动补录变量ID和设备ID
                    variable.DeviceId = deviceId.Value;

                    // 对 Variable 对象进行验证
                    var validationContext = new ValidationContext(variable);
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

                    if (GlobalData.TryGetDeviceRuntime(variable.DeviceId, out var dbvar1s) && dbvar1s.VariableRuntimes.TryGetValue(variable.Name, out var dbvar1))
                    {
                        variable.Id = dbvar1.Id;
                        variable.IsUp = true;
                    }
                    else
                    {
                        variable.IsUp = false;
                    }

                    if (variable.IsUp && (filterDeviceIds?.Contains(variable.DeviceId) == true))
                    {
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, "Operation not permitted"));
                    }
                    else
                    {
                        variables.Add(variable);
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), true, null));
                    }
                }
                catch (Exception ex)
                {
                    // 捕获异常并添加错误信息到导入预览结果
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ex.Message));
                }
            });

            // 为未成功上传的变量生成新的ID
            foreach (var item in variables)
            {
                if (!item.IsUp)
                    item.Id = CommonUtils.GetSingleId();
            }

            // 将变量列表转换为字典，并赋值给导入预览输出对象的 Data 属性
            importPreviewOutput.Data = variables.OrderBy(a => a.Row).GroupBy(a => a.DeviceId.ToString()).ToDictionary(a => a.Key, b => b.ToDictionary(a => a.Name));
        }
        else if (sheetName == GatewayExportString.AlarmName)
        {
            int row = 1;
            ImportPreviewOutput<string> importPreviewOutput = new();
            ImportPreviews.Add(sheetName, importPreviewOutput);
            var type = typeof(AlarmPropertys);
            // 获取目标类型的所有属性，并根据是否需要过滤 IgnoreExcelAttribute 进行筛选
            var variableProperties = type.GetRuntimeProperties().Where(a => (a.GetCustomAttribute<IgnoreExcelAttribute>() == null) && a.CanWrite)
                                        .ToDictionary(a => type.GetPropertyDisplayName(a.Name), a => (a, a.IsNullableType()));

            // 并行处理每一行数据
            rows.ParallelForEachStreamed((item, state, index) =>
            {
                try
                {
                    var alarm = item.ConvertToEntity<AlarmPropertys>(variableProperties);

                    // 如果转换失败，则添加错误信息到导入预览结果并返回
                    if (alarm == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ImportNullError));
                        return;
                    }

                    // 转化插件名称和变量名称
                    item.TryGetValue(GatewayExportString.VariableName, out var variableNameObj);
                    item.TryGetValue(GatewayExportString.DeviceName, out var collectDevName);
                    deviceDicts.TryGetValue(collectDevName?.ToString(), out var collectDevice);
                    // 如果设备名称或变量名称为空，或者找不到对应的设备，则添加错误信息到导入预览结果并返回
                    if (collectDevName == null || collectDevice == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, DeviceNotNull));
                        return;
                    }
                    if (variableNameObj == null)
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, VariableNotNull));
                        return;
                    }

                    // 对对象进行验证
                    var validationContext = new ValidationContext(alarm);
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

                    // 获取变量名称并检查是否存在于设备导入预览数据中
                    var variableName = variableNameObj?.ToString();
                    // 如果存在，则更新变量属性字典，并添加成功信息到导入预览结果；否则，添加错误信息到导入预览结果并返回
                    if (deviceImportPreview.Data.TryGetValue(collectDevice.Id.ToString(), out var deviceVariables) && deviceVariables.TryGetValue(variableName, out var deviceVariable))
                    {
                        deviceVariable.AlarmPropertys = alarm;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), true, null));
                    }
                    else
                    {
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, VariableNotNull));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // 捕获异常并添加错误信息到导入预览结果
                    importPreviewOutput.HasError = true;
                    importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ex.Message));
                }
            });


        }
        // 其他工作表处理
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
                    importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, Localizer["NotNull", sheetName]));
                    return deviceImportPreview;
                }

                if (propertysDict.TryGetValue(driverPluginType.FullName, out var propertys))
                {
                }
                else
                {
                    try
                    {
                        var variableProperty = ((BusinessBase)_pluginService.GetDriver(driverPluginType.FullName)).VariablePropertys;
                        var variablePropertyType = variableProperty.GetType();
                        propertys.Item1 = variablePropertyType;
                        propertys.Item2 = variablePropertyType.GetRuntimeProperties()
                            .Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null)
                            .ToDictionary(a => variablePropertyType.GetPropertyDisplayName(a.Name, a => a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description));

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

                rows.ParallelForEachStreamed(item =>
                {
                    try
                    {
                        if (propertys.Item3?.Count == null || propertys.Item1 == null)
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ImportNullError));
                            return;
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

                        // 转化插件名称和变量名称
                        item.TryGetValue(GatewayExportString.VariableName, out var variableNameObj);
                        item.TryGetValue(GatewayExportString.BusinessDeviceName, out var businessDevName);
                        item.TryGetValue(GatewayExportString.DeviceName, out var collectDevName);
                        deviceDicts.TryGetValue(businessDevName?.ToString(), out var businessDevice);
                        deviceDicts.TryGetValue(collectDevName?.ToString(), out var collectDevice);

                        // 如果设备名称或变量名称为空，或者找不到对应的设备，则添加错误信息到导入预览结果并返回
                        if (businessDevName == null || businessDevice == null || collectDevName == null || collectDevice == null)
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, DeviceNotNull));
                            return;
                        }
                        if (variableNameObj == null)
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, VariableNotNull));
                            return;
                        }

                        // 对对象进行验证
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

                        // 创建依赖属性字典
                        Dictionary<string, string> dependencyProperties = new();
                        foreach (var keyValuePair in item)
                        {
                            if (propertys.Item2.TryGetValue(keyValuePair.Key, out var propertyInfo))
                            {
                                dependencyProperties.Add(propertyInfo.Name, keyValuePair.Value?.ToString());
                            }
                        }

                        // 获取变量名称并检查是否存在于设备导入预览数据中
                        var variableName = variableNameObj?.ToString();
                        // 如果存在，则更新变量属性字典，并添加成功信息到导入预览结果；否则，添加错误信息到导入预览结果并返回
                        if (deviceImportPreview.Data.TryGetValue(collectDevice.Id.ToString(), out var deviceVariables) && deviceVariables.TryGetValue(variableName, out var deviceVariable))
                        {
                            deviceVariable.VariablePropertys ??= new();
                            deviceVariable.VariablePropertys?.AddOrUpdate(businessDevice.Id, dependencyProperties);
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), true, null));
                        }
                        else
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, Localizer["VariableNotNull"]));
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        // 捕获异常并添加错误信息到导入预览结果
                        importPreviewOutput.HasError = true;
                        importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ex.Message));
                    }
                });
            }
            catch (Exception ex)
            {
                // 捕获异常并添加错误信息到导入预览结果
                importPreviewOutput.HasError = true;
                importPreviewOutput.Results.Add(new(Interlocked.Increment(ref row), false, ex.Message));
            }
        }

        return deviceImportPreview;
    }

    #endregion 导入
}
