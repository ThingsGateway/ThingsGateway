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

using Furion;

using Mapster;

using Microsoft.Extensions.Hosting;

using System.Diagnostics;
using System.Reflection;

using ThingsGateway.Foundation.Dmtp;
using ThingsGateway.Foundation.Dmtp.Rpc;
using ThingsGateway.Foundation.Rpc;
using ThingsGateway.Foundation.Sockets;

namespace ThingsGateway.Gateway.Application;

internal class ReverseCallbackServer : RpcServer
{
    private TcpClientBase _TcpClientBase;
    private IHostApplicationLifetime _appLifetime;
    ILog _logger;
    public ReverseCallbackServer(ILog log, TcpDmtpClient tcpDmtpClient, IHostApplicationLifetime appLifetime)
    {
        _logger = log;
        _TcpClientBase = tcpDmtpClient;
        _appLifetime = appLifetime;
    }
    [DmtpRpc(true)]//使用方法名作为调用键
    public async Task<GatewayExcel> GetGatewayExcelAsync()
    {
        GatewayExcel gatewayExcel = new GatewayExcel();
        gatewayExcel.CollectDevice = await App.GetService<CollectDeviceService>().ExportFileAsync(new CollectDeviceInput());
        gatewayExcel.UploadDevice = await App.GetService<UploadDeviceService>().ExportFileAsync(new UploadDeviceInput());
        gatewayExcel.MemoryVariable = await App.GetService<VariableService>().ExportFileAsync(new MemoryVariableInput());
        gatewayExcel.DeviceVariable = await App.GetService<VariableService>().ExportFileAsync(new DeviceVariableInput());

        return gatewayExcel;
    }

    [DmtpRpc(true)]//使用方法名作为调用键
    public GatewayInfo GetGatewayInfo()
    {
        GatewayInfo gatewayInfo = new GatewayInfo();
        var assembly = Assembly.GetEntryAssembly();
        var Version = $"v{assembly?.GetName().Version}" ?? string.Empty;
        gatewayInfo.Version = Version;
        gatewayInfo.UpdateTime = DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat();
        gatewayInfo.CollectDeviceCount = App.GetService<CollectDeviceService>().Context.Queryable<CollectDevice>().Count();
        gatewayInfo.UploadDeviceCount = App.GetService<UploadDeviceService>().Context.Queryable<UploadDevice>().Count();
        gatewayInfo.VariableCount = App.GetService<VariableService>().Context.Queryable<DeviceVariable>().Count();

        return gatewayInfo;
    }

    [DmtpRpc(true)]//使用方法名作为调用键
    public async Task DBRestartAsync()
    {
        _logger.LogInformation("网关运行态重启");
        await BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>().RestartDeviceThreadAsync();
    }

    [DmtpRpc(true)]//使用方法名作为调用键
    public async Task<OperResult> SetGatewayExcelAsync(GatewayExcel excel)
    {
        OperResult result = new();
        var collectDeviceService = App.GetService<CollectDeviceService>();
        var variableService = App.GetService<VariableService>();
        var uploadDeviceService = App.GetService<UploadDeviceService>();
        collectDeviceService.Context = variableService.Context = uploadDeviceService.Context;
        var itenant = collectDeviceService.Context.AsTenant();
        //事务
        var dbResult = await itenant.UseTranAsync(async () =>
        {
            if (excel.IsCollectDevicesFullUp)
            {
                await collectDeviceService.AsDeleteable().ExecuteCommandAsync();
            }
            var collectDevices = new List<CollectDevice>();

            if (excel.CollectDevice != null && excel.CollectDevice.Length > 0)
            {

                using MemoryStream stream = excel.CollectDevice;
                stream.Seek(0, SeekOrigin.Begin);
                var previewResult = await collectDeviceService.PreviewAsync(stream);
                if (previewResult.FirstOrDefault().Value.HasError)
                {
                    throw new(previewResult.Select(a => a.Value.Results.Where(a => !a.isSuccess).ToList()).ToList().ToJsonString());
                }
                foreach (var item in previewResult)
                {
                    if (item.Key == ExportHelpers.CollectDeviceSheetName)
                    {
                        var collectDeviceImports = ((ImportPreviewOutput<CollectDevice>)item.Value).Data;
                        collectDevices = collectDeviceImports.Values.Adapt<List<CollectDevice>>();
                        break;
                    }
                }
                await collectDeviceService.ImportAsync(previewResult);

            }

            if (excel.IsUploadDevicesFullUp)
            {
                await uploadDeviceService.AsDeleteable().ExecuteCommandAsync();

            }
            var uploadDevices = new List<UploadDevice>();

            if (excel.UploadDevice != null && excel.UploadDevice.Length > 0)
            {
                using MemoryStream stream1 = excel.UploadDevice;
                stream1.Seek(0, SeekOrigin.Begin);
                var previewResult1 = await uploadDeviceService.PreviewAsync(stream1);
                if (previewResult1.FirstOrDefault().Value.HasError)
                {
                    throw new(previewResult1.Select(a => a.Value.Results.Where(a => !a.isSuccess).ToList()).ToList().ToJsonString());
                }
                foreach (var item in previewResult1)
                {
                    if (item.Key == ExportHelpers.UploadDeviceSheetName)
                    {
                        var uploadDeviceImports = ((ImportPreviewOutput<UploadDevice>)item.Value).Data;
                        uploadDevices = uploadDeviceImports.Values.Adapt<List<UploadDevice>>();
                        break;
                    }
                }
                await uploadDeviceService.ImportAsync(previewResult1);

            }

            if (excel.IsDeviceVariablesFullUp)
            {
                await variableService.AsDeleteable().Where(a => !a.IsMemoryVariable).ExecuteCommandAsync();

            }
            if (excel.DeviceVariable != null && excel.DeviceVariable.Length > 0)
            {
                using MemoryStream stream2 = excel.DeviceVariable;
                stream2.Seek(0, SeekOrigin.Begin);
                var previewResult2 = await variableService.PreviewAsync(stream2, collectDevices, uploadDevices);
                if (previewResult2.FirstOrDefault().Value.HasError)
                {
                    throw new(previewResult2.Select(a => a.Value.Results.Where(a => !a.isSuccess).ToList()).ToList().ToJsonString());
                }
                await variableService.ImportAsync(previewResult2);
            }
            if (excel.IsMemoryVariablesFullUp)
            {
                await variableService.AsDeleteable().Where(a => a.IsMemoryVariable).ExecuteCommandAsync();
            }
            if (excel.MemoryVariable != null && excel.MemoryVariable.Length > 0)
            {
                using MemoryStream stream2 = excel.MemoryVariable;
                stream2.Seek(0, SeekOrigin.Begin);
                var previewResult2 = await variableService.PreviewAsync(stream2, collectDevices, uploadDevices);
                if (previewResult2.FirstOrDefault().Value.HasError)
                {
                    throw new(previewResult2.Select(a => a.Value.Results.Where(a => !a.isSuccess).ToList()).ToList().ToJsonString());
                }
                await variableService.ImportAsync(previewResult2);
            }
        });
        Cache.SysMemoryCache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除
        Cache.SysMemoryCache.Remove(ThingsGatewayCacheConst.Cache_UploadDevice);//cache删除

        if (dbResult.IsSuccess)//如果成功了
        {
            _logger.LogInformation("网关接收配置，并保存至数据库-执行成功");
            result = OperResult.CreateSuccessResult();
        }
        else
        {
            result.Message = dbResult.ErrorMessage;
        }
        return result;
    }
    private EasyLock EasyLock = new();
    /// <summary>
    /// 加锁避免重复重启更新
    /// </summary>
    [DmtpRpc(true)]//使用方法名作为调用键
    public void FileRestart()
    {
        //EasyLock.Wait();//不会释放，由外部程序直接关闭软件 //TODO:暂时注释
        _logger.LogInformation("准备更新软件，程序将退出");
        //TODO:启动控制台程序，用于文件转移/验证/覆盖原文件/重新启动/失败恢复等
        var path = FilePluginUtil.GetFileTempPath(_TcpClientBase);
        ProcessStart(path);
        _appLifetime.StopApplication();//停止程序
    }
    static void ProcessStart(string path)
    {
        string programPath = "ThingsGateway.Upgrade.exe"; // 要启动的程序的名称
        Process process = new Process();
        process.StartInfo.FileName = programPath;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.ArgumentList.Add(path);
        var data = process.Start();
    }
}






