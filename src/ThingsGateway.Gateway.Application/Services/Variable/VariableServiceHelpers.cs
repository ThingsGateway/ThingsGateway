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

using System.Collections.Concurrent;
using System.Reflection;

using ThingsGateway.Common.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

public static class VariableServiceHelpers
{
    public static USheetDatas ExportVariable(IEnumerable<Variable> variables, string sortName = nameof(Variable.Id), SortOrder sortOrder = SortOrder.Asc)
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
        var data = ExportSheets(variables, deviceDicts, channelDicts, pluginSheetNames, null); // IEnumerable 延迟执行
        return USheetDataHelpers.GetUSheetDatas(data);
    }
    static IAsyncEnumerable<Variable> FilterPluginDevices(
    IAsyncEnumerable<Variable> data,
    string pluginName,
IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
    IReadOnlyDictionary<long, ChannelRuntime> channelDicts)
    {
        return data.Where(variable =>
        {
            if (variable.VariablePropertys == null)
                return false;

            foreach (var a in variable.VariablePropertys)
            {
                if (deviceDicts.TryGetValue(a.Key, out var device) && channelDicts.TryGetValue(device.ChannelId, out var channel))

                {
                    if (channel.PluginName == pluginName)
                        return true;
                }
            }

            return false;
        });
    }
    static IEnumerable<Variable> FilterPluginDevices(
IEnumerable<Variable> data,
string pluginName,
IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
IReadOnlyDictionary<long, ChannelRuntime> channelDicts)
    {
        return data.Where(variable =>
        {
            if (variable.VariablePropertys == null)
                return false;

            foreach (var a in variable.VariablePropertys)
            {
                if (deviceDicts.TryGetValue(a.Key, out var device) && channelDicts.TryGetValue(device.ChannelId, out var channel))
                {
                    if (channel.PluginName == pluginName)
                        return true;
                }
            }

            return false;
        });
    }

    public static Dictionary<string, object> ExportSheets(
    IEnumerable<Variable> data,
    IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
    IReadOnlyDictionary<long, ChannelRuntime> channelDicts,
    Dictionary<string, VariablePropertyBase> pluginDrivers,
    string? deviceName)
    {
        var sheets = new Dictionary<string, object>();
        var propertysDict = new NonBlockingDictionary<string, (VariablePropertyBase, Dictionary<string, PropertyInfo>)>();

        // 主变量页
        sheets.Add(GatewayExportString.VariableName, GetVariableSheets(data, deviceDicts, deviceName));
        sheets.Add(GatewayExportString.AlarmName, GetAlarmSheets(data, deviceDicts, deviceName));

        // 插件页（动态推导）
        foreach (var plugin in pluginDrivers.Keys.Distinct())
        {
            var filtered = FilterPluginDevices(data, plugin, deviceDicts, channelDicts);
            var pluginName = PluginInfoUtil.GetFileNameAndTypeName(plugin).Item2;
            sheets.Add(pluginName, GetPluginSheets(filtered, deviceDicts, channelDicts, plugin, pluginDrivers, propertysDict));
        }

        return sheets;
    }

    static IEnumerable<Dictionary<string, object>> GetVariableSheets(
    IEnumerable<Variable> data,
    IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
    string? deviceName)
    {
        var type = typeof(Variable);
        var propertyInfos = type.GetRuntimeProperties()
            .Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
            .OrderBy(a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
                if (order < 0) order += 10000000;
                else if (order == 0) order = 10000000;
                return order;
            });

        foreach (var variable in data)
        {
            yield return GetVariable(deviceDicts, deviceName, type, propertyInfos, variable);
        }
    }

    private static Dictionary<string, object> GetVariable(IReadOnlyDictionary<long, DeviceRuntime> deviceDicts, string? deviceName, Type type, IOrderedEnumerable<PropertyInfo> propertyInfos, Variable variable)
    {
        var row = new Dictionary<string, object>();
        deviceDicts.TryGetValue(variable.DeviceId, out var device);
        row.TryAdd(GatewayExportString.DeviceName, device?.Name ?? deviceName);

        foreach (var item in propertyInfos)
        {
            if (item.Name == nameof(Variable.Id))
            {
                continue;
            }
            var desc = type.GetPropertyDisplayName(item.Name);
            row.TryAdd(desc ?? item.Name, item.GetValue(variable)?.ToString());
        }

        return row;
    }

    static IEnumerable<Dictionary<string, object>> GetAlarmSheets(
IEnumerable<Variable> data,
IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
string? deviceName)
    {
        var type = typeof(AlarmPropertys);
        var propertyInfos = type.GetRuntimeProperties()
            .Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
            .OrderBy(a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
                if (order < 0) order += 10000000;
                else if (order == 0) order = 10000000;
                return order;
            });

        foreach (var variable in data)
        {
            if (variable.AlarmPropertys != null)
                yield return GetAlarm(deviceDicts, deviceName, type, propertyInfos, variable);
        }
    }

    private static Dictionary<string, object> GetAlarm(IReadOnlyDictionary<long, DeviceRuntime> deviceDicts, string? deviceName, Type type, IOrderedEnumerable<PropertyInfo> propertyInfos, Variable variable)
    {
        var row = new Dictionary<string, object>();
        deviceDicts.TryGetValue(variable.DeviceId, out var device);
        row.TryAdd(GatewayExportString.DeviceName, device?.Name ?? deviceName);
        row.TryAdd(GatewayExportString.VariableName, variable.Name);

        foreach (var item in propertyInfos)
        {
            var desc = type.GetPropertyDisplayName(item.Name);
            row.TryAdd(desc ?? item.Name, item.GetValue(variable.AlarmPropertys)?.ToString());
        }

        return row;
    }


    static IEnumerable<Dictionary<string, object>> GetPluginSheets(
    IEnumerable<Variable> data,
    IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
    IReadOnlyDictionary<long, ChannelRuntime> channelDicts,
    string plugin,
    Dictionary<string, VariablePropertyBase> pluginDrivers,
    NonBlockingDictionary<string, (VariablePropertyBase, Dictionary<string, PropertyInfo>)> propertysDict)
    {
        if (!pluginDrivers.TryGetValue(plugin, out var variablePropertyBase))
            yield break;

        if (!propertysDict.TryGetValue(plugin, out var propertys))
        {
            var driverProperties = variablePropertyBase;
            var driverPropertyType = driverProperties.GetType();
            propertys.Item1 = driverProperties;
            propertys.Item2 = driverPropertyType.GetRuntimeProperties()
                .Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null)
                .ToDictionary(
                    a => driverPropertyType.GetPropertyDisplayName(a.Name, a => a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description));
            propertysDict.TryAdd(plugin, propertys);
        }
        if (propertys.Item2?.Count == null || propertys.Item2?.Count == 0)
        {
            yield break;
        }
        foreach (var variable in data)
        {
            if (variable.VariablePropertys == null)
                continue;

            foreach (var item in variable.VariablePropertys)
            {
                if (!(deviceDicts.TryGetValue(item.Key, out var businessDevice) &&
                      deviceDicts.TryGetValue(variable.DeviceId, out var collectDevice)))
                    continue;

                channelDicts.TryGetValue(businessDevice.ChannelId, out var channel);
                if (channel?.PluginName != plugin)
                    continue;

                yield return GetPlugin(propertys, variable, item, businessDevice, collectDevice);
            }
        }
    }

    private static Dictionary<string, object> GetPlugin((VariablePropertyBase, Dictionary<string, PropertyInfo>) propertys, Variable variable, KeyValuePair<long, Dictionary<string, string>> item, Device businessDevice, Device collectDevice)
    {
        var row = new Dictionary<string, object>
            {
                { GatewayExportString.DeviceName, collectDevice.Name },
                { GatewayExportString.BusinessDeviceName, businessDevice.Name },
                { GatewayExportString.VariableName, variable.Name }
            };

        foreach (var kv in propertys.Item2)
        {
            var propDict = item.Value;
            if (propDict.TryGetValue(kv.Value.Name, out var dependencyProperty))
            {
                row.TryAdd(kv.Key, dependencyProperty);
            }
            else
            {
                row.TryAdd(kv.Key, ThingsGatewayStringConverter.Default.Serialize(null, kv.Value.GetValue(propertys.Item1)));
            }
        }

        return row;
    }

    public static Dictionary<string, object> ExportSheets(
    IAsyncEnumerable<Variable> data,
    IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
    IReadOnlyDictionary<long, ChannelRuntime> channelDicts,
    Dictionary<string, VariablePropertyBase> pluginDrivers,
    string? deviceName = null)
    {
        var sheets = new Dictionary<string, object>();
        var propertysDict = new NonBlockingDictionary<string, (VariablePropertyBase, Dictionary<string, PropertyInfo>)>();

        // 主变量页
        sheets.Add(GatewayExportString.VariableName, GetVariableSheets(data, deviceDicts, deviceName));

        // 插件页（动态推导）
        foreach (var plugin in pluginDrivers.Keys.Distinct())
        {
            var filtered = FilterPluginDevices(data, plugin, deviceDicts, channelDicts);
            var pluginName = PluginInfoUtil.GetFileNameAndTypeName(plugin).Item2;
            sheets.Add(pluginName, GetPluginSheets(filtered, deviceDicts, channelDicts, plugin, pluginDrivers, propertysDict));
        }

        return sheets;
    }

    static async IAsyncEnumerable<Dictionary<string, object>> GetVariableSheets(
    IAsyncEnumerable<Variable> data,
    IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
    string? deviceName)
    {
        var type = typeof(Variable);
        var propertyInfos = type.GetRuntimeProperties()
            .Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
            .OrderBy(a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
                if (order < 0) order += 10000000;
                else if (order == 0) order = 10000000;
                return order;
            });

        await foreach (var variable in data.ConfigureAwait(false))
        {
            yield return GetVariable(deviceDicts, deviceName, type, propertyInfos, variable);
        }
    }

    static async IAsyncEnumerable<Dictionary<string, object>> GetPluginSheets(
    IAsyncEnumerable<Variable> data,
    IReadOnlyDictionary<long, DeviceRuntime> deviceDicts,
    IReadOnlyDictionary<long, ChannelRuntime> channelDicts,
    string plugin,
    Dictionary<string, VariablePropertyBase> pluginDrivers,
    NonBlockingDictionary<string, (VariablePropertyBase, Dictionary<string, PropertyInfo>)> propertysDict)
    {
        if (!pluginDrivers.TryGetValue(plugin, out var variablePropertyBase))
            yield break;

        if (!propertysDict.TryGetValue(plugin, out var propertys))
        {
            var driverProperties = variablePropertyBase;
            var driverPropertyType = driverProperties.GetType();
            propertys.Item1 = driverProperties;
            propertys.Item2 = driverPropertyType.GetRuntimeProperties()
                .Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null)
                .ToDictionary(
                    a => driverPropertyType.GetPropertyDisplayName(a.Name, a => a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description));
            propertysDict.TryAdd(plugin, propertys);
        }
        if (propertys.Item2?.Count == null || propertys.Item2?.Count == 0)
        {
            yield break;
        }
        await foreach (var variable in data.ConfigureAwait(false))
        {
            if (variable.VariablePropertys == null)
                continue;

            foreach (var item in variable.VariablePropertys)
            {
                if (!(deviceDicts.TryGetValue(item.Key, out var businessDevice) &&
                      deviceDicts.TryGetValue(variable.DeviceId, out var collectDevice)))
                    continue;

                channelDicts.TryGetValue(businessDevice.ChannelId, out var channel);
                if (channel?.PluginName != plugin)
                    continue;

                yield return GetPlugin(propertys, variable, item, businessDevice, collectDevice);
            }
        }
    }

    public static Dictionary<string, object> ExportCore(IEnumerable<Variable> data, string deviceName = null, string sortName = nameof(Variable.Id), SortOrder sortOrder = SortOrder.Asc)
    {
        if (data?.Any() != true)
        {
            data = new List<Variable>();
        }
        var deviceDicts = GlobalData.IdDevices;
        var channelDicts = GlobalData.IdChannels;
        var driverPluginDicts = GlobalData.PluginService.GetPluginList(PluginTypeEnum.Business).ToDictionary(a => a.FullName);
        //总数据
        Dictionary<string, object> sheets = new();
        //变量页
        ConcurrentList<Dictionary<string, object>> variableExports = new();
        ConcurrentList<Dictionary<string, object>> alarmExports = new();
        //变量附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
        NonBlockingDictionary<string, ConcurrentList<Dictionary<string, object>>> devicePropertys = new();
        NonBlockingDictionary<string, (VariablePropertyBase, Dictionary<string, PropertyInfo>)> propertysDict = new();

        #region 列名称

        var type = data.FirstOrDefault()?.GetType() ?? typeof(Variable);
        var propertyInfos = type.GetRuntimeProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
             .OrderBy(
            a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
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

        var alarmPropertysType = typeof(AlarmPropertys);
        var alarmPropertysInfos = alarmPropertysType.GetRuntimeProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>(false) == null)
             .OrderBy(
            a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue;
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
        data.ParallelForEachStreamed((variable, state, index) =>
        {
            {
                Dictionary<string, object> varExport = new();
                deviceDicts.TryGetValue(variable.DeviceId, out var device);
                //设备实体没有包含设备名称，手动插入
                varExport.TryAdd(GatewayExportString.DeviceName, device?.Name ?? deviceName);
                foreach (var item in propertyInfos)
                {
                    if (item.Name == nameof(Variable.Id))
                    {
                        if (sortName != nameof(Variable.Id))
                            continue;
                    }
                    //描述
                    var desc = type.GetPropertyDisplayName(item.Name);
                    //数据源增加
                    varExport.TryAdd(desc ?? item.Name, item.GetValue(variable)?.ToString());
                }

                //添加完整设备信息
                variableExports.Add(varExport);

            }
            if (variable.AlarmPropertys != null)
            {
                Dictionary<string, object> alarmExport = new();
                deviceDicts.TryGetValue(variable.DeviceId, out var device);
                //设备实体没有包含设备名称，手动插入
                alarmExport.TryAdd(GatewayExportString.DeviceName, device?.Name ?? deviceName);
                alarmExport.TryAdd(GatewayExportString.VariableName, variable.Name);
                foreach (var item in alarmPropertysInfos)
                {
                    //描述
                    var desc = alarmPropertysType.GetPropertyDisplayName(item.Name);
                    //数据源增加
                    alarmExport.TryAdd(desc ?? item.Name, item.GetValue(variable.AlarmPropertys)?.ToString());
                }

                //添加完整设备信息
                alarmExports.Add(alarmExport);
            }
            #region 插件sheet
            if (variable.VariablePropertys != null)
            {
                foreach (var item in variable.VariablePropertys)
                {
                    //插件属性
                    //单个设备的行数据
                    Dictionary<string, object> driverInfo = new();
                    if (!(deviceDicts.TryGetValue(item.Key, out var businessDevice) && deviceDicts.TryGetValue(variable.DeviceId, out var collectDevice)))
                        continue;

                    channelDicts.TryGetValue(businessDevice.ChannelId, out var channel);

                    //没有包含设备名称，手动插入
                    driverInfo.TryAdd(GatewayExportString.DeviceName, collectDevice.Name);
                    driverInfo.TryAdd(GatewayExportString.BusinessDeviceName, businessDevice.Name);
                    driverInfo.TryAdd(GatewayExportString.VariableName, variable.Name);

                    var propDict = item.Value;

                    if (propertysDict.TryGetValue(channel.PluginName, out var propertys))
                    {
                    }
                    else
                    {
                        try
                        {
                            var variableProperty = ((BusinessBase)GlobalData.PluginService.GetDriver(channel.PluginName))?.VariablePropertys;
                            propertys.Item1 = variableProperty;
                            var variablePropertyType = variableProperty.GetType();
                            propertys.Item2 = variablePropertyType.GetRuntimeProperties()
               .Where(a => a.GetCustomAttribute<DynamicPropertyAttribute>() != null)
               .ToDictionary(a => variablePropertyType.GetPropertyDisplayName(a.Name, a =>
               a.GetCustomAttribute<DynamicPropertyAttribute>(true)?.Description));
                            propertysDict.TryAdd(channel.PluginName, propertys);
                        }
                        catch
                        {
                        }
                    }
                    if (propertys.Item2?.Count == null)
                    {
                        continue;
                    }
                    //根据插件的配置属性项生成列，从数据库中获取值或者获取属性默认值
                    foreach (var item1 in propertys.Item2)
                    {
                        if (propDict.TryGetValue(item1.Value.Name, out var dependencyProperty))
                        {
                            driverInfo.TryAdd(item1.Key, dependencyProperty);
                        }
                        else
                        {
                            //添加对应属性数据
                            driverInfo.TryAdd(item1.Key, ThingsGatewayStringConverter.Default.Serialize(null, item1.Value.GetValue(propertys.Item1)));
                        }
                    }

                    if (!driverPluginDicts.ContainsKey(channel.PluginName))
                        continue;

                    var pluginName = PluginInfoUtil.GetFileNameAndTypeName(channel.PluginName);
                    //lock (devicePropertys)
                    {
                        if (devicePropertys.ContainsKey(pluginName.Item2))
                        {
                            if (driverInfo.Count > 0)
                                devicePropertys[pluginName.Item2].Add(driverInfo);
                        }
                        else
                        {
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
                    }
                }
            }

            #endregion 插件sheet
        });

        var sort = type.GetPropertyDisplayName(sortName);
        if (variableExports.FirstOrDefault()?.ContainsKey(sort) == false)
            sort = nameof(Variable.Id);
        if (variableExports.FirstOrDefault()?.ContainsKey(sort) == false)
            sort = type.GetPropertyDisplayName(nameof(Variable.Name));

        variableExports = new(
            sortOrder == SortOrder.Desc ?
            variableExports.OrderByDescending(a => a[sort])
            :
            variableExports.OrderBy(a => a[sort])
            );

        if (sortName == nameof(Variable.Id))
            variableExports.ForEach(a => a.Remove("Id"));

        //添加设备页
        sheets.Add(GatewayExportString.VariableName, variableExports);
        sheets.Add(GatewayExportString.AlarmName, alarmExports);

        //HASH
        foreach (var item in devicePropertys.Keys)
        {
            devicePropertys[item] = new(devicePropertys[item].OrderBy(a => a[GatewayExportString.DeviceName]).ThenBy(a => a[GatewayExportString.VariableName]));

            sheets.Add(item, devicePropertys[item]);
        }

        return sheets;
    }

    /// <inheritdoc/>
    public static async Task<Dictionary<string, ImportPreviewOutputBase>> ImportAsync(USheetDatas uSheetDatas)
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);

        // 获取所有设备的字典，以设备名称作为键
        var deviceDicts = GlobalData.Devices;

        // 存储导入检验结果的字典
        Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();

        // 设备页导入预览输出
        ImportPreviewOutput<Dictionary<string, Variable>> deviceImportPreview = new();
        var plugins = GlobalData.PluginService.GetPluginList();
        // 获取驱动插件的全名和名称的字典
        var driverPluginFullNameDict = plugins.ToDictionary(a => a.FullName);
        var driverPluginNameDict = plugins.ToDictionary(a => a.Name);
        NonBlockingDictionary<string, (Type, Dictionary<string, PropertyInfo>, Dictionary<string, PropertyInfo>)> propertysDict = new();

        var sheetNames = uSheetDatas.sheets.Keys.ToList();
        foreach (var sheetName in sheetNames)
        {
            List<IDictionary<string, object>> rows = new();
            var first = uSheetDatas.sheets[sheetName].cellData.FirstOrDefault().Value;

            foreach (var item in uSheetDatas.sheets[sheetName].cellData)
            {
                if (item.Key == 0)
                {
                    continue;
                }
                var expando = new Dictionary<string, object>();
                foreach (var keyValue in item.Value)
                {
                    expando.Add(first[keyValue.Key].v?.ToString(), keyValue.Value.v);
                }
                rows.Add(expando);
            }

            deviceImportPreview = GlobalData.VariableService.SetVariableData(dataScope, deviceDicts, ImportPreviews, deviceImportPreview, driverPluginNameDict, propertysDict, sheetName, rows);
            if (ImportPreviews.Any(a => a.Value.HasError))
            {
                throw new(ImportPreviews.FirstOrDefault(a => a.Value.HasError).Value.Results.FirstOrDefault(a => !a.Success).ErrorMessage ?? "error");
            }
        }

        return ImportPreviews;
    }
}