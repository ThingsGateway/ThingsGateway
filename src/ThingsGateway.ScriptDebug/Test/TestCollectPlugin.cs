//using System.Text.Json.Nodes;

//using ThingsGateway.Foundation;
//using ThingsGateway.Gateway.Application;
//namespace ThingsGateway.Server;

///// <summary>
///// 插件类，默认实现了<see cref="IDevice"/>接口，继承<see cref="CollectFoundationBase"/> 实现采集插件
///// </summary>
//public class TestCollectPlugin : CollectFoundationBase
//{
//    /// <summary>
//    /// 调试UI Type，如果不存在无需重写
//    /// </summary>
//    public override Type DriverDebugUIType => base.DriverDebugUIType;
//    /// <summary>
//    /// 插件属性UI Type，继承<see cref="IPropertyUIBase"/>如果不存在无需重写
//    /// </summary>
//    public override Type DriverPropertyUIType => base.DriverPropertyUIType;
//    /// <summary>
//    /// 插件UI Type，继承<see cref="IDriverUIBase"/>如果不存在无需重写
//    /// </summary>
//    public override Type DriverUIType => base.DriverUIType;
//    /// <summary>
//    /// 插件变量寄存器UI Type，继承<see cref="IAddressUIBase"/>如果不存在无需重写
//    /// </summary>
//    public override Type DriverVariableAddressUIType => base.DriverVariableAddressUIType;

//    /// <summary>
//    /// 插件配置项，继承<see cref="CollectPropertyBase"/> 返回类实例
//    /// </summary>
//    public override CollectPropertyBase CollectProperties => _property;
//    private TestCollectProperty? _property = new();

//    /// <summary>
//    /// 在插件初始化时调用，只会执行一次，参数为插件默认的链路通道类，如未实现可忽略l
//    /// </summary>
//    protected override Task InitChannelAsync(ChannelObject channelObject, CancellationToken cancellationToken)
//    {
//        //做一些初始化操作

//        return Task.CompletedTask;
//    }

//    /// <summary>
//    /// 变量打包操作，会在默认的AfterVariablesChangedAsync方法里执行，参数为设备变量列表，返回源读取变量列表
//    /// </summary>
//    protected override Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables)
//    {
//        //实现将设备变量打包成源读取变量
//        //比如如果需要实现MC中的字多读功能，需将多个变量地址打包成一个源读取地址和读取长度，根据一系列规则，添加解析标识，然后在返回的整个字节数组中解析出原来的变量地址代表的数据字节

//        //一般可操作 VariableRuntime 类中的 index, thingsgatewaybitconvter 等属性
//        //一般可操作 VariableSourceRead 类中的 address, length 等属性

//        return Task.FromResult(new List<VariableSourceRead>());
//    }

//    /// <summary>
//    /// 实现<see cref="IDevice"/>
//    /// </summary>
//    public override IDevice? FoundationDevice => base.FoundationDevice;

//    /// <summary>
//    /// 特殊方法，添加<see cref="DynamicMethodAttribute"/> 特性 ，返回IOperResult
//    /// 支持<see cref="CancellationToken"/> 参数，需放到最后
//    /// 默认解析方式为英文分号
//    /// 比如rpc参数为 test1;test2，解析query1="test1",query2="test2"
//    /// 也可以在变量地址中填入test1，rpc参数传入test2，解析query1="test1",query2="test2"
//    /// </summary>
//    [DynamicMethod("测试特殊方法")]
//    public IOperResult<string> TestMethod(string query1, string query2, CancellationToken cancellationToken)
//    {
//        return new OperResult<string>() { Content = "测试特殊方法" };
//    }
//}

///// <summary>
///// 插件类配置
///// </summary>
//public class TestCollectProperty : CollectFoundationPackPropertyBase
//{
//    /// <summary>
//    /// 添加<see cref="DynamicPropertyAttribute"/> 特性，如需多语言配置，可添加json资源，参考其他插件
//    /// </summary>
//    [DynamicProperty(Description = null, Remark = null)]
//    public string TestString { get; set; }

//}

///// <summary>
///// 插件类，完全自定义
///// </summary>
//public class TestCollectPlugin1 : CollectBase
//{
//    /// <summary>
//    /// 调试UI Type，如果不存在无需重写
//    /// </summary>
//    public override Type DriverDebugUIType => base.DriverDebugUIType;
//    /// <summary>
//    /// 插件属性UI Type，继承<see cref="IPropertyUIBase"/>如果不存在无需重写
//    /// </summary>
//    public override Type DriverPropertyUIType => base.DriverPropertyUIType;
//    /// <summary>
//    /// 插件UI Type，继承<see cref="IDriverUIBase"/>如果不存在无需重写
//    /// </summary>
//    public override Type DriverUIType => base.DriverUIType;
//    /// <summary>
//    /// 插件变量寄存器UI Type，继承<see cref="IAddressUIBase"/>如果不存在无需重写
//    /// </summary>
//    public override Type DriverVariableAddressUIType => base.DriverVariableAddressUIType;

//    /// <summary>
//    /// 插件配置项，继承<see cref="CollectPropertyBase"/> 返回类实例
//    /// </summary>
//    public override CollectPropertyBase CollectProperties => _property;
//    private TestCollectProperty1? _property = new();

//    /// <summary>
//    /// 在插件初始化时调用，只会执行一次，参数为插件默认的链路通道类，如未实现可忽略l
//    /// </summary>
//    protected override Task InitChannelAsync(ChannelObject channelObject, CancellationToken cancellationToken)
//    {
//        //做一些初始化操作

//        return Task.CompletedTask;
//    }

//    /// <summary>
//    /// 变量打包操作，会在默认的AfterVariablesChangedAsync方法里执行，参数为设备变量列表，返回源读取变量列表
//    /// </summary>
//    protected override Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables)
//    {
//        //实现将设备变量打包成源读取变量
//        //比如如果需要实现MC中的字多读功能，需将多个变量地址打包成一个源读取地址和读取长度，根据一系列规则，添加解析标识，然后在返回的整个字节数组中解析出原来的变量地址代表的数据字节

//        //一般可操作 VariableRuntime 类中的 index, thingsgatewaybitconvter 等属性
//        //一般可操作 VariableSourceRead 类中的 address, length 等属性

//        return Task.FromResult(new List<VariableSourceRead>());
//    }

//    /// <summary>
//    /// 如果不实现ReadSourceAsync方法，可以返回flase
//    /// </summary>
//    protected override bool VariableSourceReadsEnable => base.VariableSourceReadsEnable;

//    /// <summary>
//    /// 获取任务列表，默认会实现 TestOnline任务，SetDeviceStatus任务以及 VariableSourceRead等任务，VariableSourceRead任务启用条件为<see cref="VariableSourceReadsEnable"/> 为true。任务即是timer实现，可通过<see cref="ScheduledTaskHelper.GetTask(string, Func{object?, CancellationToken, Task}, object?, TouchSocket.Core.ILog, CancellationToken)"/> 方法实现定时任务
//    /// </summary>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
//    {
//        return base.ProtectedGetTasks(cancellationToken);
//    }

//    /// <summary>
//    /// 实现离线重连任务
//    /// </summary>
//    /// <param name="state"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    protected override ValueTask TestOnline(object? state, CancellationToken cancellationToken)
//    {
//        return base.TestOnline(state, cancellationToken);
//    }

//    /// <summary>
//    /// 返回是否成功连接设备
//    /// </summary>
//    /// <returns></returns>
//    public override bool IsConnected()
//    {
//        return true;
//    }

//    /// <summary>
//    /// 在变量发生组态变化后执行，默认会执行<see cref="ProtectedLoadSourceReadAsync"/> 方法，重新获取源读取变量列表，并且重新启动VariableSourceRead等任务
//    /// </summary>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    public override Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
//    {
//        return base.AfterVariablesChangedAsync(cancellationToken);
//    }

//    /// <summary>
//    /// 变量寄存器的字符串描述
//    /// </summary>
//    /// <returns></returns>
//    public override string GetAddressDescription()
//    {
//        return base.GetAddressDescription();
//    }

//    /// <summary>
//    /// 设备暂停时执行，默认会暂停所有任务
//    /// </summary>
//    /// <param name="pause"></param>
//    public override void PauseThread(bool pause)
//    {
//        base.PauseThread(pause);
//    }

//    /// <summary>
//    /// 开始前执行
//    /// </summary>
//    protected override Task ProtectedStartAsync(CancellationToken cancellationToken)
//    {
//        //一般实现PLC长连接
//        return base.ProtectedStartAsync(cancellationToken);
//    }

//    /// <summary>
//    /// 写入变量，实现设备写入操作，注意执行写锁，        using var writeLock =await ReadWriteLock.WriterLockAsync(cancellationToken).ConfigureAwait(false);
//    /// </summary>
//    protected override ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
//    {
//        return base.WriteValuesAsync(writeInfoLists, cancellationToken);
//    }

//    /// <summary>
//    /// 读取源变量，在<see cref="VariableSourceReadsEnable"/> 为true时，添加源读取任务，任务启动时会执行
//    /// 一般需要更新设备变量值，调用<see cref="VariableRuntime.SetValue(object?, DateTime, bool)"/>
//    /// </summary>
//    protected override ValueTask<OperResult<ReadOnlyMemory<byte>>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
//    {
//        return base.ReadSourceAsync(variableSourceRead, cancellationToken);
//    }

//    protected override Task DisposeAsync(bool disposing)
//    {
//        return base.DisposeAsync(disposing);
//    }

//    /// <summary>
//    /// 特殊方法，添加<see cref="DynamicMethodAttribute"/> 特性 ，返回IOperResult
//    /// 支持<see cref="CancellationToken"/> 参数，需放到最后
//    /// 默认解析方式为英文分号
//    /// 比如rpc参数为 test1;test2，解析query1="test1",query2="test2"
//    /// 也可以在变量地址中填入test1，rpc参数传入test2，解析query1="test1",query2="test2"
//    /// </summary>
//    [DynamicMethod("测试特殊方法")]
//    public IOperResult<string> TestMethod(string query1, string query2, CancellationToken cancellationToken)
//    {
//        return new OperResult<string>() { Content = "测试特殊方法" };
//    }
//}

///// <summary>
///// 插件类配置
///// </summary>
//public class TestCollectProperty1 : CollectPropertyBase
//{
//    /// <summary>
//    /// 添加<see cref="DynamicPropertyAttribute"/> 特性，如需多语言配置，可添加json资源，参考其他插件
//    /// </summary>
//    [DynamicProperty(Description = null, Remark = null)]
//    public string TestString { get; set; }

//}
