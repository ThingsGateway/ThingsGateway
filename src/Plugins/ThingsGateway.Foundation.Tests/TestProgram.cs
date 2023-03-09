using Furion.Xunit;

using Xunit;
using Xunit.Abstractions;

[assembly: TestFramework("ThingsGateway.Foundation.Tests.TestProgram", "ThingsGateway.Foundation.Tests")]
namespace ThingsGateway.Foundation.Tests;

/// <summary>
/// 单元测试启动类
/// </summary>

public class TestProgram : TestStartup
{
    public TestProgram(IMessageSink messageSink) : base(messageSink)
    {
        // 初始化 Furion
        Serve.Run(silence: true);
    }
}