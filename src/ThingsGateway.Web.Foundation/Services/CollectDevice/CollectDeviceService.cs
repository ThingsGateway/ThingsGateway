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

using Microsoft.AspNetCore.Components.Forms;

using MiniExcelLibs;

using NewLife.Serialization;

using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <inheritdoc cref="ICollectDeviceService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class CollectDeviceService : DbRepository<CollectDevice>, ICollectDeviceService
    {
        private readonly SysCacheService _sysCacheService;
        private readonly IDriverPluginService _driverPluginService;
        private readonly IFileService _fileService;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <inheritdoc cref="ICollectDeviceService"/>
        public CollectDeviceService(SysCacheService sysCacheService
            , IDriverPluginService driverPluginService, IFileService fileService,
        IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _fileService = fileService;
            _sysCacheService = sysCacheService;

            _driverPluginService = driverPluginService;
        }

        /// <inheritdoc/>
        [OperDesc("添加采集设备")]
        public async Task AddAsync(CollectDevice input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0)
                throw Oops.Bah($"存在重复的名称:{input.Name}");
            var result = await InsertReturnEntityAsync(input);//添加数据
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除

        }

        /// <inheritdoc/>
        [OperDesc("复制采集设备")]
        public async Task CopyDevAsync(IEnumerable<CollectDevice> input)
        {
            var newId = Yitter.IdGenerator.YitIdHelper.NextId();
            var newDevs = input.Adapt<List<CollectDevice>>();
            newDevs.ForEach(a =>
            {
                a.Id = newId;
                a.Name = "Copy-" + a.Name + "-" + newId.ToString();
            });

            var result = await InsertRangeAsync(newDevs);//添加数据
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除

        }
        /// <inheritdoc/>
        [OperDesc("复制采集设备与变量")]
        public async Task CopyDevAndVarAsync(IEnumerable<CollectDevice> input)
        {

            using var serviceScope = _scopeFactory.CreateScope();
            var variableService = serviceScope.ServiceProvider.GetService<IVariableService>();
            List<CollectDeviceVariable> collectDeviceVariables = new();
            var newDevs = input.Adapt<List<CollectDevice>>();
            foreach (var item in newDevs)
            {
                var newId = YitIdHelper.NextId();
                var deviceVariables = await Context.Queryable<CollectDeviceVariable>().Where(a => a.DeviceId == item.Id).ToListAsync();
                deviceVariables.ForEach(b =>
                {
                    b.Id = 0;
                    b.DeviceId = newId;
                    b.Name = b.Name + YitIdHelper.NextId();
                });
                collectDeviceVariables.AddRange(deviceVariables);
                item.Id = newId;
                item.Name = "Copy-" + item.Name + "-" + Yitter.IdGenerator.YitIdHelper.NextId().ToString();
            }

            var result = await itenant.UseTranAsync(async () =>
            {
                await InsertRangeAsync(newDevs);//添加数据
                await Context.Insertable(collectDeviceVariables).ExecuteCommandAsync();//添加数据
            });

            if (result.IsSuccess)
            {
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除

            }
            else
            {
                throw Oops.Oh(ErrorCodeEnum.A0003 + result.ErrorMessage);
            }


        }
        /// <inheritdoc/>
        public long? GetIdByName(string name)
        {
            var data = GetCacheList();
            return data.FirstOrDefault(it => it.Name == name)?.Id;
        }
        /// <inheritdoc/>
        public string GetNameById(long id)
        {
            var data = GetCacheList();
            return data.FirstOrDefault(it => it.Id == id)?.Name;
        }
        /// <inheritdoc/>
        public List<DeviceTree> GetTree()
        {
            var data = GetCacheList();
            var trees = CollectDeviceServiceHelpers.GetTree(data);
            return trees;
        }

        /// <inheritdoc/>
        [OperDesc("删除采集设备")]
        public async Task DeleteAsync(List<BaseIdInput> input)
        {
            //获取所有ID
            var ids = input.Select(it => it.Id).ToList();
            if (ids.Count > 0)
            {
                var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
                using var serviceScope = _scopeFactory.CreateScope();
                var variableService = serviceScope.ServiceProvider.GetService<IVariableService>();
                await variableService.Context.Deleteable<CollectDeviceVariable>(it => ids.Contains(it.DeviceId)).ExecuteCommandAsync();
                variableService.DeleteVariableFromCache();
                if (result)
                {
                    _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除
                }
            }
        }

        /// <inheritdoc/>
        [OperDesc("编辑采集设备")]
        public async Task EditAsync(CollectDeviceEditInput input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0 && account_Id != input.Id)
                throw Oops.Bah($"存在重复的名称:{input.Name}");

            if (await Context.Updateable(input.Adapt<CollectDevice>()).ExecuteCommandAsync() > 0)//修改数据
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<CollectDevice>> PageAsync(CollectDevicePageInput input)
        {
            long? pluginid = 0;
            if (!string.IsNullOrEmpty(input.PluginName))
            {
                pluginid = _driverPluginService.GetCacheList().FirstOrDefault(it => it.AssembleName.Contains(input.PluginName))?.Id;
            }
            var query = Context.Queryable<CollectDevice>()
             .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
             .WhereIF(!string.IsNullOrEmpty(input.DeviceGroup), u => u.DeviceGroup == input.DeviceGroup)
             .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginId == (pluginid ?? 0))
             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
             .OrderBy(u => u.Id)//排序
             .Select((u) => new CollectDevice { Id = u.Id.SelectAll() })
                ;
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

        /// <inheritdoc/>
        public CollectDevice GetDeviceById(long Id)
        {
            var data = GetCacheList();
            return data.FirstOrDefault(it => it.Id == Id);
        }
        /// <inheritdoc/>
        public List<CollectDevice> GetCacheList()
        {
            //先从Cache拿
            var collectDevice = _sysCacheService.Get<List<CollectDevice>>(ThingsGatewayCacheConst.Cache_CollectDevice, "");
            if (collectDevice == null)
            {
                collectDevice = Context.Queryable<CollectDevice>()
                .Select((u) => new CollectDevice { Id = u.Id.SelectAll() })
                .ToList();
                if (collectDevice != null)//做个大小写限制
                {
                    //插入Cache
                    _sysCacheService.Set(ThingsGatewayCacheConst.Cache_CollectDevice, "", collectDevice);
                }
            }
            return collectDevice;
        }

        /// <inheritdoc/>
        public async Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntimeAsync(long devId = 0)
        {
            if (devId == 0)
            {
                var devices = GetCacheList();
                var runtime = devices.Adapt<List<CollectDeviceRunTime>>();
                using var serviceScope = _scopeFactory.CreateScope();
                var variableService = serviceScope.ServiceProvider.GetService<IVariableService>();
                var collectVariableRunTimes = await variableService.GetCollectDeviceVariableRuntimeAsync();
                foreach (var device in runtime)
                {
                    var pluginName = _driverPluginService.GetNameById(device.PluginId);
                    device.PluginName = pluginName;
                    device.DeviceVariableRunTimes = collectVariableRunTimes.Where(a => a.DeviceId == device.Id).ToList();
                }
                return runtime;
            }
            else
            {
                var devices = GetCacheList();
                devices = devices.Where(it => it.Id == devId).ToList();
                var runtime = devices.Adapt<List<CollectDeviceRunTime>>();
                using var serviceScope = _scopeFactory.CreateScope();
                var variableService = serviceScope.ServiceProvider.GetService<IVariableService>();
                foreach (var device in runtime)
                {
                    var pluginName = _driverPluginService.GetNameById(device.PluginId);
                    var collectVariableRunTimes = await variableService.GetCollectDeviceVariableRuntimeAsync(devId);
                    device.PluginName = pluginName;
                    device.DeviceVariableRunTimes = collectVariableRunTimes;
                }
                return runtime;

            }

        }

        #region 导入导出
        //查找出列表中的所有重复元素及其重复次数
        private static Dictionary<string, int> QueryRepeatElementAndCountOfList(IEnumerable<IDictionary<string, object>> list)
        {
            Dictionary<string, int> DicTmp = new Dictionary<string, int>();
            if (list != null && list.Count() > 0)
            {
                DicTmp = list.GroupBy(x => ((ExpandoObject)x).ConvertToEntity<CollectDevice>().Name)
                             .Where(g => g.Count() > 1)
               .ToDictionary(x => x.Key, y => y.Count());
            }
            return DicTmp;
        }

        /// <summary>
        /// 插件前置名称
        /// </summary>
        public const string PluginLeftName = "ThingsGateway.";
        /// <summary>
        /// 设备表名称
        /// </summary>
        public const string CollectDeviceSheetName = "采集设备";
        /// <inheritdoc/>
        [OperDesc("导出采集设备表", IsRecordPar = false)]
        public async Task<MemoryStream> ExportFileAsync()
        {

            var devDatas = GetCacheList();

            //总数据
            Dictionary<string, object> sheets = new Dictionary<string, object>();
            //设备页
            List<Dictionary<string, object>> devExports = new();
            //设备附加属性，转成Dict<表名,List<Dict<列名，列数据>>>的形式
            Dictionary<string, List<Dictionary<string, object>>> devicePropertys = new();

            foreach (var devData in devDatas)
            {

                //设备页
                var data = devData.GetType().GetAllProps().Where(a => a.GetCustomAttribute<ExcelAttribute>() != null);
                Dictionary<string, object> devExport = new();
                foreach (var item in data)
                {
                    //描述
                    var desc = ObjectExtensions.FindDisplayAttribute(item);
                    //数据源增加
                    devExport.Add(desc ?? item.Name, item.GetValue(devData)?.ToString());
                }
                //设备实体没有包含插件名称，手动插入
                devExport.Add("插件", _driverPluginService.GetNameById(devData.PluginId));
                //添加完整设备信息
                devExports.Add(devExport);

                //插件属性
                //单个设备的行数据
                Dictionary<string, object> driverInfo = new Dictionary<string, object>();
                foreach (var item in devData.DevicePropertys ?? new())
                {
                    //添加对应属性数据
                    driverInfo.Add(item.Description, item.Value);
                }
                //没有包含设备名称，手动插入
                if (driverInfo.Count > 0)
                {
                    driverInfo.Add("设备", devData.Name);
                }

                //插件名称去除首部ThingsGateway.作为表名
                var pluginName = _driverPluginService.GetNameById(devData.PluginId).Replace(PluginLeftName, "");
                if (devicePropertys.ContainsKey(pluginName))
                {
                    if (driverInfo.Count > 0)
                        devicePropertys[pluginName].Add(driverInfo);
                }
                else
                {
                    if (driverInfo.Count > 0)
                        devicePropertys.Add(pluginName, new() { driverInfo });
                }

            }

            //添加设备页
            sheets.Add(CollectDeviceSheetName, devExports);
            //添加插件属性页
            foreach (var item in devicePropertys)
            {
                sheets.Add(item.Key, item.Value);
            }

            var memoryStream = new MemoryStream();
            await memoryStream.SaveAsAsync(sheets);
            return memoryStream;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile file)
        {
            _fileService.ImportVerification(file);
            using var fs = new MemoryStream();
            using var stream = file.OpenReadStream(512000000);
            await stream.CopyToAsync(fs);
            var sheetNames = MiniExcel.GetSheetNames(fs);

            //导入检验结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            //设备页
            ImportPreviewOutput<CollectDevice> deviceImportPreview = new();
            foreach (var sheetName in sheetNames)
            {
                //单页数据
                var rows = fs.Query(useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();
                if (sheetName == CollectDeviceSheetName)
                {
                    ImportPreviewOutput<CollectDevice> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);
                    deviceImportPreview = importPreviewOutput;

                    var DicTmp = QueryRepeatElementAndCountOfList(rows);
                    if (DicTmp.Count > 0)
                    {
                        throw new Exception("发现重复名称" + Environment.NewLine + DicTmp.Select(a => a.Key).ToJson());
                    }
                    List<CollectDevice> devices = new List<CollectDevice>();
                    foreach (var item in rows)
                    {
                        var device = ((ExpandoObject)item).ConvertToEntity<CollectDevice>();
                        devices.Add(device);
                        //转化插件名称
                        var value = item.FirstOrDefault(a => a.Key == "插件");
                        if (_driverPluginService.GetIdByName(value.Value.ToString()) == null)
                        {
                            //找不到对应的插件
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.ErrorStr = "插件名称不存在";
                            return ImportPreviews;
                        }
                        else
                        {
                            //插件ID和设备ID都需要手动补录
                            device.PluginId = _driverPluginService.GetIdByName(value.Value.ToString()).ToLong();
                            device.Id = this.GetIdByName(device.Name) ?? YitIdHelper.NextId();
                        }
                    }
                    importPreviewOutput.Data = devices;

                }
                else
                {
                    //插件属性需加上前置名称
                    var newName = PluginLeftName + sheetName;
                    var pluginId = _driverPluginService.GetIdByName(newName);
                    if (pluginId == null)
                    {
                        deviceImportPreview.HasError = true;
                        deviceImportPreview.ErrorStr = deviceImportPreview.ErrorStr + Environment.NewLine + $"设备{newName}不存在";
                        return ImportPreviews;
                    }

                    var driverPlugin = _driverPluginService.GetDriverPluginById(pluginId.Value);
                    using var serviceScope = _scopeFactory.CreateScope();
                    var pluginSingletonService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
                    var driver = (CollectBase)pluginSingletonService.GetDriver(YitIdHelper.NextId(), driverPlugin);
                    var propertys = driver.DriverPropertys.GetType().GetAllProps().Where(a => a.GetCustomAttribute<DevicePropertyAttribute>() != null);
                    foreach (var item in rows)
                    {
                        List<DependencyProperty> devices = new List<DependencyProperty>();
                        foreach (var item1 in item)
                        {
                            var propertyInfo = propertys.FirstOrDefault(p => p.FindDisplayAttribute(a => a.GetCustomAttribute<DevicePropertyAttribute>()?.Name) == item1.Key);
                            if (propertyInfo == null)
                            {
                                //不存在时不报错
                            }
                            else
                            {
                                devices.Add(new()
                                {
                                    PropertyName = propertyInfo.Name,
                                    Description = item1.Key.ToString(),
                                    Value = item1.Value?.ToString()
                                });
                            }

                        }
                        //转化插件名称
                        var value = item.FirstOrDefault(a => a.Key == "设备");
                        if (deviceImportPreview.Data?.Any(it => it.Name == value.Value.ToString()) == true)
                        {
                            var deviceId = this.GetIdByName(value.Value.ToString()) ?? deviceImportPreview.Data.FirstOrDefault(it => it.Name == value.Value.ToString()).Id;
                            deviceImportPreview.Data.FirstOrDefault(a => a.Id == deviceId).DevicePropertys = devices;
                        }

                    }

                }
            }



            return ImportPreviews;
        }

        /// <inheritdoc/>
        [OperDesc("导入采集设备表", IsRecordPar = false)]
        public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
        {
            var collectDevices = new List<CollectDevice>();
            foreach (var item in input)
            {
                if (item.Key == CollectDeviceSheetName)
                {
                    var collectDeviceImports = ((ImportPreviewOutput<CollectDevice>)item.Value).Data;
                    collectDevices = collectDeviceImports.Adapt<List<CollectDevice>>();
                    break;
                }
            }
            await Context.Storageable(collectDevices).ExecuteCommandAsync();
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除

        }

        #endregion
    }
}