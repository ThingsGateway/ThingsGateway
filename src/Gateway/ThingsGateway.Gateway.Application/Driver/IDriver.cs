// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application
{
    public interface IDriver : IDisposable
    {
        bool DisposedValue { get; }
        ChannelRuntime CurrentChannel { get; }
        DeviceRuntime? CurrentDevice { get; }
        long DeviceId { get; }
        string? DeviceName { get; }
        Type DriverDebugUIType { get; }
        object DriverProperties { get; }

        Type DriverPropertyUIType { get; }
        Type DriverUIType { get; }
        Type DriverVariableAddressUIType { get; }
        IDevice? FoundationDevice { get; }
        bool? IsCollectDevice { get; }
        bool IsInitSuccess { get; }
        bool IsStarted { get; }
        LoggerGroup LogMessage { get; }
        string LogPath { get; }
        bool Pause { get; }
        string PluginDirectory { get; }
        List<IEditorItem> PluginPropertyEditorItems { get; }
        Dictionary<string, VariableRuntime> VariableRuntimes { get; }
        IDeviceThreadManage DeviceThreadManage { get; }

        bool IsConnected();
        void PauseThread(bool pause);
        Task SetLogAsync(bool enable, LogLevel? logLevel = null, bool upDataBase = true);
        Task AfterVariablesChangedAsync();
    }
}