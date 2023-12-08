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

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Gateway.Core.Extensions;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// 读取字符串，DateTime等等不确定返回字节数量的方法属性特殊方法，需使用<see cref="DeviceMethodAttribute"/>特性标识
/// </summary>
public abstract class CollectBase : DriverBase
{
    public CollectBase() : base()
    {
        Methods = _driverPluginService.GetDriverMethodInfo(this);
    }

    public new CollectDeviceRunTime CurrentDevice { get; set; }

    /// <summary>
    /// 特殊方法
    /// </summary>
    public List<DependencyPropertyWithMethodInfo> Methods { get; set; }

    public override async Task AfterStopAsync()
    {
        //去除全局设备变量
        lock (_globalDeviceData.CollectDevices)
        {
            _globalDeviceData.CollectDevices.RemoveWhere(it => it.Id == DeviceId);
        }
        await base.AfterStopAsync();
    }

    public override void Init(DeviceRunTime device)
    {
        base.Init(device);
        Logger = _serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger($"南向设备：{device.Name}");
        CurrentDevice = device as CollectDeviceRunTime;
        lock (_globalDeviceData.CollectDevices)
        {
            _globalDeviceData.CollectDevices.RemoveWhere(it => it.Id == device.Id);
            _globalDeviceData.CollectDevices.Add(CurrentDevice);
        }
    }

    /// <summary>
    /// 采集驱动读取，读取成功后直接赋值变量，失败不做处理，注意非IReadWrite设备需重写
    /// </summary>
    public virtual async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        OperResult<byte[]> read = await _readWrite.ReadAsync(deviceVariableSourceRead.Address, deviceVariableSourceRead.Length, cancellationToken);
        if (read?.IsSuccess == true)
        {
            deviceVariableSourceRead.DeviceVariableRunTimes.PraseStructContent(_readWrite, read.Content);
        }
        return read;
    }

    /// <summary>
    /// 批量写入变量值,需返回变量名称/结果，注意非IReadWrite设备需重写
    /// </summary>
    /// <returns></returns>
    public virtual async Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<DeviceVariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        if (_readWrite == null)
            throw new($"无法写入数据，{nameof(_readWrite)}为null");
        Dictionary<string, OperResult> operResults = new();
        foreach (var writeInfo in writeInfoLists)
        {
            var result = await _readWrite.WriteAsync(writeInfo.Key.Address, writeInfo.Value.ToString(), writeInfo.Value.CalculateActualValueRank(), writeInfo.Key.DataTypeEnum, cancellationToken);
            operResults.Add(writeInfo.Key.Name, result);
        }
        return operResults;
    }

    /// <summary>
    /// 注意非IReadWrite设备需重写
    /// </summary>
    /// <returns></returns>
    protected virtual string GetAddressDescription()
    {
        return _readWrite?.GetAddressDescription();
    }

    protected override void Init(ISenderClient client = null)
    {
        LoadSourceRead(CurrentDevice.DeviceVariableRunTimes);
    }

    /// <summary>
    /// 获取设备变量打包列表/特殊方法列表
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    protected virtual void LoadSourceRead(List<DeviceVariableRunTime> collectVariableRunTimes)
    {
        var currentDevice = CurrentDevice;
        if (CurrentDevice == null)
        {
            Logger?.LogWarning($"{nameof(CurrentDevice)}不能为null");
            return;
        }
        try
        {
            //连读打包
            var tags = collectVariableRunTimes
                .Where(it => it.ProtectTypeEnum != ProtectTypeEnum.WriteOnly
                && string.IsNullOrEmpty(it.OtherMethod)
                && !string.IsNullOrEmpty(it.Address)).ToList();
            currentDevice.DeviceVariableSourceReads = this.ProtectedLoadSourceRead(tags);
        }
        catch
        {
            throw new($"变量打包失败，请查看变量地址是否正确，变量示例：{GetAddressDescription()}");
        }

        var variablesMethod = collectVariableRunTimes.Where(it => !string.IsNullOrEmpty(it.OtherMethod));

        {
            var tag = variablesMethod.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.WriteOnly);
            List<DeviceVariableMethodSource> variablesMethodResult1 = GetMethod(tag, false);
            currentDevice.DeviceVariableMethodReads = variablesMethodResult1;
        }

        {
            var tag = variablesMethod.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.ReadOnly);
            List<DeviceVariableMethodSource> variablesMethodResult2 = GetMethod(tag, true);
            currentDevice.DeviceVariableMethodSources = variablesMethodResult2;
        }

        List<DeviceVariableMethodSource> GetMethod(IEnumerable<DeviceVariableRunTime> tag, bool init)
        {
            var variablesMethodResult = new List<DeviceVariableMethodSource>();
            foreach (var item in tag)
            {
                var methodResult = new DeviceVariableMethodSource(item.IntervalTime ?? item.CollectDeviceRunTime.IntervalTime);
                var method = Methods.FirstOrDefault(it => it.Description == item.OtherMethod);
                if (method != null)
                {
                    methodResult.MethodInfo = new Method(method.MethodInfo);
                    methodResult.MethodStr = item.Address;

                    if (init)
                    {
                        //获取实际执行的参数列表
                        var ps = methodResult.MethodInfo.Info.GetParameters();
                        methodResult.MethodObj = new object[ps.Length];

                        if (!string.IsNullOrEmpty(methodResult.MethodStr))
                        {
                            string[] strs = methodResult.MethodStr?.Trim()?.TrimEnd(',').Split(',');
                            try
                            {
                                int index = 0;
                                for (int i = 0; i < ps.Length; i++)
                                {
                                    if (typeof(CancellationToken).IsAssignableFrom(ps[i].ParameterType))
                                    {
                                        methodResult.HasTokenObj = true;
                                    }
                                    else
                                    {
                                        if (strs.Length <= index)
                                            continue;
                                        //得到对于的方法参数值
                                        methodResult.MethodObj[i] = methodResult.Converter.ConvertFrom(strs[index], ps[i].ParameterType);
                                        index++;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new($"特殊方法初始化失败，请查看变量地址/传入参数是否正确", ex);
                            }
                        }
                    }

                    methodResult.DeviceVariable = item;
                    variablesMethodResult.Add(methodResult);
                }
            }

            return variablesMethodResult;
        }
    }

    /// <summary>
    /// 执行读取等方法，如果插件不支持读取，而是自更新值的话，需重写此方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        int deviceMethodsVariableSuccessNum = 0;
        int deviceMethodsVariableFailedNum = 0;
        int deviceSourceVariableSuccessNum = 0;
        int deviceSourceVariableFailedNum = 0;

        foreach (var deviceVariableSourceRead in CurrentDevice.DeviceVariableSourceReads)
        {
            if (KeepRun != true)
                continue;

            if (cancellationToken.IsCancellationRequested)
                break;

            //连读变量
            if (deviceVariableSourceRead.CheckIfRequestAndUpdateTime(DateTimeExtensions.CurrentDateTime))
            {
                try
                {
                    await DeviceThread.EasyLock.WaitAsync();
                    var readErrorCount = 0;
                    var readResult = await ReadSourceAsync(deviceVariableSourceRead, cancellationToken);

                    while (readResult != null && !readResult.IsSuccess && readErrorCount < DriverPropertys.RetryCount)
                    {
                        readErrorCount++;
                        LogMessage?.Trace($"{DeviceName} - 采集[{deviceVariableSourceRead?.Address} - {deviceVariableSourceRead?.Length}] 数据失败 - {readResult?.ToString()}");
                        readResult = await ReadSourceAsync(deviceVariableSourceRead, cancellationToken);
                    }

                    if (readResult != null && readResult.IsSuccess)
                    {
                        LogMessage?.Trace($"{DeviceName} - 采集[{deviceVariableSourceRead?.Address} - {deviceVariableSourceRead?.Length}] 数据成功 - {readResult?.Content?.ToHexString(' ')}");
                        deviceMethodsVariableSuccessNum++;
                    }
                    else
                    {
                        if (readResult != null)
                        {
                            if (deviceVariableSourceRead.LastErrorMessage != readResult?.Message)
                                LogMessage?.Warning($"{DeviceName} - 采集[{deviceVariableSourceRead?.Address} - {deviceVariableSourceRead?.Length}] 数据失败 - {readResult?.ToString()}");
                            else
                                LogMessage?.Trace($"{DeviceName} - 采集[{deviceVariableSourceRead?.Address} - {deviceVariableSourceRead?.Length}] 数据连续失败 - {readResult?.ToString()}");

                            deviceMethodsVariableFailedNum++;
                            deviceVariableSourceRead.SetValue(readResult?.Message);
                            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1, readResult?.Message);
                        }
                    }
                }
                finally
                {
                    DeviceThread.EasyLock.Release();
                }
            }
        }

        foreach (var deviceVariableMethodRead in CurrentDevice.DeviceVariableMethodReads)
        {
            if (KeepRun != true)
                continue;
            if (cancellationToken.IsCancellationRequested)
                break;

            //连读变量
            if (deviceVariableMethodRead.CheckIfRequestAndUpdateTime(DateTimeExtensions.CurrentDateTime))
            {
                try
                {
                    await DeviceThread.EasyLock.WaitAsync();

                    var readErrorCount = 0;
                    var readResult = await InvokeMethodAsync(deviceVariableMethodRead, cancellationToken);

                    while (readResult != null && !readResult.IsSuccess && readErrorCount < DriverPropertys.RetryCount)
                    {
                        readErrorCount++;
                        LogMessage?.Trace($"{DeviceName} - 执行方法[{deviceVariableMethodRead.MethodInfo.Name}] - 失败 - {readResult?.ToString()}");
                        readResult = await InvokeMethodAsync(deviceVariableMethodRead, cancellationToken);
                    }

                    if (readResult != null && readResult.IsSuccess)
                    {
                        LogMessage?.Trace($"{DeviceName} - 执行方法[{deviceVariableMethodRead.MethodInfo.Name}] - 成功 - {readResult?.Content?.ToJsonString(true)}");
                        deviceSourceVariableSuccessNum++;
                    }
                    else
                    {
                        if (readResult != null)
                        {
                            LogMessage?.Warning($"{DeviceName} - 执行方法[{deviceVariableMethodRead.MethodInfo.Name}] - 失败 - {readResult?.ToString()}");
                            deviceSourceVariableFailedNum++;
                            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1, readResult?.ToString());
                        }
                    }

                    if (_globalDeviceData.CollectDevices.Count > 100)
                        await Task.Delay(10);
                }
                finally
                {
                    DeviceThread.EasyLock.Release();
                }
            }
        }

        if (deviceMethodsVariableFailedNum == 0 && deviceSourceVariableFailedNum == 0 && (deviceMethodsVariableSuccessNum != 0 || deviceSourceVariableSuccessNum != 0))
        {
            //只有成功读取一次，失败次数都会清零
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
        }
    }

    /// <summary>
    /// 连读打包，返回实际通讯包信息<see cref="DeviceVariableSourceRead"/>
    /// <br></br>每个驱动打包方法不一样，所以需要实现这个接口
    /// </summary>
    /// <param name="deviceVariables">设备下的全部通讯点位</param>
    /// <returns></returns>
    protected abstract List<DeviceVariableSourceRead> ProtectedLoadSourceRead(List<DeviceVariableRunTime> deviceVariables);

    #region 写入方法

    /// <summary>
    /// 执行特殊方法
    /// </summary>
    internal async Task<OperResult<JToken>> InvokeMethodAsync(DeviceVariableMethodSource deviceVariableMethodSource, bool isRead, string value, CancellationToken cancellationToken)
    {
        try
        {
            await DeviceThread.EasyLock.WaitAsync();
            OperResult<JToken> result = new OperResult<JToken>();
            var method = deviceVariableMethodSource.MethodInfo;
            if (method == null)
            {
                result.ErrorCode = 999;
                result.Message = $"{deviceVariableMethodSource.DeviceVariable.Name}找不到执行方法{deviceVariableMethodSource.DeviceVariable.OtherMethod}";
                return result;
            }
            else
            {
                if (!isRead)
                {
                    //获取执行参数
                    var ps = method.Info.GetParameters();
                    deviceVariableMethodSource.MethodObj = new object[ps.Length];

                    if (!string.IsNullOrEmpty(deviceVariableMethodSource.MethodStr) || !string.IsNullOrEmpty(value))
                    {
                        var strs1 = deviceVariableMethodSource.MethodStr?.Trim()?.TrimEnd(',').Split(',') ?? Array.Empty<string>();
                        var strs2 = value?.Trim()?.TrimEnd(',').Split(',') ?? Array.Empty<string>();
                        //通过分号分割，并且合并参数
                        var strs = GenericExtensions.SpliceArray(strs1, strs2);
                        int index = 0;
                        for (int i = 0; i < ps.Length; i++)
                        {
                            if (typeof(CancellationToken).IsAssignableFrom(ps[i].ParameterType))
                            {
                                deviceVariableMethodSource.MethodObj[i] = cancellationToken;
                            }
                            else
                            {
                                if (index >= strs.Length)
                                {
                                    if (ps[i].HasDefaultValue)
                                    {
                                        break;
                                    }

                                    result.ErrorCode = 999;
                                    result.Message = $"{deviceVariableMethodSource.DeviceVariable.Name} 执行方法 {deviceVariableMethodSource.DeviceVariable.OtherMethod} 参数不足{deviceVariableMethodSource.MethodStr}";
                                    //参数数量不符
                                    return result;
                                }

                                deviceVariableMethodSource.MethodObj[i] = deviceVariableMethodSource.Converter.ConvertFrom(strs[index], ps[i].ParameterType);
                                index++;
                            }
                        }
                    }
                }
                else if (deviceVariableMethodSource.HasTokenObj)
                {
                    var ps = method.Info.GetParameters();
                    var newObjs = deviceVariableMethodSource.MethodObj.ToList();

                    if (!string.IsNullOrEmpty(deviceVariableMethodSource.MethodStr) || !string.IsNullOrEmpty(value))
                    {
                        for (int i = 0; i < ps.Length; i++)
                        {
                            if (typeof(CancellationToken).IsAssignableFrom(ps[i].ParameterType))
                            {
                                newObjs.Insert(i, cancellationToken);
                            }
                        }
                    }

                    deviceVariableMethodSource.MethodObj = newObjs.ToArray();
                }

                try
                {
                    object data = null;
                    switch (method.TaskType)
                    {
                        case TaskReturnType.None:
                            data = method.Invoke(this, deviceVariableMethodSource.MethodObj);
                            break;

                        case TaskReturnType.Task:
                            await method.InvokeAsync(this, deviceVariableMethodSource.MethodObj);
                            break;

                        case TaskReturnType.TaskObject:
                            //执行方法
                            data = await method.InvokeObjectAsync(this, deviceVariableMethodSource.MethodObj);
                            break;
                    }

                    var result1 = data?.Adapt<OperResult<object>>();
                    if (result1 != null)
                    {
                        result = new(result1);
                        if (result.IsSuccess)
                            result.Content = JToken.FromObject(result1.Content);
                    }
                    if (method.HasReturn)
                    {
                        if (result?.IsSuccess == true)
                        {
                            var content = deviceVariableMethodSource.Converter.ConvertTo(result.Content?.ToString()?.Replace($"\0", ""));
                            if (isRead)
                                deviceVariableMethodSource.DeviceVariable.SetValue(content);
                        }
                        else
                        {
                            deviceVariableMethodSource.DeviceVariable.SetValue(null, lastErrorMessage: result?.Message ?? "未知错误");
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    result.ErrorCode = 999;
                    result.Message = $"{deviceVariableMethodSource.DeviceVariable.Name}执行{deviceVariableMethodSource.DeviceVariable.OtherMethod} 方法失败:{ex}";
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            return new(ex);
        }
        finally
        {
            DeviceThread.EasyLock.Release();
        }
    }

    /// <summary>
    /// 执行变量写入
    /// </summary>
    /// <returns></returns>
    internal async Task<Dictionary<string, OperResult>> InVokeWriteAsync(Dictionary<DeviceVariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        try
        {
            await DeviceThread.EasyLock.WaitAsync();
            Dictionary<string, OperResult> results = new Dictionary<string, OperResult>();
            foreach (var (deviceVariable, jToken) in writeInfoLists)
            {
                if (!string.IsNullOrEmpty(deviceVariable.WriteExpressions))
                {
                    object rawdata = jToken is JValue jValue ? jValue.Value : jToken.ToString();
                    try
                    {
                        object data = deviceVariable.WriteExpressions.GetExpressionsResult(rawdata);
                        writeInfoLists[deviceVariable] = JToken.FromObject(data);
                    }
                    catch (Exception ex)
                    {
                        results.Add(deviceVariable.Name, new OperResult($"{deviceVariable.Name} 转换写入表达式 {deviceVariable.WriteExpressions} 失败：{ex}"));
                    }
                }
            }

            var result = await WriteValuesAsync(writeInfoLists.
                Where(a => !results.Any(b => b.Key == a.Key.Name)).
               ToDictionary(item => item.Key, item => item.Value),
                cancellationToken);

            return result;
        }
        finally
        {
            DeviceThread.EasyLock.Release();
        }
    }

    /// <summary>
    /// 执行特殊方法读取,并设置变量值
    /// </summary>
    private async Task<OperResult<JToken>> InvokeMethodAsync(DeviceVariableMethodSource deviceVariableMethodRead, CancellationToken cancellationToken)
    {
        return await InvokeMethodAsync(deviceVariableMethodRead, true, string.Empty, cancellationToken);
    }

    #endregion
}