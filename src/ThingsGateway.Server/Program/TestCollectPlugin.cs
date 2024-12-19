// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

//using Newtonsoft.Json.Linq;

//using ThingsGateway.Foundation;
//using ThingsGateway.Gateway.Application;

//namespace ThingsGateway.Server;

///// <summary>
///// 插件类
///// </summary>
//public class TestCollectPlugin : CollectBase
//{
//    /// <summary>
//    /// 插件配置项，继承<see cref="CollectPropertyBase"/> 返回类实例
//    /// </summary>
//    public override CollectPropertyBase CollectProperties => _property;
//    private TestCollectProperty? _property = new();

//    /// <summary>
//    /// 插件默认的PLC通讯类，如未实现，返回null
//    /// </summary>
//    public override IProtocol? Protocol => null;

//    /// <summary>
//    /// 在插件初始化时调用，只会执行一次，参数为插件默认的链路通道类，如未实现可忽略l
//    /// </summary>
//    protected override void Init(IChannel? channel = null)
//    {
//        //做一些初始化操作
//    }

//    /// <summary>
//    /// 变量打包操作，会在Init方法后执行，参数为设备变量列表，返回源读取变量列表
//    /// </summary>
//    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
//    {
//        //实现将设备变量打包成源读取变量
//        //比如如果需要实现MC中的字多读功能，需将多个变量地址打包成一个源读取地址和读取长度，根据一系列规则，添加解析标识，然后在返回的整个字节数组中解析出原来的变量地址代表的数据字节

//        //一般可操作 VariableRunTime 类中的 index, thingsgatewaybitconvter 等属性
//        //一般可操作 VariableSourceRead 类中的 address, length 等属性

//        return new List<VariableSourceRead>();
//    }

//    /// <summary>
//    /// 开始前执行
//    /// </summary>
//    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
//    {
//        //一般实现PLC长连接
//        return base.ProtectedBeforStartAsync(cancellationToken);
//    }

//    /// <summary>
//    /// 循环执行方法，父类会自动调用<see cref="ReadSourceAsync(VariableSourceRead, CancellationToken)"/>
//    /// <br></br>
//    /// 一般需要更新设备变量值，调用<see cref="VariableRunTime.SetValue(object?, DateTime, bool)"/>
//    /// <br></br>
//    /// 通讯失败时，更新设备状态，调用<see cref="DeviceRunTime.SetDeviceStatus(DateTime?, int?, string)"/>
//    /// <br></br>
//    /// </summary>
//    protected override ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken)
//    {
//        return base.ProtectedExecuteAsync(cancellationToken);
//    }

//    /// <summary>
//    /// 写入变量，实现设备写入操作
//    /// </summary>
//    protected override ValueTask<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
//    {
//        return base.WriteValuesAsync(writeInfoLists, cancellationToken);
//    }

//    /// <summary>
//    /// 读取源变量，如重写了<see cref="ProtectedExecuteAsync"/> ，此方法可能不会执行
//    /// 一般需要更新设备变量值，调用<see cref="VariableRunTime.SetValue(object?, DateTime, bool)"/>
//    /// </summary>
//    protected override ValueTask<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead variableSourceRead, CancellationToken cancellationToken)
//    {
//        return base.ReadSourceAsync(variableSourceRead, cancellationToken);
//    }

//    /// <summary>
//    /// 插件释放
//    /// </summary>
//    /// <param name="disposing"></param>
//    protected override void Dispose(bool disposing)
//    {
//        base.Dispose(disposing);
//    }


//    /// <summary>
//    /// 特殊方法，添加<see cref="DynamicMethodAttribute"/> 特性
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
//public class TestCollectProperty : CollectPropertyBase
//{
//    /// <summary>
//    /// 添加<see cref="DynamicPropertyAttribute"/> 特性，如需多语言配置，可添加json资源，参考其他插件
//    /// </summary>
//    [DynamicProperty(Description = null, Remark = null)]
//    public string TestString { get; set; }

//}
