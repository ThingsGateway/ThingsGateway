using Furion.FriendlyException;

using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Core.Models;
using Magicodes.ExporterAndImporter.Excel;

using Microsoft.AspNetCore.Components.Forms;

using NewLife;

using System.IO;
using System.Linq;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class CollectDeviceService : DbRepository<CollectDevice>, ICollectDeviceService
    {
        private readonly SysCacheService _sysCacheService;
        private readonly IDriverPluginService _driverPluginService;
        private readonly IFileService _fileService;
        private readonly IServiceScopeFactory _scopeFactory;

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
        public async Task Add(CollectDevice input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0)
                throw Oops.Bah($"存在重复的名称:{input.Name}");
            var result = await InsertReturnEntityAsync(input);//添加数据
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除

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
        public List<DeviceTree> GetTree()
        {
            var data = GetCacheList();
         var trees=   GetTree(data);
            return trees;
        }

        public static List<DeviceTree> GetTree(List<CollectDevice> data)
        {
            Dictionary<string, DeviceTree> trees = new();
            foreach (var item in data)
            {
                if (item.DeviceGroup.IsNullOrEmpty())
                {
                    trees.Add(item.Name, new() { Name = item.Name, Childrens = null });
                }
                else
                {
                    if (trees.ContainsKey(item.DeviceGroup))
                    {
                        trees[item.DeviceGroup].Childrens.Add(new() { Name = item.Name, Childrens = null });
                    }
                    else
                    {
                        trees.Add(item.DeviceGroup, new()
                        {
                            Name = item.DeviceGroup,
                            Childrens = new() { new() { Name = item.Name, Childrens = null } }
                        });
                    }
                }
            }
            return trees.Values?.ToList();
        }

        /// <inheritdoc/>
        [OperDesc("删除采集设备")]
        public async Task Delete(List<BaseIdInput> input)
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
        public async Task Edit(CollectDeviceEditInput input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0 && account_Id != input.Id)
                throw Oops.Bah($"存在重复的名称:{input.Name}");

            if (await Context.Updateable(input.Adapt<CollectDevice>()).ExecuteCommandAsync() > 0)//修改数据
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<CollectDevice>> Page(CollectDevicePageInput input)
        {
            long? pluginid = 0;
            if (!string.IsNullOrEmpty(input.PluginName))
            {
                pluginid = _driverPluginService.GetCacheListAsync().FirstOrDefault(it => it.AssembleName.Contains(input.PluginName))?.Id;
            }
            var query = Context.Queryable<CollectDevice>()
             .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
             .WhereIF(!string.IsNullOrEmpty(input.DeviceGroup), u => u.DeviceGroup==input.DeviceGroup)
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
        public async Task<List<CollectDeviceRunTime>> GetCollectDeviceRuntime(long devId = 0)
        {
            if (devId == 0)
            {
                var devices = GetCacheList();
                var runtime = devices.Adapt<List<CollectDeviceRunTime>>();
                using var serviceScope = _scopeFactory.CreateScope();
                var variableService = serviceScope.ServiceProvider.GetService<IVariableService>();
                var collectVariableRunTimes = await variableService.GetCollectDeviceVariableRuntime();
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
                    var collectVariableRunTimes = await variableService.GetCollectDeviceVariableRuntime(devId);
                    device.PluginName = pluginName;
                    device.DeviceVariableRunTimes = collectVariableRunTimes;
                }
                return runtime;

            }

        }

        #region 导入导出
        [OperDesc("导出采集设备模板", IsRecordPar = false)]
        public async Task<MemoryStream> Template()
        {
            IImporter Importer = new ExcelImporter();
            var byteArray = await Importer.GenerateTemplateBytes<CollectDeviceWithPropertyImport>();

            var result = new MemoryStream(byteArray);
            return result;
        }

        [OperDesc("导出采集设备表", IsRecordPar = false)]
        public async Task<MemoryStream> ExportFile()
        {
            var devDatas = GetCacheList();

            var devExports = devDatas.Adapt<List<CollectDeviceExport>>();
            //需要手动改正插件名称
            devExports.ForEach(it => it.PluginName = _driverPluginService.GetNameById(it.PluginId));

            List<DevicePropertyExport> devicePropertys = new List<DevicePropertyExport>();
            foreach (var devData in devDatas)
            {
                var propertyExcels = devData.DevicePropertys.Adapt<List<DevicePropertyExport>>();
                //需要手动改正设备名称
                propertyExcels.ForEach(it => it.DeviceName = devData.Name);
                devicePropertys.AddRange(propertyExcels);
            }

            var exporter = new ExcelExporter();
            var byteArray = await exporter.Append(devExports)
.SeparateBySheet()
    .Append(devicePropertys).ExportAppendDataAsByteArray();
            var result = new MemoryStream(byteArray);

            return result;
        }
        /// <inheritdoc/>
        public async Task<Dictionary<string, ImportPreviewOutputBase>> Preview(IBrowserFile file)
        {
            _fileService.ImportVerification(file);
            var Importer = new ExcelImporter();
            using var fs = new MemoryStream();
            using var stream = file.OpenReadStream(5120000);
            await stream.CopyToAsync(fs);
            var importDic = await Importer.ImportMultipleSheet<CollectDeviceWithPropertyImport>(fs);//导入的文件转化为带入结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            ImportPreviewOutput<CollectDeviceImport> DeviceImportPreview = new();
            foreach (var item in importDic)
            {
                //导入的Sheet数据，
                if (item.Key == "采集设备")
                {
                    //多个不同类型的Sheet返回的值为object，需要进行类型转换
                    var import = item.Value.Adapt<ImportResult<CollectDeviceImport>>();
                    DeviceImportPreview = _fileService.TemplateDataVerification(import);//验证数据完整度
                    //遍历错误的行
                    import.RowErrors.ForEach(row =>
                    {
                        row.RowIndex -= 2;//下表与列表中的下标一致
                        DeviceImportPreview.Data[row.RowIndex].HasError = true;//错误的行HasError = true
                        DeviceImportPreview.Data[row.RowIndex].ErrorInfo = row.FieldErrors;
                    });

                    for (int i = 0; i < DeviceImportPreview.Data.Count; i++)
                    {
                        var data = DeviceImportPreview.Data[i];
                        var error = data.ErrorInfo ?? new Dictionary<string, string>();
                        if (!data.HasError)
                        {
                            if (_driverPluginService.GetIdByName(data.PluginName) == null)
                            {
                                //找不到对应的插件
                                data.HasError = true;
                                error.TryAdd(data.Description(it => it.PluginName), "不存在这个插件");
                                DeviceImportPreview.HasError = true;
                                DeviceImportPreview.RowErrors.Add(new DataRowErrorInfo() { RowIndex = i, FieldErrors = error });
                            }
                            else
                            {
                                data.PluginId = _driverPluginService.GetIdByName(data.PluginName).ToLong();
                                data.Id = this.GetIdByName(data.Name) ?? YitIdHelper.NextId();
                            }
                        }

                        data.ErrorInfo = error;

                    }

                    ImportPreviews.Add(item.Key, DeviceImportPreview);
                }
                if (item.Key == "设备附加属性")
                {
                    //多个不同类型的Sheet返回的值为object，需要进行类型转换
                    var import = item.Value.Adapt<ImportResult<DevicePropertyImport>>();
                    ImportPreviewOutput<DevicePropertyImport> ImportPreview = _fileService.TemplateDataVerification(import);//验证数据完整度
                    //遍历错误的行
                    import.RowErrors.ForEach(row =>
                    {
                        row.RowIndex -= 2;//下表与列表中的下标一致
                        ImportPreview.Data[row.RowIndex].HasError = true;//错误的行HasError = true
                        ImportPreview.Data[row.RowIndex].ErrorInfo = row.FieldErrors;
                    });
                    for (int i = 0; i < ImportPreview.Data.Count; i++)
                    {
                        var data = ImportPreview.Data[i];
                        var error = data.ErrorInfo ?? new Dictionary<string, string>();
                        if (!data.HasError)
                        {
                            if (this.GetIdByName(data.DeviceName) == null && !DeviceImportPreview.Data?.Any(it => it.Name == data.DeviceName && it.HasError == false) == true)
                            {
                                //找不到对应的设备
                                data.HasError = true;
                                error.TryAdd(data.Description(it => it.DeviceName), "不存在这个设备");
                                ImportPreview.HasError = true;
                                ImportPreview.RowErrors.Add(new DataRowErrorInfo() { RowIndex = i + 1, FieldErrors = error });
                            }
                            else
                            {
                                data.DeviceId = this.GetIdByName(data.DeviceName) ?? DeviceImportPreview.Data.FirstOrDefault(it => it.Name == data.DeviceName).Id;
                            }
                        }

                        data.ErrorInfo = error;

                    }
                    ImportPreviews.Add(item.Key, ImportPreview);
                }
            }

            return ImportPreviews;
        }

        /// <inheritdoc/>
        [OperDesc("导入采集设备表", IsRecordPar = false)]
        public async Task Import(Dictionary<string, ImportPreviewOutputBase> input)
        {
            var collectDevices = new List<CollectDevice>();
            foreach (var item in input)
            {
                if (item.Key == "采集设备")
                {
                    var collectDeviceImports = ((ImportPreviewOutput<CollectDeviceImport>)item.Value).Data;
                    collectDevices = collectDeviceImports.Adapt<List<CollectDevice>>();
                    break;
                }
            }
            foreach (var item in input)
            {
                if (item.Key == "设备附加属性")
                {
                    var propertys = ((ImportPreviewOutput<DevicePropertyImport>)item.Value).Data;
                    foreach (var collectDevice in collectDevices.Where(a => propertys.Select(b => b.DeviceId).ToList().Contains(a.Id)).ToList())
                    {
                        collectDevice.DevicePropertys = (propertys.Where(it => it.DeviceId == collectDevice.Id).ToList().Adapt<List<DependencyProperty>>());
                    }
                    break;
                }
            }
            await Context.Storageable(collectDevices).ExecuteCommandAsync();
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_CollectDevice, "");//cache删除

        }

        #endregion
    }
}