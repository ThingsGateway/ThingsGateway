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

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// 读取字符串，DateTime等等不确定返回字节数量的方法属性特殊方法，需使用<see cref="DeviceMethodAttribute"/>特性标识
/// </summary>
public abstract class CollectBase : DriverBase
{
    public new CollectDeviceRunTime CurrentDevice { get; set; }

    /// <summary>
    /// 特殊方法
    /// </summary>
    public List<DependencyPropertyWithInfo>? DeviceMethods { get; private set; }

    public override async Task AfterStopAsync()
    {
        //去除全局设备变量
        lock (GlobalData.CollectDevices)
        {
            GlobalData.CollectDevices.RemoveWhere(it => it.Id == DeviceId);
        }
        await base.AfterStopAsync();
    }

    internal override void Init(DeviceRunTime device)
    {
        base.Init(device);
        CurrentDevice = device as CollectDeviceRunTime;
        var data = PluginService.GetDriverMethodInfos(device.PluginName, this).Values.ToList();

        DeviceMethods = data;

        lock (GlobalData.CollectDevices)
        {
            GlobalData.CollectDevices.RemoveWhere(it => it.Id == device.Id);
            GlobalData.CollectDevices.Add(CurrentDevice);
        }
    }

    /// <summary>
    /// 采集驱动读取，读取成功后直接赋值变量，失败不做处理，注意非通用设备需重写
    /// </summary>
    protected virtual async Task<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
    {
        if (IsSingleThread)
        {
            while (WriteLock.IsWaitting)
            {
                //等待写入完成
                await Task.Delay(100);
            }
        }

        OperResult<byte[]> read = await Protocol.ReadAsync(variableSourceRead.RegisterAddress, variableSourceRead.Length, cancellationToken);
        Interlocked.Increment(ref variableSourceRead.ReadCount);
        if (read?.IsSuccess == true)
        {
            variableSourceRead.VariableRunTimes.PraseStructContent(Protocol, read.Content, variableSourceRead);
        }
        return read;
    }

    /// <summary>
    /// 批量写入变量值,需返回变量名称/结果，注意非通用设备需重写
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        try
        {
            if (IsSingleThread)
                await WriteLock.WaitAsync(cancellationToken);
            if (Protocol == null)
                throw new NotSupportedException($"不支持写入数据，{nameof(Protocol)}为null");
            ConcurrentDictionary<string, OperResult> operResults = new();
            await writeInfoLists.ParallelForEachAsync(async (writeInfo, cancellationToken) =>
            //foreach (var writeInfo in writeInfoLists)
                {
                    var result = await Protocol.WriteAsync(writeInfo.Key.RegisterAddress, writeInfo.Value, writeInfo.Key.DataType, cancellationToken);
                    operResults.TryAdd(writeInfo.Key.Name, result);
                }, DriverPropertys.ConcurrentCount, cancellationToken);
            return new(operResults);
        }
        finally
        {
            if (IsSingleThread)
                WriteLock.Release();
        }
    }

    /// <summary>
    /// 注意非通用设备需重写
    /// </summary>
    /// <returns></returns>
    protected virtual string GetAddressDescription()
    {
        return Protocol?.GetAddressDescription();
    }

    /// <summary>
    /// 获取设备变量打包列表/特殊方法列表
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    public override void LoadSourceRead(List<VariableRunTime> collectVariableRunTimes)
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
                .Where(it => it.ProtectType != ProtectTypeEnum.WriteOnly
                && string.IsNullOrEmpty(it.OtherMethod)
                && !string.IsNullOrEmpty(it.RegisterAddress)).ToList();
            currentDevice.VariableSourceReads = this.ProtectedLoadSourceRead(tags);
        }
        catch (Exception ex)
        {
            currentDevice.VariableSourceReads = new();
            LogMessage.LogWarning(ex, $"变量打包失败，请查看变量地址是否正确，变量示例：{GetAddressDescription()}");
        }
        try
        {
            var variablesMethod = collectVariableRunTimes.Where(it => !string.IsNullOrEmpty(it.OtherMethod));

            {
                var tag = variablesMethod.Where(it => it.ProtectType != ProtectTypeEnum.WriteOnly);
                List<VariableMethod> variablesMethodResult = GetMethod(tag);
                currentDevice.ReadVariableMethods = variablesMethodResult;
            }

            {
                var tag = variablesMethod.Where(it => it.ProtectType != ProtectTypeEnum.ReadOnly);
                List<VariableMethod> variablesMethodResult = GetMethod(tag);
                currentDevice.VariableMethods = variablesMethodResult;
            }
        }
        catch (Exception ex)
        {
            currentDevice.ReadVariableMethods ??= new();
            currentDevice.VariableMethods ??= new();
            LogMessage.LogWarning(ex, $"动态方法初始化失败");
        }
        List<VariableMethod> GetMethod(IEnumerable<VariableRunTime> tag)
        {
            var variablesMethodResult = new List<VariableMethod>();
            foreach (var item in tag)
            {
                var method = DeviceMethods.FirstOrDefault(it => it.Description == item.OtherMethod);
                if (method != null)
                {
                    var methodResult = new VariableMethod(new Method(method.MethodInfo), item, item.IntervalTime ?? item.CollectDeviceRunTime.IntervalTime);
                    variablesMethodResult.Add(methodResult);
                }
                else
                {
                    throw new($"特色方法变量{item.Name} 找不到执行方法 {item.OtherMethod},请查看现有方法列表");
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

        await CurrentDevice.VariableSourceReads.ParallelForEachAsync(async (variableSourceRead, cancellationToken) =>
        //foreach (var variableSource in CurrentDevice.VariableSourceReads)
          {
              if (KeepRun != true)
                  return;
              if (cancellationToken.IsCancellationRequested)
                  return;

              //连读变量
              if (variableSourceRead.CheckIfRequestAndUpdateTime(DateTimeUtil.Now))
              {
                  var readErrorCount = 0;
                  var readResult = await ReadSourceAsync(variableSourceRead, cancellationToken);
                  //读取一定次数后，判定失败
                  while (readResult != null && !readResult.IsSuccess && readErrorCount < DriverPropertys.RetryCount)
                  {
                      if (KeepRun != true)
                          return;
                      if (cancellationToken.IsCancellationRequested)
                          return;
                      readErrorCount++;
                      LogMessage?.Trace($"{DeviceName} - 采集[{variableSourceRead?.RegisterAddress} - {variableSourceRead?.Length}] 数据失败 - {readResult?.ToString()}");
                      readResult = await ReadSourceAsync(variableSourceRead, cancellationToken);
                  }

                  if (readResult != null && readResult.IsSuccess)
                  {
                      LogMessage?.Trace($"{DeviceName} - 采集[{variableSourceRead?.RegisterAddress} - {variableSourceRead?.Length}] 数据成功 - {readResult?.Content?.ToHexString(' ')}");
                      deviceSourceVariableSuccessNum++;
                  }
                  else
                  {
                      if (readResult != null)
                      {
                          if (variableSourceRead.LastErrorMessage != readResult?.ErrorMessage)
                              LogMessage?.LogWarning(readResult.Exception, $"{DeviceName} - 采集[{variableSourceRead?.RegisterAddress} - {variableSourceRead?.Length}] 数据失败 - {readResult?.ErrorMessage}");
                          else
                              LogMessage?.Trace($"{DeviceName} - 采集[{variableSourceRead?.RegisterAddress} - {variableSourceRead?.Length}] 数据连续失败 - {readResult?.ToString()}");

                          deviceSourceVariableFailedNum++;
                          variableSourceRead.LastErrorMessage = readResult?.ErrorMessage;
                          CurrentDevice.SetDeviceStatus(DateTimeUtil.Now, CurrentDevice.ErrorCount + 1, readResult?.ErrorMessage);
                          variableSourceRead.VariableRunTimes.ForEach(a => a.SetValue(null, isOnline: false));
                      }
                  }
              }
          }, DriverPropertys.ConcurrentCount, cancellationToken);

        await CurrentDevice.ReadVariableMethods.ParallelForEachAsync(async (readVariableMethods, cancellationToken) =>
        //foreach (var readVariableMethods in CurrentDevice.ReadVariableMethods)
        {
            if (KeepRun != true)
                return;
            if (cancellationToken.IsCancellationRequested)
                return;

            //连读变量
            if (readVariableMethods.CheckIfRequestAndUpdateTime(DateTimeUtil.Now))
            {
                var readErrorCount = 0;
                var readResult = await InvokeMethodAsync(readVariableMethods, cancellationToken: cancellationToken);

                while (readResult != null && !readResult.IsSuccess && readErrorCount < DriverPropertys.RetryCount)
                {
                    if (KeepRun != true)
                        return;
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    readErrorCount++;
                    LogMessage?.Trace($"{DeviceName} - 执行方法[{readVariableMethods.MethodInfo.Name}] - 失败 - {readResult?.ToString()}");
                    readResult = await InvokeMethodAsync(readVariableMethods, cancellationToken: cancellationToken);
                }

                if (readResult != null && readResult.IsSuccess)
                {
                    LogMessage?.Trace($"{DeviceName} - 执行方法[{readVariableMethods.MethodInfo.Name}] - 成功 - {readResult?.Content?.ToJsonString(true)}");
                    deviceMethodsVariableSuccessNum++;
                }
                else
                {
                    if (readResult != null)
                    {
                        if (readVariableMethods.LastErrorMessage != readResult?.ErrorMessage)
                            LogMessage?.Warning($"{DeviceName} - 执行方法[{readVariableMethods.MethodInfo.Name}] - 失败 - {readResult?.ToString()}");
                        else
                            LogMessage?.Trace($"{DeviceName} - 执行方法[{readVariableMethods.MethodInfo.Name}] - 失败 - {readResult?.ToString()}");

                        deviceMethodsVariableFailedNum++;
                        readVariableMethods.LastErrorMessage = readResult?.ErrorMessage;
                        CurrentDevice.SetDeviceStatus(DateTimeUtil.Now, CurrentDevice.ErrorCount + 1, readResult?.ToString());
                    }
                }
            }
        }, DriverPropertys.ConcurrentCount, cancellationToken);

        if (deviceMethodsVariableFailedNum == 0 && deviceSourceVariableFailedNum == 0 && (deviceMethodsVariableSuccessNum != 0 || deviceSourceVariableSuccessNum != 0))
        {
            //只有成功读取一次，失败次数都会清零
            CurrentDevice.SetDeviceStatus(DateTimeUtil.Now, 0);
        }
    }

    /// <summary>
    /// 连读打包，返回实际通讯包信息<see cref="VariableSourceRead"/>
    /// <br></br>每个驱动打包方法不一样，所以需要实现这个接口
    /// </summary>
    /// <param name="deviceVariables">设备下的全部通讯点位</param>
    /// <returns></returns>
    protected abstract List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables);

    #region 写入方法

    /// <summary>
    /// 执行特殊方法
    /// </summary>
    internal async Task<OperResult<JToken>> InvokeMethodAsync(VariableMethod variableMethod, string? value = null, bool isRead = true, CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsSingleThread)
                await WriteLock.WaitAsync(cancellationToken);
            OperResult<JToken> result = new OperResult<JToken>();
            var method = variableMethod.MethodInfo;
            if (method == null)
            {
                result.OperCode = 999;
                result.ErrorMessage = $"{variableMethod.Variable.Name}找不到执行方法{variableMethod.Variable.OtherMethod}";
                return result;
            }
            else
            {
                try
                {
                    var data = await variableMethod.InvokeMethodAsync(value, cancellationToken);
                    var result1 = data?.Adapt<OperResult<object>>();
                    if (result1 != null)
                    {
                        result = new(result1);
                        if (result.IsSuccess)
                            result.Content = JToken.FromObject(result1.Content);
                    }
                    if (method.HasReturn)
                    {
                        if (isRead)
                        {
                            if (result?.IsSuccess == true)
                            {
                                var content = variableMethod.Converter.ConvertTo(result.Content?.ToString()?.Replace($"\0", ""));
                                var variableResult = variableMethod.Variable.SetValue(content);
                                if (!variableResult.IsSuccess)
                                    variableMethod.LastErrorMessage = result.ErrorMessage;
                            }
                            else
                            {
                                var variableResult = variableMethod.Variable.SetValue(null, isOnline: false);
                                if (!variableResult.IsSuccess)
                                    variableMethod.LastErrorMessage = result.ErrorMessage;
                            }
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    return new(ex);
                }
            }
        }
        catch (Exception ex)
        {
            return new(ex);
        }
        finally
        {
            if (IsSingleThread)
                WriteLock.Release();
        }
    }

    /// <summary>
    /// 执行变量写入
    /// </summary>
    /// <returns></returns>
    internal async Task<Dictionary<string, OperResult>> InVokeWriteAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
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

        //转换失败的变量不再写入
        var result = await WriteValuesAsync(writeInfoLists.
            Where(a => !results.Any(b => b.Key == a.Key.Name)).
           ToDictionary(item => item.Key, item => item.Value),
            cancellationToken);

        return result;
    }

    #endregion 写入方法
}