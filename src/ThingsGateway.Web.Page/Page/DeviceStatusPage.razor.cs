#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using Mapster;

using Masa.Blazor;

using Microsoft.JSInterop;

using System;
using System.IO;

using TouchSocket.Core;

namespace ThingsGateway.Web.Page
{
    public partial class DeviceStatusPage
    {
        List<CollectDeviceCore> _collectDeviceCores = new();
        private string _collectDeviceGroup;
        List<string> _collectDeviceGroups = new();
        string _collectDeviceGroupSearchName;
        List<UploadDeviceCore> _uploadDeviceCores = new();
        private string _uploadDeviceGroup;
        List<string> _uploadDeviceGroups = new();
        string _uploadDeviceGroupSearchName;
        CollectDeviceCore collectDeviceInfoItem;
        List<string> CurMessages = new();
        private System.Timers.Timer DelayTimer;
        private bool fab;
        bool isAllRestart;
        private bool isDownExport;
        bool isRestart;
        bool pauseMessage;
        StringNumber tab;
        UploadDeviceCore uploadDeviceInfoItem;
        [Inject]
        public JsInitVariables JsInitVariables { get; set; } = default!;

        AlarmWorker AlarmHostService { get; set; }
        CollectDeviceWorker CollectDeviceHostService { get; set; }
        [Inject]
        IJSRuntime JS { get; set; }

        StringNumber panel { get; set; }

        UploadDeviceWorker UploadDeviceHostService { get; set; }
        StringNumber uppanel { get; set; }
        HistoryValueWorker HistoryValueHostService { get; set; }
        MemoryVariableWorker MemoryVariableWorker { get; set; }

        [Inject]
        IVariableService VariableService { get; set; }

        protected override async Task DisposeAsync(bool disposing)
        {
            await base.DisposeAsync(disposing);
            DelayTimer?.SafeDispose();
        }

        protected override Task OnInitializedAsync()
        {
            CollectDeviceHostService = ServiceExtensions.GetBackgroundService<CollectDeviceWorker>();
            UploadDeviceHostService = ServiceExtensions.GetBackgroundService<UploadDeviceWorker>();
            AlarmHostService = ServiceExtensions.GetBackgroundService<AlarmWorker>();
            HistoryValueHostService = ServiceExtensions.GetBackgroundService<HistoryValueWorker>();
            MemoryVariableWorker = ServiceExtensions.GetBackgroundService<MemoryVariableWorker>();
            DelayTimer = new System.Timers.Timer(1000);
            DelayTimer.Elapsed += timer_Elapsed;
            DelayTimer.AutoReset = true;
            DelayTimer.Start();
            return base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            collectDeviceQuery();
            uploadDeviceQuery();
            await base.OnParametersSetAsync();
        }

        async Task AllRestart()
        {
            try
            {
                var confirm = await PopupService.OpenConfirmDialogAsync(T("����"), T("ȷ������?"));
                if (confirm)
                {
                    isAllRestart = true;
                    StateHasChanged();
                    await Task.Run(async () => await CollectDeviceHostService.RestartDeviceThreadAsync());
                    collectDeviceQuery();
                    uploadDeviceQuery();
                }
            }
            finally
            {
                isAllRestart = false;
            }
        }

        void collectDeviceInfo(CollectDeviceCore item)
        {
            collectDeviceInfoItem = item;
            CurMessages = new();
        }
        [Inject]
        GlobalDeviceData _globalDeviceData { get; set; }
        void collectDeviceQuery()
        {
            _collectDeviceGroups = _globalDeviceData.CollectDevices.Adapt<List<CollectDevice>>()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList() ?? new();
            _collectDeviceCores = CollectDeviceHostService?.CollectDeviceCores?.WhereIf(!_collectDeviceGroup.IsNullOrEmpty(), a => a.Device?.DeviceGroup == _collectDeviceGroup).ToList() ?? new();
        }

        async Task Config(long devId, bool? isStart)
        {
            var str = isStart == true ? T("����") : T("��ͣ");
            var confirm = await PopupService.OpenConfirmDialogAsync(str, $"ȷ��{str}?");
            if (confirm)
            {
                await CollectDeviceHostService.ConfigDeviceThreadAsync(devId, isStart == true);
            }
        }

        async Task DownDeviceMessageExport(List<string> values)
        {
            try
            {
                isDownExport = true;
                StateHasChanged();
                using var memoryStream = new MemoryStream();
                StreamWriter writer = new StreamWriter(memoryStream);
                foreach (var item in values)
                {
                    writer.WriteLine(item);
                }

                writer.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var streamRef = new DotNetStreamReference(stream: memoryStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", $"���ĵ���{DateTime.UtcNow.Add(JsInitVariables.TimezoneOffset).ToString("yyyy-MM-dd HH:mm:ss fff")}.txt", streamRef);
            }
            finally
            {
                isDownExport = false;
            }
        }

        async Task Restart(long devId)
        {
            try
            {
                var confirm = await PopupService.OpenConfirmDialogAsync(T("����"), T("ȷ������?"));
                if (confirm)
                {
                    isRestart = true;
                    StateHasChanged();
                    await Task.Run(async () => await CollectDeviceHostService.UpDeviceThreadAsync(devId));
                    collectDeviceQuery();
                }
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Warning);
            }
            finally
            {
                isRestart = false;
            }
        }

        async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_collectDeviceCores?.FirstOrDefault()?.Device == null)
                {
                    collectDeviceQuery();
                }

                if (_uploadDeviceCores?.FirstOrDefault()?.Device == null)
                {
                    uploadDeviceQuery();
                }

                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }
        }

        async Task UpConfig(long devId, bool? isStart)
        {
            var str = isStart == true ? T("����") : T("��ͣ");
            var confirm = await PopupService.OpenConfirmDialogAsync(str, $"ȷ��{str}?");
            if (confirm)
            {
                await UploadDeviceHostService.ConfigDeviceThreadAsync(devId, isStart == true);
            }
        }

        void uploadDeviceInfo(UploadDeviceCore item)
        {
            uploadDeviceInfoItem = item;
        }

        void uploadDeviceQuery()
        {
            _uploadDeviceGroups = UploadDeviceHostService.UploadDeviceRunTimes.Adapt<List<CollectDevice>>()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList() ?? new();
            _uploadDeviceCores = UploadDeviceHostService?.UploadDeviceCores?.WhereIf(!_uploadDeviceGroup.IsNullOrEmpty(), a => a.Device?.DeviceGroup == _uploadDeviceGroup).ToList() ?? new();
        }
        async Task UpRestart(long devId)
        {
            try
            {
                var confirm = await PopupService.OpenConfirmDialogAsync(T("����"), T("ȷ������?"));
                if (confirm)
                {
                    isRestart = true;
                    StateHasChanged();
                    await Task.Run(async () => await UploadDeviceHostService.UpDeviceThreadAsync(devId));
                    uploadDeviceQuery();
                }
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Warning);
            }
            finally
            {
                isRestart = false;
            }
        }
    }
}