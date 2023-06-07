#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.FriendlyException;
using Furion.Logging.Extensions;

using Microsoft.Extensions.Logging;


using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using ThingsGateway.Foundation;

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

    /// <summary>
    /// 分包变量
    /// </summary>
    private List<DeviceVariableSourceRead> DeviceVariableSourceReads = new();

    private bool isInitSuccess = true;

    /// <inheritdoc cref="CollectDeviceCore"/>
    public CollectDeviceCore(IServiceScopeFactory scopeFactory)
    {

        _scopeFactory = scopeFactory;
        using var scope = scopeFactory.CreateScope();

        _pluginService = scope.ServiceProvider.GetService<PluginSingletonService>();
        _globalCollectDeviceData = scope.ServiceProvider.GetService<GlobalCollectDeviceData>();
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

    /// <inheritdoc cref="GlobalCollectDeviceData"/>
    private GlobalCollectDeviceData _globalCollectDeviceData { get; set; }
    #region 设备子线程采集启动停止
    /// <summary>
    /// 暂停采集
    /// </summary>
    public void PasueThread(bool keepOn)
    {
        lock (this)
        {
            var str = keepOn == false ? "设备线程采集暂停" : "设备线程采集继续";
            _logger?.LogInformation($"{str}:{_device.Name}");
            this.Device.KeepOn = keepOn;
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
    /// 是否多个设备共享链路;
    /// </summary>
    public bool IsShareChannel;
    /// <summary>
    /// 已经停止
    /// </summary>
    public bool IsExited;

    /// <summary>
    /// 开始前
    /// </summary>
    /// <returns></returns>
    public async Task<bool> BeforeActionAsync(CancellationToken cancellationToken, object client = null)
    {
        try
        {
            IsExited = false;
            _logger?.LogInformation($"{_device.Name}采集设备线程开始");

            StoppingTokens.Add(new());

            if (_driver != null)
            {
                InitDriver(client);
                Device.SourceVariableNum = DeviceVariableSourceReads.Count;
                Device.MethodVariableNum = DeviceVariableMedReads.Count;
            }
            else
            {
                Device.DeviceStatus = DeviceStatusEnum.OffLine;
                Device.DeviceOffMsg = "获取插件失败";
                return false;
            }
            try
            {
                if (Device?.KeepOn == true)
                {
                    //驱动插件执行循环前方法
                    Device.ActiveTime = DateTime.UtcNow;
                    await _driver?.BeforStartAsync(cancellationToken);
                }

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, _device.Name + "BeforStart错误");
                Device.DeviceStatus = DeviceStatusEnum.OffLine;
                Device.DeviceOffMsg = "开始前发生错误，通常为打开端口失败";
            }
            isInitSuccess = true;
            return isInitSuccess;

        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, _device.Name + "初始化失败");
            Device.DeviceStatus = DeviceStatusEnum.OffLine;
            Device.DeviceOffMsg = "初始化失败";
        }
        isInitSuccess = false;
        return isInitSuccess;
    }
    /// <summary>
    /// 结束后
    /// </summary>
    public async Task FinishActionAsync()
    {
        Device.DeviceVariableRunTimes.ForEach(a =>
        {
            a.VariablePropertys.Clear();
            a.VariablePropertys = null;
        });
        Device.DeviceVariableRunTimes.Clear();
        _globalCollectDeviceData.CollectDevices.RemoveWhere(it => it.Id == Device.Id);
        try
        {
            _logger?.LogInformation($"{_device.Name}采集线程停止中");
            await _driver?.AfterStopAsync();
            _driver?.SafeDispose();
            _pluginService.DeleteDriver(DeviceId, Device.PluginId);
            _logger?.LogInformation($"{_device.Name}采集线程已停止");
            IsExited = true;

        }
        catch (Exception ex)
        {
            _logger.LogError($"{Device.Name} 释放失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 初始化，在设备子线程创建或更新时才会执行
    /// </summary>
    public void Init(CollectDeviceRunTime device)
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
            using var scope = _scopeFactory.CreateScope();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger("采集设备:" + _device.Name);
            //更新插件信息
            CreatDriver();
            //全局数据更新
            if (isUpDevice)
            {
                _globalCollectDeviceData.CollectDevices.RemoveWhere(it => it.Id == device.Id);
                _globalCollectDeviceData.CollectDevices.Add(device);
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
    public async Task<ThreadRunReturn> RunActionAsync(CancellationToken cancellationToken)
    {
        try
        {

            using CancellationTokenSource StoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, StoppingTokens.LastOrDefault().Token);
            if (_driver == null) return ThreadRunReturn.Continue;

            if (Device?.KeepOn == false)
            {
                Device.DeviceStatus = DeviceStatusEnum.Pause;
                return ThreadRunReturn.Continue; ;
            }
            if (Device.DeviceStatus != DeviceStatusEnum.OnLineButNoInitialValue && Device.DeviceStatus != DeviceStatusEnum.OnLine && Device.DeviceStatus != DeviceStatusEnum.OffLine)
                Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
            if (DeviceVariableSourceReads.Count == 0 && Device.DeviceVariableRunTimes.Where(a => a.OtherMethod.IsNullOrEmpty()).Count() > 0)
            {
                Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                Device.DeviceOffMsg = "分包失败，请检查变量地址是否符合规则";
            }
            int deviceMedsVariableSuccessNum = 0;
            int deviceMedsVariableFailedNum = 0;
            int deviceSourceVariableSuccessNum = 0;
            int deviceSourceVariableFailedNum = 0;
            if (StoppingToken.Token.IsCancellationRequested)
                return ThreadRunReturn.Break;

            Device.ActiveTime = DateTime.UtcNow;

            if (_driver.IsSupportRequest())
            {
                foreach (var deviceVariableSourceRead in DeviceVariableSourceReads)
                {
                    if (Device?.KeepOn == false)
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
                        }
                    }


                }

                foreach (var deviceVariableMedRead in DeviceVariableMedReads)
                {
                    if (Device?.KeepOn == false)
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
                    Device.DeviceStatus = DeviceStatusEnum.OnLine;

                }
                else if (deviceMedsVariableFailedNum != 0 || deviceSourceVariableFailedNum != 0)
                {
                    var oper = _driver.IsConnected();

                    if (oper.IsSuccess)
                    {
                        Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                    }
                    else
                    {
                        Device.DeviceStatus = DeviceStatusEnum.OffLine;
                        Device.DeviceOffMsg = oper.Message;

                    }
                }
                else
                {

                    if (DeviceVariableSourceReads.Count == 0 && DeviceVariableMedReads.Count == 0)
                    {
                        if (Device.DeviceVariablesNum > 0)
                            Device.DeviceOffMsg = "分包失败，请检查变量地址是否符合规则";
                        else
                            Device.DeviceOffMsg = "无采集变量";
                    }
                }
            }
            else
            {

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
            return ThreadRunReturn.None;
        }
    }

    #endregion

    #region 分包

    /// <summary>
    /// 传入设备变量列表，执行后赋值<see cref="DeviceVariableSourceReads"/>
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    private void LoadSourceReads(List<CollectVariableRunTime> collectVariableRunTimes)
    {
        if (collectVariableRunTimes == null || _driver == null) { return; }
        try
        {
            var tag = collectVariableRunTimes.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.WriteOnly &&
            it.OtherMethod.IsNullOrEmpty() && !it.VariableAddress.IsNullOrEmpty()).ToList();
            var result = _driver.LoadSourceRead(tag);
            if (result.IsSuccess)
                DeviceVariableSourceReads = result.Content;
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
    }

    #endregion

    #region 写入与方法
    private EasyLock easyLock = new();

    /// <summary>
    /// 执行特殊方法
    /// </summary>
    /// <param name="coreMethod"></param>
    /// <param name="par"></param>
    /// <returns></returns>
    public async Task<OperResult> InvokeMedAsync(Method coreMethod, params object[] par)
    {
        try
        {
            await easyLock.LockAsync();
            if (IsShareChannel) _driver.InitDataAdapter();
            return (OperResult)await coreMethod.InvokeObjectAsync(_driver, par);
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
    /// 执行变量写入
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult> InVokeWriteAsync(CollectVariableRunTime deviceVariable, string value, CancellationToken cancellationToken)
    {
        try
        {
            await easyLock.LockAsync();
            if (IsShareChannel) _driver.InitDataAdapter();
            if (!deviceVariable.WriteExpressions.IsNullOrEmpty() && value != null)
            {
                object data = null;
                try
                {
                    Regex regex = new Regex("^[-+]?[0-9]*\\.?[0-9]+$");
                    bool match = regex.IsMatch(value);

                    //bool match = NewLife.StringHelper.IsMatch(value, @"^[-+]?[0-9]*\.?[0-9]+$");
                    if (match)
                    {
                        if (value.ToDouble() == 0 && Convert.ToInt32(value) != 0)
                        {
                            return (new OperResult(deviceVariable.Name + " 转换写入表达式失败"));
                        }
                        data = deviceVariable.WriteExpressions.GetExpressionsResult(value.ToDouble());
                    }
                    else
                    {
                        data = deviceVariable.WriteExpressions.GetExpressionsResult(value);
                    }
                    var result = await _driver.WriteValueAsync(deviceVariable, data.ToString(), cancellationToken);
                    return result;
                }
                catch (Exception ex)
                {

                    (deviceVariable.Name + " 转换写入表达式失败：" + ex.Message).LogError();
                    return (new OperResult(deviceVariable.Name + " 转换写入表达式失败：" + ex.Message));
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
    /// 执行特殊方法，方法参数以";"分割
    /// </summary>
    /// <param name="deviceVariableMedRead"></param>
    /// <returns></returns>
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
            if (deviceVariableMedRead.MedObj == null)
            {
                var ps = method.Info.GetParameters();
                deviceVariableMedRead.MedObj = new object[ps.Length];

                if (!deviceVariableMedRead.MedStr.IsNullOrEmpty())
                {
                    string[] strs = deviceVariableMedRead.MedStr?.Split(';');
                    int index = 0;
                    for (int i = 0; i < ps.Length; i++)
                    {
                        if (strs.Length <= i)
                        {
                            result.ResultCode = ResultCode.Error;
                            result.Message = $"{deviceVariableMedRead.DeviceVariable.Name} 执行方法 {deviceVariableMedRead.DeviceVariable.OtherMethod} 参数不足{deviceVariableMedRead.MedStr}";
                            return result;
                        }
                        deviceVariableMedRead.MedObj[i] = deviceVariableMedRead.Converter.ConvertFrom(strs[index], ps[i].ParameterType);
                        index++;
                    }
                }
            }
            try
            {
                var data = await method.InvokeObjectAsync(_driver, deviceVariableMedRead.MedObj);
                result = data.Map<OperResult<object>>();
                if (method.HasReturn && result.IsSuccess)
                {
                    var content = deviceVariableMedRead.Converter.ConvertTo(result.Content?.ToString()?.Replace($"\0", ""));
                    deviceVariableMedRead.DeviceVariable.SetValue(content);
                }
                else
                {
                    deviceVariableMedRead.DeviceVariable.SetValue(null);
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
        base.Dispose(disposing);
        StopThread();
        _device.DeviceVariableRunTimes = null;
        _device = null;
        GC.Collect();
    }


}

