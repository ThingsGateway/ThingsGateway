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

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System.Reflection;

using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Byte;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Application;

/// <summary>
/// 设备子线程服务
/// </summary>
public class CollectDeviceCore
{
    /// <summary>
    /// 特殊方法变量
    /// </summary>
    public List<DeviceVariableMethodSource> DeviceVariableMethodReads = new();
    /// <summary>
    /// 特殊方法变量,不参与轮询执行
    /// </summary>
    public List<DeviceVariableMethodSource> DeviceVariableMethodSources = new();
    /// <summary>
    /// 变量打包
    /// </summary>
    public List<DeviceVariableSourceRead> DeviceVariableSourceReads = new();
    /// <summary>
    /// 全局插件服务
    /// </summary>
    private readonly PluginSingletonService _pluginService;
    /// <summary>
    /// 读写锁
    /// </summary>
    private readonly EasyLock easyLock = new();
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
    /// 是否初始化成功
    /// </summary>
    private bool isInitSuccess = true;

    /// <inheritdoc cref="CollectDeviceCore"/>
    public CollectDeviceCore()
    {
        _pluginService = ServiceHelper.Services.GetService<PluginSingletonService>();
        GlobalDeviceData = ServiceHelper.Services.GetService<GlobalDeviceData>();
        DriverPluginService = ServiceHelper.Services.GetService<IDriverPluginService>();
    }

    /// <summary>
    /// 当前设备
    /// </summary>
    public CollectDeviceRunTime Device => _device;

    /// <summary>
    /// 当前设备Id
    /// </summary>
    public long DeviceId => (long)(_device?.Id.ToLong());

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

    private IDriverPluginService DriverPluginService { get; set; }

    private GlobalDeviceData GlobalDeviceData { get; set; }

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

    #region 插件处理

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <returns></returns>
    private CollectBase CreatDriver()
    {

        var driverPlugin = DriverPluginService.GetDriverPluginById(_device.PluginId);
        if (driverPlugin != null)
        {
            try
            {
                _driver = (CollectBase)_pluginService.GetDriver(driverPlugin);
                if (_driver == null)
                {
                    throw Oops.Oh($"创建插件失败");
                }
                Methods = _pluginService.GetMethod(_driver);
                Propertys = _pluginService.GetDriverProperties(_driver);
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"创建插件失败：{ex.Message}");
            }

        }
        else
        {
            throw Oops.Oh($"找不到驱动{driverPlugin?.AssembleName}");
        }
        //设置插件配置项
        SetPluginProperties(_device.DevicePropertys);
        return _driver;

    }

    private void InitDriver(object client)
    {
        //初始化插件
        _driver.Init(_logger, _device, client);
        //变量打包
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
    /// 是否多个设备共享链路，由外部传入
    /// </summary>
    public bool IsShareChannel;

    /// <summary>
    /// 线程开始时执行
    /// </summary>
    /// <returns></returns>
    internal async Task BeforeActionAsync(CancellationToken token, object client = null)
    {
        try
        {
            if (_device == null)
            {
                _logger?.LogError(nameof(CollectDeviceRunTime) + "设备不能为null");
                isInitSuccess = false;
                return;
            }
            if (_driver == null)
            {
                _logger?.LogWarning(_device.Name + " - 插件不能为null");
                isInitSuccess = false;
                return;
            }

            _logger?.LogInformation($"{_device.Name}采集设备线程开始");

            InitDriver(client);
            Device.SourceVariableCount = DeviceVariableSourceReads.Count;
            Device.MethodVariableCount = DeviceVariableMethodReads.Count;

            try
            {
                if (Device.KeepRun == true)
                {
                    await _driver.BeforStartAsync(token);
                    Device.SetDeviceStatus(SysDateTimeExtensions.CurrentDateTime);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, _device.Name);
                Device.SetDeviceStatus(null, Device.ErrorCount + 1, ex.Message);
            }
            isInitSuccess = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, _device.Name);
            isInitSuccess = false;
            Device.SetDeviceStatus(SysDateTimeExtensions.CurrentDateTime, 999, ex.Message);
        }
    }

    /// <summary>
    /// 结束后
    /// </summary>
    internal async Task FinishActionAsync()
    {
        try
        {
            _logger?.LogInformation($"{_device.Name}采集线程停止中");
            await _driver?.AfterStopAsync();

            _driver?.SafeDispose();
            _logger?.LogInformation($"{_device.Name}采集线程已停止");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"{Device.Name} 释放失败");
        }
        finally
        {
            isInitSuccess = false;
            GlobalDeviceData.CollectDevices.RemoveWhere(it => it.Id == Device.Id);
            easyLock.SafeDispose();
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    internal bool Init(CollectDeviceRunTime device)
    {
        if (device == null)
        {
            _logger?.LogError(nameof(CollectDeviceRunTime) + "设备不能为null");
            return false;
        }
        try
        {
            bool isUpDevice = Device != device;
            _device = device;
            _logger = ServiceHelper.Services.GetService<ILoggerFactory>().CreateLogger("采集设备:" + _device.Name);
            //更新插件信息
            CreatDriver();
            //全局数据更新
            if (isUpDevice)
            {
                GlobalDeviceData.CollectDevices.RemoveWhere(it => it.Id == device.Id);
                GlobalDeviceData.CollectDevices.Add(device);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, device.Name);
            return false;
        }

    }

    /// <summary>
    /// 执行一次读取
    /// </summary>
    internal async Task<ThreadRunReturn> RunActionAsync(CancellationToken token)
    {
        try
        {
            if (_device == null)
            {
                _logger?.LogError(nameof(CollectDeviceRunTime) + "设备不能为null");
                return ThreadRunReturn.Continue;
            }
            if (_driver == null)
            {
                _logger?.LogWarning(_device.Name + " - 插件不能为null");
                return ThreadRunReturn.Continue;
            }

            if (Device.KeepRun == false)
            {
                //采集暂停
                return ThreadRunReturn.Continue;
            }

            if (token.IsCancellationRequested)
                return ThreadRunReturn.Break;

            int deviceMethodsVariableSuccessNum = 0;
            int deviceMethodsVariableFailedNum = 0;
            int deviceSourceVariableSuccessNum = 0;
            int deviceSourceVariableFailedNum = 0;

            //支持读取
            if (_driver.IsSupportRequest)
            {
                foreach (var deviceVariableSourceRead in DeviceVariableSourceReads)
                {
                    if (Device?.KeepRun == false)
                    {
                        continue;
                    }

                    if (token.IsCancellationRequested)
                        break;

                    //连读变量
                    if (deviceVariableSourceRead.CheckIfRequestAndUpdateTime(SysDateTimeExtensions.CurrentDateTime))
                    {
                        try
                        {
                            await easyLock.WaitAsync();
                            var read = await _driver.ReadSourceAsync(deviceVariableSourceRead, token);
                            if (read != null && read.IsSuccess)
                            {
                                _logger?.LogTrace(_device.Name + " - " + " - 采集[" + deviceVariableSourceRead.Address + " - " + deviceVariableSourceRead.Length + "] 数据成功" + read.Content?.ToHexString(" "));
                                deviceVariableSourceRead.LastSuccess = true;
                                deviceSourceVariableSuccessNum += 1;
                            }
                            else
                            {
                                _logger?.LogWarning(_device.Name + " - " + " - 采集[" + deviceVariableSourceRead.Address + " -" + deviceVariableSourceRead.Length + "] 数据失败 - " + read?.Message);
                                deviceVariableSourceRead.LastSuccess = false;
                                deviceSourceVariableFailedNum += 1;
                                Device.SetDeviceStatus(null, Device.ErrorCount + deviceSourceVariableFailedNum, read?.Message);
                            }
                        }
                        finally
                        {
                            easyLock.Release();
                        }
                    }
                }

                foreach (var deviceVariableMethodRead in DeviceVariableMethodReads)
                {
                    if (Device?.KeepRun == false)
                        continue;
                    if (token.IsCancellationRequested)
                        break;

                    //连读变量
                    if (deviceVariableMethodRead.CheckIfRequestAndUpdateTime(SysDateTimeExtensions.CurrentDateTime))
                    {

                        var read = await InvokeMethodAsync(deviceVariableMethodRead, token);
                        if (read.IsSuccess)
                        {
                            _logger?.LogTrace(_device.Name + "执行方法[" + deviceVariableMethodRead.MethodInfo.Name + "] 成功" + read.Content.ToJson());
                            deviceMethodsVariableSuccessNum += 1;
                        }
                        else
                        {
                            _logger?.LogWarning(_device.Name + "执行方法[" + deviceVariableMethodRead.MethodInfo.Name + "] 失败" + read?.Message);
                            deviceMethodsVariableFailedNum += 1;
                            Device.SetDeviceStatus(null, Device.ErrorCount + deviceSourceVariableFailedNum, read.Message);
                        }

                    }

                }


                if (deviceMethodsVariableFailedNum == 0 && deviceSourceVariableFailedNum == 0 && (deviceMethodsVariableSuccessNum != 0 || deviceSourceVariableSuccessNum != 0))
                {
                    //只有成功读取一次，失败次数都会清零
                    Device.SetDeviceStatus(SysDateTimeExtensions.CurrentDateTime, 0);
                }
                else
                {
                    if (deviceMethodsVariableFailedNum == 0 && deviceSourceVariableFailedNum == 0 && deviceMethodsVariableSuccessNum == 0 && deviceSourceVariableSuccessNum == 0)
                    {
                        //这次没有执行读取
                        //判断是否已连接
                        if (_driver.IsConnected())
                        {
                            //更新设备活动时间
                            Device.SetDeviceStatus(SysDateTimeExtensions.CurrentDateTime);
                        }
                    }
                }
            }
            else //插件自更新设备状态与变量状态
            {
                //获取设备连接状态
                if (_driver.IsConnected())
                {
                    //更新设备活动时间
                    Device.SetDeviceStatus(SysDateTimeExtensions.CurrentDateTime, 0);
                }
                else
                {
                    Device.SetDeviceStatus(SysDateTimeExtensions.CurrentDateTime, 999);
                }
            }

            if (token.IsCancellationRequested)
                return ThreadRunReturn.Break;

            //正常返回None
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
            Device.SetDeviceStatus(null, Device.ErrorCount + 1, ex.Message);
            return ThreadRunReturn.None;
        }
    }



    /// <summary>
    /// 获取设备变量打包列表/特殊方法列表
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    private void LoadSourceReads(List<DeviceVariableRunTime> collectVariableRunTimes)
    {
        if (_device == null)
        {
            _logger?.LogError(nameof(CollectDeviceRunTime) + "设备不能为null");
            return;
        }
        if (_driver == null)
        {
            _logger?.LogWarning(_device.Name + " - 插件不能为null");
            return;
        }

        try
        {
            //连读打包
            var tags = collectVariableRunTimes.Where(it =>
            it.ProtectTypeEnum != ProtectTypeEnum.WriteOnly &&
            string.IsNullOrEmpty(it.OtherMethod)
            && !string.IsNullOrEmpty(it.VariableAddress)).ToList();
            DeviceVariableSourceReads = _driver.LoadSourceRead(tags);
        }
        catch (Exception ex)
        {
            throw new($"连读打包失败，请查看变量地址是否正确", ex);
        }
        var variablesMethod = collectVariableRunTimes.Where(it => !string.IsNullOrEmpty(it.OtherMethod));
        {
            var tag = variablesMethod.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.WriteOnly);
            List<DeviceVariableMethodSource> variablesMethodResult1 = GetMethod(tag, true);
            DeviceVariableMethodReads = variablesMethodResult1;
        }

        {
            var tag = variablesMethod.Where(it => it.ProtectTypeEnum != ProtectTypeEnum.ReadOnly);
            List<DeviceVariableMethodSource> variablesMethodResult2 = GetMethod(tag, false);
            DeviceVariableMethodSources = variablesMethodResult2;
        }

        List<DeviceVariableMethodSource> GetMethod(IEnumerable<DeviceVariableRunTime> tag, bool isRead)
        {
            var variablesMethodResult = new List<DeviceVariableMethodSource>();
            foreach (var item in tag)
            {
                var methodResult = new DeviceVariableMethodSource(item.IntervalTime);
                var method = Methods.FirstOrDefault(it => it.Name == item.OtherMethod);
                if (method != null)
                {
                    methodResult.MethodInfo = new Method(method);
                    methodResult.MethodStr = item.VariableAddress;

                    if (isRead)
                    {
                        //获取实际执行的参数列表
                        var ps = methodResult.MethodInfo.Info.GetParameters();
                        methodResult.MethodObj = new object[ps.Length];

                        if (!string.IsNullOrEmpty(methodResult.MethodStr))
                        {
                            string[] strs = methodResult.MethodStr?.Trim()?.TrimEnd(';').Split(';');
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


    #endregion
    #region 写入与方法

    /// <summary>
    /// 执行特殊方法
    /// </summary>
    internal async Task<OperResult<string>> InvokeMethodAsync(DeviceVariableMethodSource deviceVariableMethodSource, bool isRead, string value, CancellationToken token)
    {
        try
        {
            await easyLock.WaitAsync();
            OperResult<string> result = new();
            var method = deviceVariableMethodSource.MethodInfo;
            if (method == null)
            {
                result.ResultCode = ResultCode.Error;
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
                        string[] strs1 = deviceVariableMethodSource.MethodStr?.Trim()?.TrimEnd(';').Split(';');
                        string[] strs2 = value?.Trim()?.TrimEnd(';').Split(';');
                        //通过分号分割，并且合并参数
                        var strs = strs1?.SpliceArray(strs2);
                        int index = 0;
                        for (int i = 0; i < ps.Length; i++)
                        {
                            if (typeof(CancellationToken).IsAssignableFrom(ps[i].ParameterType))
                            {
                                deviceVariableMethodSource.MethodObj[i] = token;
                            }
                            else
                            {
                                //得到对于的方法参数值
                                deviceVariableMethodSource.MethodObj[i] = deviceVariableMethodSource.Converter.ConvertFrom(strs[index], ps[i].ParameterType);
                                index++;
                                if (strs.Length <= i)
                                {
                                    result.ResultCode = ResultCode.Error;
                                    result.Message = $"{deviceVariableMethodSource.DeviceVariable.Name} 执行方法 {deviceVariableMethodSource.DeviceVariable.OtherMethod} 参数不足{deviceVariableMethodSource.MethodStr}";
                                    //参数数量不符
                                    return result;
                                }
                            }

                        }
                    }
                }
                else if (deviceVariableMethodSource.HasTokenObj)
                {
                    //获取执行参数
                    var ps = method.Info.GetParameters();
                    var newObjs = deviceVariableMethodSource.MethodObj.ToList();
                    if (!string.IsNullOrEmpty(deviceVariableMethodSource.MethodStr) || !string.IsNullOrEmpty(value))
                    {
                        for (int i = 0; i < ps.Length; i++)
                        {
                            if (typeof(CancellationToken).IsAssignableFrom(ps[i].ParameterType))
                            {
                                newObjs.Insert(i, token);
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
                            data = method.Invoke(_driver, deviceVariableMethodSource.MethodObj);
                            break;
                        case TaskReturnType.Task:
                            await method.InvokeAsync(_driver, deviceVariableMethodSource.MethodObj);
                            break;
                        case TaskReturnType.TaskObject:
                            //执行方法
                            data = await method.InvokeObjectAsync(_driver, deviceVariableMethodSource.MethodObj);
                            break;
                    }

                    result = data?.Adapt<OperResult<string>>();
                    if (method.HasReturn && result != null && result.IsSuccess)
                    {
                        var content = deviceVariableMethodSource.Converter.ConvertTo(result.Content?.ToString()?.Replace($"\0", ""));
                        if (isRead)
                        {
                            var operResult = deviceVariableMethodSource.DeviceVariable.SetValue(content);
                            if (!operResult.IsSuccess)
                            {
                                _logger?.LogWarning(operResult.Message, ToString());
                            }
                        }

                    }
                    else
                    {
                        if (isRead)
                        {
                            var operResult = deviceVariableMethodSource.DeviceVariable.SetValue(null);
                            if (!operResult.IsSuccess)
                            {
                                _logger?.LogWarning(operResult.Message, ToString());
                            }
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    result.ResultCode = ResultCode.Error;
                    result.Message = $"{deviceVariableMethodSource.DeviceVariable.Name}执行{deviceVariableMethodSource.DeviceVariable.OtherMethod} 方法失败:{ex.Message}";
                    return result;
                }


            }
        }
        catch (Exception ex)
        {
            return (new OperResult<string>(ex));
        }
        finally
        {
            easyLock.Release();
        }
    }

    /// <summary>
    /// 执行变量写入
    /// </summary>
    /// <returns></returns>
    internal async Task<OperResult> InVokeWriteAsync(DeviceVariableRunTime deviceVariable, JToken value, CancellationToken token)
    {
        try
        {
            await easyLock.WaitAsync();
            if (IsShareChannel) _driver.InitDataAdapter();

            if (!string.IsNullOrEmpty(deviceVariable.WriteExpressions))
            {
                var jToken = value;
                object rawdata;
                if (jToken is JValue jValue)
                {
                    rawdata = jValue.Value;
                }
                else
                {
                    rawdata = jToken.ToString();
                }
                object data;
                try
                {
                    data = deviceVariable.WriteExpressions.GetExpressionsResult(rawdata);
                    var result = await _driver.WriteValueAsync(deviceVariable, JToken.FromObject(data), token);
                    return result;
                }
                catch (Exception ex)
                {
                    return new OperResult(deviceVariable.Name + " 转换写入表达式失败：" + ex.Message);
                }
            }
            else
            {
                var result = await _driver.WriteValueAsync(deviceVariable, value, token);
                return result;
            }
        }
        catch (Exception ex)
        {
            return (new OperResult(ex));
        }
        finally
        {
            easyLock.Release();
        }
    }

    /// <summary>
    /// 执行轮询特殊方法,并设置变量值
    /// </summary>
    private async Task<OperResult<string>> InvokeMethodAsync(DeviceVariableMethodSource deviceVariableMethodRead, CancellationToken token)
    {
        var data = await InvokeMethodAsync(deviceVariableMethodRead, true, string.Empty, token);
        return data;
    }

    #endregion

}

