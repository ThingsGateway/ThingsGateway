using Furion.FriendlyException;
using Furion.Logging.Extensions;

using Microsoft.Extensions.Logging;

using System.Linq;
using System.Reflection;
using System.Threading;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

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

    private ILogger _logger;

    /// <summary>
    /// 当前设备信息
    /// </summary>
    private CollectDeviceRunTime _device;

    /// <summary>
    /// 当前的驱动插件实例
    /// </summary>
    internal DriverBase _driver;

    /// <summary>
    /// 全局插件服务
    /// </summary>
    private PluginCore _pluginService;

    /// <summary>
    /// 分包变量
    /// </summary>
    private List<DeviceVariableSourceRead> DeviceVariableSourceReads = new();
    private IServiceScopeFactory _scopeFactory;

    public CollectDeviceCore(IServiceScopeFactory scopeFactory)
    {

        _scopeFactory = scopeFactory;
        using var scope = scopeFactory.CreateScope();

        _pluginService = scope.ServiceProvider.GetService<PluginCore>();
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
    /// 当前设备全部特殊方法，执行初始化后获取正确值
    /// </summary>
    public List<MethodInfo> Methods { get; private set; }

    /// <summary>
    /// 当前设备全部设备属性，执行初始化后获取正确值
    /// </summary>
    public List<DependencyProperty> Propertys { get; private set; }
    GlobalCollectDeviceData _globalCollectDeviceData { get; set; }
    IDriverPluginService _driverPluginService { get; set; }
    /// <summary>
    /// 初始化，在设备子线程创建或更新时才会执行
    /// </summary>
    public void Init(CollectDeviceRunTime device, object client = null)
    {
        if (device == null) return;
        try
        {
            _device = device;
            using var scope = _scopeFactory.CreateScope();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger("采集设备:" + _device.Name);
            UpDriver();
            SetPluginProperties(_device.DevicePropertys);
            _driver.IsLogOut = _device.IsLogOut;
            _driver.Init(_logger, _device, client);
            LoadSourceReads(_device.DeviceVariableRunTimes);
            StoppingTokens.Add(new());
            Init();
            //重新初始化设备属性
            _globalCollectDeviceData.CollectDevices.RemoveWhere(it => it.Id == device.Id);
            _globalCollectDeviceData.CollectDevices.Add(device);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, device.Name);
        }


        void UpDriver()
        {
            var driverPlugin = _driverPluginService.GetDriverPluginById(Device.PluginId);
            _driver = (DriverBase)_pluginService.AddDriver(DeviceId, driverPlugin);
            if (driverPlugin != null)
            {
                Methods = _pluginService.GetMethod(_driver);
                Propertys = _pluginService.GetDriverProperties(_driver);
            }
            else
            {
                throw Oops.Oh($"找不到驱动{driverPlugin.AssembleName}");
            }
        }
    }

    #region 设备子线程采集启动停止

    private Task<Task> DeviceTask;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        DeviceTask = new Task<Task>(() =>
        {
            CancellationTokenSource StoppingToken = StoppingTokens.Last();
            return Task.Factory.StartNew(async (a) =>
            {
                _logger?.LogInformation($"{_device.Name}采集设备线程开始");
                try
                {
                    if (Device?.Enable == true)
                    {
                        //驱动插件执行循环前方法
                        _driver?.BeforStart();
                    }

                    if (!StoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, _device.Name + "初始化出错");
                    Device.DeviceStatus = DeviceStatusEnum.OffLine;
                    Device.DeviceOffMsg = "初始化出错";
                }
                Device.SourceVariableNum = DeviceVariableSourceReads.Count;
                Device.MethodVariableNum = DeviceVariableMedReads.Count;
                while (!StoppingToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(100, StoppingToken.Token);

                        if (_driver == null) continue;

                        if (Device?.Enable == false)
                        {
                            Device.DeviceStatus = DeviceStatusEnum.OffLine;
                            Device.DeviceOffMsg = "暂停";
                            continue;
                        }
                        try
                        {
                            if (Device.DeviceStatus != DeviceStatusEnum.OnLineButNoInitialValue && Device.DeviceStatus != DeviceStatusEnum.OnLine)
                                Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                            Device.ActiveTime = DateTime.Now;

                            int deviceMedsVariableSuccessNum = 0;
                            int deviceMedsVariableFailedNum = 0;
                            int deviceSourceVariableSuccessNum = 0;
                            int deviceSourceVariableFailedNum = 0;
                            if (StoppingToken.Token.IsCancellationRequested)
                                break;
                            if (_driver.IsSupportAddressRequest())
                            {
                                foreach (var deviceVariableSourceRead in DeviceVariableSourceReads)
                                {
                                    if (Device?.Enable == false)
                                    {
                                        continue;
                                    }

                                    if (StoppingToken.Token.IsCancellationRequested)
                                        break;

                                    //连读变量
                                    if (deviceVariableSourceRead.CheckIfRequestAndUpdateTime(DateTime.Now))
                                    {
                                        var read = await _driver.ReadSourceAsync(deviceVariableSourceRead, StoppingToken.Token);
                                        if (read != null && read.IsSuccess)
                                        {
                                            _logger?.LogTrace(_device.Name + "采集[" + deviceVariableSourceRead.Address + " -" + deviceVariableSourceRead.Length + "] 数据成功" + read.Content?.ToHexString(" "));
                                            deviceSourceVariableSuccessNum += 1;
                                        }
                                        else if (read != null && read.IsSuccess == false)
                                        {
                                            _logger?.LogWarning(_device.Name + "采集[" + deviceVariableSourceRead.Address + " -" + deviceVariableSourceRead.Length + "] 数据失败 - " + read?.Message);
                                            deviceSourceVariableFailedNum += 1;
                                        }
                                    }

                                    await Task.Delay(20);

                                }

                                foreach (var deviceVariableMedRead in DeviceVariableMedReads)
                                {
                                    if (Device?.Enable == false)
                                        continue;
                                    if (StoppingToken.IsCancellationRequested)
                                        break;

                                    //连读变量
                                    if (deviceVariableMedRead.CheckIfRequestAndUpdateTime(DateTime.Now))
                                    {
                                        var read = await InvokeMed(deviceVariableMedRead);
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

                                    await Task.Delay(20);

                                }

                                if (deviceMedsVariableFailedNum == 0 && deviceSourceVariableFailedNum == 0)
                                {
                                    Device.DeviceStatus = DeviceStatusEnum.OnLine;
                                }
                                else
                                {
                                    Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                                }
                            }
                        }
                        catch (TaskCanceledException)
                        {

                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, $"采集线程循环异常{_device.Name}");
                        }
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"采集线程循环异常{_device.Name}");
                    }
                }

            }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);
        }
         );
    }

    /// <summary>
    /// 暂停采集
    /// </summary>
    public void PasueThread(bool enable)
    {
        lock (this)
        {
            var str = enable == false ? "设备线程采集暂停" : "设备线程采集继续";
            _logger?.LogInformation($"{str}:{_device.Name}");
            this.Device.Enable = enable;
        }
    }

    /// <summary>
    /// 开始采集
    /// </summary>
    public void StartThread()
    {
        DeviceTask?.Start();
    }
    public void StopThread()
    {
        try
        {
            CancellationTokenSource StoppingToken = StoppingTokens.LastOrDefault();
            StoppingToken?.Cancel();
            _logger?.LogInformation($"{_device.Name}采集线程停止中");
            var devResult = DeviceTask?.Result;
            if (devResult?.Status != TaskStatus.Canceled)
            {
                if (devResult?.Wait(5000) == true)
                {
                    _logger?.LogInformation($"{_device.Name}采集线程已停止");
                }
                else
                {
                    _logger?.LogInformation($"{_device.Name}采集线程停止超时，已强制取消");
                }
            }
            DeviceTask?.Dispose();
            if (StoppingToken != null)
            {
                StoppingTokens.Remove(StoppingToken);
            }
            _globalCollectDeviceData.CollectDevices.RemoveWhere(it => it.Id == Device.Id);

            try
            {
                _driver?.AfterStop();
                _driver?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{Device.Name} Dispose Error: {ex.Message}");
            }

        }
        finally
        {
            _pluginService.DeleteDriver(DeviceId, Device.PluginId);
        }
    }
    #endregion

    #region 驱动信息获取

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


    /// <summary>
    /// 执行特殊方法，方法参数以";"分割
    /// </summary>
    /// <param name="deviceVariableMedRead"></param>
    /// <returns></returns>
    public async Task<OperResult<object>> InvokeMed(DeviceVariableMedRead deviceVariableMedRead)
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
    public async Task<OperResult> InvokeMed(Method coreMethod, params object[] par)
    {
        return (OperResult)await coreMethod.InvokeObjectAsync(_driver, par);
    }

    /// <summary>
    /// 执行变量写入
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult> InVokeWriteAsync(CollectVariableRunTime deviceVariable, string value)
    {
        try
        {
            if (!deviceVariable.WriteExpressions.IsNullOrEmpty() && value != null)
            {
                object data = null;
                try
                {
                    data = deviceVariable.WriteExpressions.GetExpressionsResult(Convert.ChangeType(value, deviceVariable.DataType));
                }
                catch (Exception ex)
                {
                    (deviceVariable.Name + " 转换写入表达式失败：" + ex.Message).LogError();
                }
                var result = await _driver.WriteValueAsync(deviceVariable, data.ToString());
                return result;
            }
            else
            {
                var result = await _driver.WriteValueAsync(deviceVariable, value);
                return result;
            }
        }
        catch (Exception ex)
        {
            return (new OperResult(ex));
        }
    }

    /// <summary>
    /// 设置驱动插件的属性值
    /// </summary>
    public void SetPluginProperties(List<DependencyProperty> deviceProperties)
    {
        if (deviceProperties == null) return;
        var pluginPropertys = _driver.GetType().GetProperties();
        foreach (var propertyInfo in pluginPropertys)
        {
            var propAtt = propertyInfo.GetCustomAttribute(typeof(DevicePropertyAttribute));
            var deviceProperty = deviceProperties.FirstOrDefault(x => x.PropertyName == propertyInfo.Name);
            if (propAtt == null || deviceProperty == null || string.IsNullOrEmpty(deviceProperty?.Value?.ToString())) continue;
            var value = ReadWriteHelpers.ObjToTypeValue(propertyInfo, deviceProperty.Value);
            propertyInfo.SetValue(_driver, value);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        StopThread();
    }




}

