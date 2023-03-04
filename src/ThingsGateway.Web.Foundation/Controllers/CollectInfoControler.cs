using Furion.DynamicApiController;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NewLife;

using System.Linq;

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 采集设备
    /// </summary>
    [ApiDescriptionSettings(CateGoryConst.ThingsGatewayOpenApi, Order = 200)]
    [Route("openApi/collectInfo")]
    [Description("获取采集信息")]
    [OpenApiPermission]
    [LoggingMonitor]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CollectInfoControler : IDynamicApiController
    {
        IServiceScopeFactory _scopeFactory;
        CollectDeviceHostService _collectDeviceHostService { get; set; }
        AlarmHostService _alarmHostService { get; set; }
        public CollectInfoControler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            using var serviceScope = _scopeFactory.CreateScope();
            _collectDeviceHostService = serviceScope.ServiceProvider.GetBackgroundService<CollectDeviceHostService>();
            _alarmHostService = serviceScope.ServiceProvider.GetBackgroundService<AlarmHostService>();
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("collectDeviceList")]
        [Description("获取设备信息")]
        public List<DeviceData> GetCollectDeviceList()
        {
            return _collectDeviceHostService.CollectDeviceCores.Select(a => a.Device).ToList().Adapt<List<DeviceData>>();
        }
        /// <summary>
        /// 获取变量信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("collectVariableList")]
        [Description("获取变量信息")]
        public async Task<SqlSugarPagedList<VariableData>> GetCollectDeviceList(string name, string devName, int pageIndex = 1, int pageSize = 50)
        {
            var data = await _collectDeviceHostService.CollectDeviceCores.SelectMany(a => a.Device.DeviceVariableRunTimes)
                .WhereIf(!name.IsNullOrEmpty(), a => a.Name == name)
                .WhereIf(!devName.IsNullOrEmpty(), a => a.DeviceName == devName)
                .ToPagedListAsync(
                new BasePageInput()
                {
                    Current = pageIndex,
                    Size = pageSize,
                }
                );
            return data.Adapt<SqlSugarPagedList<VariableData>>();
        }

        /// <summary>
        /// 获取实时报警信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("realAlarmList")]
        [Description("获取实时报警信息")]
        public async Task<SqlSugarPagedList<VariableData>> GetRealAlarmList(string name, string devName, int pageIndex = 1, int pageSize = 50)
        {
            var data = await _alarmHostService.RealAlarmDeviceVariables
                .WhereIf(!name.IsNullOrEmpty(), a => a.Name == name)
                .WhereIf(!devName.IsNullOrEmpty(), a => a.DeviceName == devName)
                .ToPagedListAsync(
                new BasePageInput()
                {
                    Current = pageIndex,
                    Size = pageSize,
                }
                );
            return data.Adapt<SqlSugarPagedList<VariableData>>();
        }

    }
}

