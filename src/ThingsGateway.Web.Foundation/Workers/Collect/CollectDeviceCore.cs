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
using Furion.Logging.Extensions;

using Microsoft.Extensions.Logging;

using System.Linq;
using System.Reflection;
using System.Threading;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备子线程服务
/// </summary>
public class CollectDeviceCore : DisposableObject
{
    /// <summary>
    /// 特殊方法变量
    /// </summary>
    public List<DeviceVariableMedRead> DeviceVariableMedReads = new();
    /// <summary>
    /// 特殊方法变量,不参与轮询执行
    /// </summary>
    public List<DeviceVariableMedSource> DeviceVariableMedSources = new();
    /// <summary>
    /// 分包变量
    /// </summary>
    public List<DeviceVariableSourceRead> DeviceVariableSourceReads = new();

    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    public ConcurrentList<CancellationTokenSource> StoppingTokens = new();
    /// <summary>
    /// 当前设备信息
    /// </summary>
    private CollectDeviceRunTime _device;

    /// <summary>
    /// 当前的驱动插件实例
    /// </summary>
    private CollectBase _driver;

    /// <summary>
    /// 日志
    /// </summary>
    private ILogger _logger;
    /// <summary>
    /// 全局插件服务
    /// </summary>
    private PluginSingletonService _pluginService;

    private IServiceScopeFactory _scopeFactory;
    private bool isInitSuccess = true;

    /// <inheritdoc cref="CollectDeviceCore"/>
    public CollectDeviceCore(IServiceScopeFactory scopeFactory)
    {

        _scopeFactory = scopeFactory;
        using var scope = scopeFactory.CreateScope();

        _pluginService = scope.ServiceProvider.GetService<PluginSingletonService>();
        _globalDeviceData = scope.ServiceProvider.GetService<GlobalDeviceData>();
        _driverPluginService = scope.ServiceProvider.GetService<IDriverPluginService>();

    }

    /// <summary>
    /// 当前设备
    /// </summary>
    public CollectDeviceRunTime Device => _device;

    /// <summary>
    /// 当前设备Id
    /// </summary>
    public long DeviceId => (_device?.Id).ToLong();

    /// <summary>
    /// 当前插件
    /// </summary>
    public CollectBase Driver => _driver;

    /// <summary>
    /// 初始化成功
    /// </summary>
    public bool IsInitSuccess => isInitSuccess;

    /// <summary>
    /// 日志
    /// </summary>
    public ILogger Logger => _logger;

    /// <summary>
    /// 当前设备全部特殊方法，执行初始化后获取正确值
    /// </summary>
    public List<MethodInfo> Methods { get; private set; }

    /// <summary>
    /// 当前设备全部设备属性，执行初始化后获取正确值
    /// </summary>
    public List<DependencyProperty> Propertys { get; private set; }

    private IDriverPluginService _driverPluginService { get; set; }

    /// <inheritdoc cref="GlobalDeviceData"/>
    private GlobalDeviceData _globalDeviceData { get; set; }

    #region 设备子线程采集启动停止
    /// <summary>
    /// 暂停采集
    /// </summary>
    public void PasueThread(bool keepRun)
    {
        lock (this)
        {
            var str = keepRun == false ? "设备线程采集暂停" : "设备线程采集继续";
            _logger?.LogInformation($"{str}:{_device.Name}");
            this.Device.KeepRun = keepRun;
        }
    }
    /// <summary>
    /// 停止采集
    /// </summary>
    private void StopThread()
    {
        lock (this)
        {
            CancellationTokenSource StoppingToken = StoppingTokens.LastOrDefault();
            StoppingToken?.Cancel();
            StoppingToken?.SafeDispose();
        }
    }
    #endregion

    #region 插件处理

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <returns></returns>
    private CollectBase CreatDriver()
    {
        try
        {
            var driverPlugin = _driverPluginService.GetDriverPluginById(Device.PluginId);
            _driver = (CollectBase)_pluginService.GetDriver(DeviceId, driverPlugin);
            if (driverPlugin != null)
            {
                Methods = _pluginService.GetMethod(_driver);
                Propertys = _pluginService.GetDriverProperties(_driver);
            }
            else
            {
                throw Oops.Oh($"找不到驱动{driverPlugin.AssembleName}");
            }
            //设置插件配置项
            SetPluginProperties(Device.DevicePropertys);
            return _driver;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_device.Name}初始化失败");
        }
        return null;
    }
    private void InitDriver(object client)
    {
        //初始化插件
        _driver.Init(_logger, _device, client);
        //变量分包
        LoadSourceReads(_device.DeviceVariableRunTimes);
    }
    /// <summary>
    /// 设置驱动插件的属性值
    /// </summary>
    private void SetPluginProperties(List<DependencyProperty> deviceProperties)
    {
        if (deviceProperties == null) return;
        _pluginService.SetDriverProperties(_driver, deviceProperties);
    }

    #endregion

    #region 核心读写
    /// <summary>
    /// 已经停止
    /// </summary>
    public bool IsExited;

    /// <summary>
    /// 是否多个设备共享链路;
    /// </summary>
    public bool IsShareChannel;
    /// <summary>
    /// 开始前
    /// </summary>
    /// <returns></returns>
    internal async Task<bool> BeforeActionAsync(CancellationToken cancellationToken, object client = null)
    {
        try
        {
            IsExited = false;
            _logger?.LogInformation($"{_device.Name}采集设备线程开始");

            StoppingTokens.Add(new());

            if (_driver != null)
            {
                InitDriver(client);
                Device.SourceVariableCount = DeviceVariableSourceReads.Count;
                Device.MethodVariableCount = DeviceVariableMedReads.Count;
            }
            else
            {
                Device.ErrorCount = 999;
                Device.LastErrorMessage = "获取插件失败";
                return false;
            }
            try
            {
                if (Device?.KeepRun == true)
                {
                    //驱动插件执行循环前方法
                    Device.ActiveTime = DateTime.UtcNow;
                    await _driver?.BeforStartAsync(cancellationToken);
                }

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, _device.Name + "开始前发生错误");
                Device.ErrorCount += 1;
                Device.LastErrorMessage = "开始前发生错误：" + ex.Message;
            }
            isInitSuccess = true;
            return isInitSuccess;

        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, _device.Name + "初始化失败");
            Device.ErrorCount = 999;
            Device.LastErrorMessage = "初始化失败：" + ex.Message;
        }
        isInitSuccess = false;
        return isInitSuccess;
    }

    /// <summary>
    /// 结束后
    /// </summary>
    internal async Task FinishActionAsync()
    {
        IsExited = true;
        try
        {
            _logger?.LogInformation($"{_device.Name}采集线程停止中");
            await _driver?.AfterStopAsync();
            _driver?.SafeDispose();
            _logger?.LogInformation($"{_device.Name}采集线程已停止");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{Device.Name} 释放失败");
        }
    }

    /// <summary>
    /// 初始化，在设备子线程创建或更新时才会执行
    /// </summary>
    internal void Init(CollectDeviceRunTime device)
    {
        if (device == null) return;
        try
        {
            bool isUpDevice;
            if (Device == device)
            {
                isUpDevice = false;
            }
            else
            {
                isUpDevice = true;
            }
            _device = device;
            var scope = _scopeFactory.CreateScope();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger("采集设备:" + _device.Name);
            //更新插件信息
            CreatDriver();
            //全局数据更新
            if (isUpDevice)
            {
                _globalDeviceData.CollectDevices.RemoveWhere(it => it.Id == device.Id);
                _globalDeviceData.CollectDevices.Add(device);
            }

        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, device.Name);
        }


    }
    /// <summary>
    /// 运行
    /// </summary>
    internal async Task<ThreadRunReturn> RunActionAsync(CancellationToken cancellationToken)
    {
        try
        {
            using CancellationTokenSource StoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, StoppingTokens.LastOrDefault().Token);
            if (_driver == null) return ThreadRunReturn.Continue;

            if (Device?.KeepRun == false)
            {
                return ThreadRunReturn.Continue;
            }

            if (DeviceVariableSourceReads.Count == 0 && Device.DeviceVariableRunTimes.Where(a => a.OtherMethod.IsNullOrEmpty()).Any())
            {
                Device.ErrorCount = 999;
                Device.LastErrorMessage = "分包失败，请检查变量地址是否符合规则";
                return ThreadRunReturn.Continue;
            }
            int deviceMedsVariableSuccessNum = 0;
            int deviceMedsVariableFailedNum = 0;
            int deviceSourceVariableSuccessNum = 0;
            int deviceSourceVariableFailedNum = 0;
            if (StoppingToken.Token.IsCancellationRequested)
                return ThreadRunReturn.Break;

            Device.ActiveTime = DateTime.UtcNow;

            if (_driver.IsSupportRequest)
            {
                foreach (var deviceVariableSourceRead in DeviceVariableSourceReads)
                {
                    await Task.Delay(10);

                    if (Device?.KeepRun == false)
                    {
                        continue;
                    }

                    if (StoppingToken.Token.IsCancellationRequested)
                        break;

                    //连读变量
                    if (deviceVariableSourceRead.CheckIfRequestAndUpdateTime(DateTime.UtcNow))
                    {
                        var read = await _driver.ReadSourceAsync(deviceVariableSourceRead, StoppingToken.Token);
                        if (read != null && read.IsSuccess)
                        {
                            _logger?.LogTrace(_device.Name + " - " + " - 采集[" + deviceVariableSourceRead.Address + " - " + deviceVariableSourceRead.Length + "] 数据成功" + read.Content?.ToHexString(" "));
                            deviceSourceVariableSuccessNum += 1;
                        }
                        else if (read != null && read.IsSuccess == false)
                        {
                            _logger?.LogWarning(_device.Name + " - " + " - 采集[" + deviceVariableSourceRead.Address + " -" + deviceVariableSourceRead.Length + "] 数据失败 - " + read?.Message);
                            deviceSourceVariableFailedNum += 1;
                            Device.LastErrorMessage = read.Message;
                        }
                    }

                }

                foreach (var deviceVariableMedRead in DeviceVariableMedReads)
                {
                    await Task.Delay(10);
                    if (Device?.KeepRun == false)
                        continue;
                    if (StoppingToken.IsCancellationRequested)
                        break;

                    //连读变量
                    if (deviceVariableMedRead.CheckIfRequestAndUpdateTime(DateTime.UtcNow))
                    {
                        var read = await InvokeMedAsync(deviceVariableMedRead);
                        if (read != null && read.IsSuccess)
                        {
                            _logger?.LogDebug(_device.Name + "执行方法[" + deviceVariableMedRead.MedInfo.Name + "] 成功" + read.Content.ToJson());
                            deviceMedsVariableSuccessNum += 1;
                        }
                        else if ((read != null && read.IsSuccess == false) || read == null)
                        {
                            _logger?.LogWarning(_device.Name + "执行方法[" + deviceVariableMedRead.MedInfo.Name + "] 失败" + read?.Message);
                            deviceMedsVariableFailedNum += 1;
                        }
                    }


                }


                if (deviceMedsVariableFailedNum == 0 && deviceSourceVariableFailedNum == 0 && (deviceMedsVariableSuccessNum != 0 || deviceSourceVariableSuccessNum != 0))
                {
                    //只有成功读取一次，失败次数都会清零
                    Device.ErrorCount = 0;
                }
                else if (deviceMedsVariableFailedNum != 0 || deviceSourceVariableFailedNum != 0)
                {
                    Device.ErrorCount += 1;
                }
            }
            else
            {
                var oper = _driver.IsConnected();
                if (oper.IsSuccess)
                {
                    Device.ErrorCount = 0;
                }
                else
                {
                    Device.ErrorCount = 999;
                    Device.LastErrorMessage = oper.Message;
                }
            }
            if (StoppingToken.Token.IsCancellationRequested)
                return ThreadRunReturn.Break;
            return ThreadRunReturn.None;

        }
        catch (TaskCanceledException)
        {
            return ThreadRunReturn.Break;
        }
        catch (ObjectDisposedException)
        {
            return ThreadRunReturn.Break;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, $"采集线程循环异常{_device.Name}");
            Device.ErrorCount += 1;
            Device.LastErrorMessage = ex.Message;
            return ThreadRunReturn.None;
        }
    }

    #endregion

    #region 分包

    /// <summary>
    /// 获取设备变量分包列表/特殊方法列表
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    private void LoadSourceReads(List<DeviceVariableRunTime> collectVariableRunTimes)
    {
        if (collectVariableRunTimes == null || _driver == null)
        {
            _logger?.LogError("初始化未完成，分包失败");
            return;
        }
        try
        {
            var tag = collectVariableRunTimes.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.WriteOnly &&
            it.OtherMethod.IsNullOrEmpty() && !it.VariableAddress.IsNullOrEmpty()).ToList();
            var result = _driver.LoadSourceRead(tag);
            if (result.IsSuccess)
                DeviceVariableSourceReads = result.Content;
            else
                _logger?.LogError("分包失败,错误信息：" + result.Message);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "分包失败");
        }
        try
        {
            var variablesMed = collectVariableRunTimes.Where(it => !string.IsNullOrEmpty(it.OtherMethod));
            var tag = variablesMed.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.WriteOnly);
            var variablesMedResult = new List<DeviceVariableMedRead>();
            foreach (var item in tag)
            {
                var medResult = new DeviceVariableMedRead(item.IntervalTime);
                var med = Methods.FirstOrDefault(it => it.Name == item.OtherMethod);
                if (med != null)
                {
                    medResult.MedInfo = new Method(med);
                    medResult.MedStr = item.VariableAddress;

                    //获取实际执行的参数列表
                    var ps = medResult.MedInfo.Info.GetParameters();
                    medResult.MedObj = new object[ps.Length];

                    if (!medResult.MedStr.IsNullOrEmpty())
                    {
                        string[] strs = medResult.MedStr?.Trim()?.Split(';');
                        try
                        {
                            for (int i = 0; i < ps.Length; i++)
                            {
                                if (strs.Length <= i)
                                {
                                    throw new($"{medResult.DeviceVariable.Name} 获取执行方法 {medResult.DeviceVariable.OtherMethod} 参数不足{medResult.MedStr}");
                                }
                                medResult.MedObj[i] = medResult.Converter.ConvertFrom(strs[i], ps[i].ParameterType);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "获取方法参数失败");
                            continue;
                        }

                    }

                    medResult.DeviceVariable = item;
                    variablesMedResult.Add(medResult);
                }
            }
            DeviceVariableMedReads = variablesMedResult;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取特殊方法失败");
        }
        try
        {
            var variablesMed = collectVariableRunTimes.Where(it => !string.IsNullOrEmpty(it.OtherMethod));
            var tag = variablesMed.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.ReadOnly);
            var variablesMedResult = new List<DeviceVariableMedSource>();
            foreach (var item in tag)
            {
                var medResult = new DeviceVariableMedSource();
                var med = Methods.FirstOrDefault(it => it.Name == item.OtherMethod);
                if (med != null)
                {
                    medResult.MedInfo = new Method(med);
                    medResult.MedStr = item.VariableAddress;
                    medResult.DeviceVariable = item;
                    variablesMedResult.Add(medResult);
                }
            }
            DeviceVariableMedSources = variablesMedResult;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取特殊方法失败");
        }
    }

    #endregion

    #region 写入与方法
    private EasyLock easyLock = new();

    /// <summary>
    /// 执行特殊方法
    /// </summary>
    internal async Task<OperResult<object>> InvokeMedAsync(DeviceVariableMedSource deviceVariableMedSource, string value)
    {
        OperResult<object> result = new();
        var method = deviceVariableMedSource.MedInfo;
        if (method == null)
        {
            result.ResultCode = ResultCode.Error;
            result.Message = $"{deviceVariableMedSource.DeviceVariable.Name}找不到执行方法{deviceVariableMedSource.DeviceVariable.OtherMethod}";
            return result;
        }
        else
        {
            var ps = method.Info.GetParameters();
            deviceVariableMedSource.MedObj = new object[ps.Length];

            if (!deviceVariableMedSource.MedStr.IsNullOrEmpty() || !value.IsNullOrEmpty())
            {
                string[] strs1 = deviceVariableMedSource.MedStr?.Trim()?.Split(';');
                string[] strs2 = value?.Trim()?.Split(';');
                var strs = strs1?.SpliceArray(strs2);
                int index = 0;
                for (int i = 0; i < ps.Length; i++)
                {
                    if (strs.Length <= i)
                    {
                        result.ResultCode = ResultCode.Error;
                        result.Message = $"{deviceVariableMedSource.DeviceVariable.Name} 执行方法 {deviceVariableMedSource.DeviceVariable.OtherMethod} 参数不足{deviceVariableMedSource.MedStr}";
                        return result;
                    }
                    deviceVariableMedSource.MedObj[i] = deviceVariableMedSource.Converter.ConvertFrom(strs[index], ps[i].ParameterType);
                    index++;
                }
            }
            try
            {
                var data = await method.InvokeObjectAsync(_driver, deviceVariableMedSource.MedObj);
                result = data.Map<OperResult<object>>();
                if (method.HasReturn && result.IsSuccess)
                {
                    var content = deviceVariableMedSource.Converter.ConvertTo(result.Content?.ToString()?.Replace($"\0", ""));
                    var operResult = deviceVariableMedSource.DeviceVariable.SetValue(content);
                    if (!operResult.IsSuccess)
                    {
                        _logger?.LogWarning(operResult.Message, ToString());
                    }
                }
                else
                {
                    var operResult = deviceVariableMedSource.DeviceVariable.SetValue(null);
                    if (!operResult.IsSuccess)
                    {
                        _logger?.LogWarning(operResult.Message, ToString());
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Error;
                result.Message = $"{deviceVariableMedSource.DeviceVariable.Name}执行{deviceVariableMedSource.DeviceVariable.OtherMethod} 方法失败:{ex.Message}";
                return result;
            }
        }

    }

    /// <summary>
    /// 执行变量写入
    /// </summary>
    /// <returns></returns>
    internal async Task<OperResult> InVokeWriteAsync(DeviceVariableRunTime deviceVariable, string value, CancellationToken cancellationToken)
    {
        try
        {
            await easyLock.LockAsync();
            if (IsShareChannel) _driver.InitDataAdapter();
            if (!deviceVariable.WriteExpressions.IsNullOrEmpty() && !value.IsNullOrEmpty())
            {
                object rawdata = value.GetObjectData();
                object data;
                try
                {
                    data = deviceVariable.WriteExpressions.GetExpressionsResult(rawdata);
                    var result = await _driver.WriteValueAsync(deviceVariable, data.ToString(), cancellationToken);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(deviceVariable.Name + " 转换写入表达式失败", ex);
                    return new OperResult(deviceVariable.Name + " 转换写入表达式失败：" + ex.Message);
                }
            }
            else
            {
                var result = await _driver.WriteValueAsync(deviceVariable, value, cancellationToken);
                return result;
            }
        }
        catch (Exception ex)
        {
            return (new OperResult(ex));
        }
        finally
        {
            easyLock.UnLock();
        }
    }

    /// <summary>
    /// 执行轮询特殊方法,并设置变量值
    /// </summary>
    private async Task<OperResult<object>> InvokeMedAsync(DeviceVariableMedRead deviceVariableMedRead)
    {
        OperResult<object> result = new();
        var method = deviceVariableMedRead.MedInfo;
        if (method == null)
        {
            result.ResultCode = ResultCode.Error;
            result.Message = $"{deviceVariableMedRead.DeviceVariable.Name}找不到执行方法{deviceVariableMedRead.DeviceVariable.OtherMethod}";
            return result;
        }
        else
        {
            try
            {
                var data = await method.InvokeObjectAsync(_driver, deviceVariableMedRead.MedObj);
                result = data.Map<OperResult<object>>();
                if (method.HasReturn && result.IsSuccess)
                {
                    var content = deviceVariableMedRead.Converter.ConvertTo(result.Content?.ToString()?.Replace($"\0", ""));
                    var operResult = deviceVariableMedRead.DeviceVariable.SetValue(content);
                    if (!operResult.IsSuccess)
                    {
                        _logger?.LogWarning(operResult.Message, ToString());
                    }
                }
                else
                {
                    var operResult = deviceVariableMedRead.DeviceVariable.SetValue(null);
                    if (!operResult.IsSuccess)
                    {
                        _logger?.LogWarning(operResult.Message, ToString());
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.Error;
                result.Message = $"{deviceVariableMedRead.DeviceVariable.Name}执行{deviceVariableMedRead.DeviceVariable.OtherMethod} 方法失败:{ex.Message}";
                return result;
            }
        }

    }
    #endregion

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        IsExited = true;
        base.Dispose(disposing);
        StopThread();
        _driver = null;
        Methods = null;
        Propertys = null;
        if (Device != null)
        {
            _pluginService.DeleteDriver(DeviceId, Device.PluginId);
        }

    }


}

