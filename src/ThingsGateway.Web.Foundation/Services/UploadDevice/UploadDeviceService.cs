using Furion.FriendlyException;

using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Core.Models;
using Magicodes.ExporterAndImporter.Excel;

using Microsoft.AspNetCore.Components.Forms;

using System.IO;
using System.Linq;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class UploadDeviceService : DbRepository<UploadDevice>, IUploadDeviceService
    {
        private readonly SysCacheService _sysCacheService;
        private readonly IDriverPluginService _driverPluginService;
        private readonly IFileService _fileService;
        private readonly IServiceScopeFactory _scopeFactory;

        public UploadDeviceService(SysCacheService sysCacheService
            , IDriverPluginService driverPluginService, IFileService fileService,
        IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _fileService = fileService;
            _sysCacheService = sysCacheService;

            _driverPluginService = driverPluginService;
        }

        /// <inheritdoc/>
        [OperDesc("添加上传设备")]
        public async Task Add(UploadDevice input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0)
                throw Oops.Bah($"存在重复的名称:{input.Name}");
            var result = await InsertReturnEntityAsync(input);//添加数据
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除

        }

        /// <inheritdoc/>
        [OperDesc("复制上传设备")]
        public async Task CopyDev(IEnumerable<UploadDevice> input)
        {
            var newId = Yitter.IdGenerator.YitIdHelper.NextId();
            var newDevs = input.Adapt<List<UploadDevice>>();
            newDevs.ForEach(a =>
            {
                a.Id = newId;
                a.Name = "Copy-" + a.Name + "-" + newId.ToString();
            });

            var result = await InsertRangeAsync(newDevs);//添加数据
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除

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
        [OperDesc("删除上传设备")]
        public async Task Delete(List<BaseIdInput> input)
        {
            //获取所有ID
            var ids = input.Select(it => it.Id).ToList();
            if (ids.Count > 0)
            {
                var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
                if (result)
                {
                    _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除
                }
            }
        }

        /// <inheritdoc/>
        [OperDesc("编辑上传设备")]
        public async Task Edit(UploadDeviceEditInput input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0 && account_Id != input.Id)
                throw Oops.Bah($"存在重复的名称:{input.Name}");

            if (await Context.Updateable(input.Adapt<UploadDevice>()).ExecuteCommandAsync() > 0)//修改数据
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<UploadDevice>> Page(UploadDevicePageInput input)
        {
            long? pluginid = 0;
            if (!string.IsNullOrEmpty(input.PluginName))
            {
                pluginid = _driverPluginService.GetCacheListAsync().FirstOrDefault(it => it.AssembleName.Contains(input.PluginName))?.Id;
            }
            var query = Context.Queryable<UploadDevice>()
             .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
             .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginId == (pluginid ?? 0))
             .WhereIF(!string.IsNullOrEmpty(input.DeviceGroup), u => u.DeviceGroup == input.DeviceGroup)
             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
             .OrderBy(u => u.Id)//排序
             .Select((u) => new UploadDevice { Id = u.Id.SelectAll() })
                ;
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

        /// <inheritdoc/>
        public UploadDevice GetDeviceById(long Id)
        {
            var data = GetCacheList();
            return data.FirstOrDefault(it => it.Id == Id);
        }
        public List<UploadDevice> GetCacheList()
        {
            //先从Cache拿
            var collectDevice = _sysCacheService.Get<List<UploadDevice>>(ThingsGatewayCacheConst.Cache_UploadDevice, "");
            if (collectDevice == null)
            {
                collectDevice = Context.Queryable<UploadDevice>()
                .Select((u) => new UploadDevice { Id = u.Id.SelectAll() })
                .ToList();
                if (collectDevice != null)//做个大小写限制
                {
                    //插入Cache
                    _sysCacheService.Set(ThingsGatewayCacheConst.Cache_UploadDevice, "", collectDevice);
                }
            }
            return collectDevice;
        }

        /// <inheritdoc/>
        public List<UploadDeviceRunTime> GetUploadDeviceRuntime(long devId = 0)
        {
            if (devId == 0)
            {
                var devices = GetCacheList();
                var runtime = devices.Adapt<List<UploadDeviceRunTime>>();
                using var serviceScope = _scopeFactory.CreateScope();
                var variableService = serviceScope.ServiceProvider.GetService<IVariableService>();
                foreach (var device in runtime)
                {
                    var pluginName = _driverPluginService.GetNameById(device.PluginId);
                    device.PluginName = pluginName;
                }
                return runtime;
            }
            else
            {
                var devices = GetCacheList();
                devices = devices.Where(it => it.Id == devId).ToList();
                var runtime = devices.Adapt<List<UploadDeviceRunTime>>();
                using var serviceScope = _scopeFactory.CreateScope();
                var variableService = serviceScope.ServiceProvider.GetService<IVariableService>();
                foreach (var device in runtime)
                {
                    var pluginName = _driverPluginService.GetNameById(device.PluginId);
                    device.PluginName = pluginName;
                }
                return runtime;

            }

        }

        #region 导入导出
        [OperDesc("导出上传设备模板", IsRecordPar = false)]
        public async Task<MemoryStream> Template()
        {
            IImporter Importer = new ExcelImporter();
            var byteArray = await Importer.GenerateTemplateBytes<UploadDeviceWithPropertyImport>();

            var result = new MemoryStream(byteArray);
            return result;
        }

        [OperDesc("导出上传设备表", IsRecordPar = false)]
        public async Task<MemoryStream> ExportFile()
        {
            var devDatas = GetCacheList();

            var devExports = devDatas.Adapt<List<UploadDeviceExport>>();
            //需要手动改正插件名称
            devExports.ForEach(it => it.PluginName = _driverPluginService.GetNameById(it.PluginId));

            List<DevicePropertyExport> devicePropertys = new List<DevicePropertyExport>();
            foreach (var devData in devDatas)
            {
                var propertyExcels = devData.DevicePropertys.Adapt<List<DevicePropertyExport>>();
                if (propertyExcels != null)
                {
                    //需要手动改正设备名称
                    propertyExcels.ForEach(it => it.DeviceName = devData.Name);
                    devicePropertys.AddRange(propertyExcels);
                }

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
            var importDic = await Importer.ImportMultipleSheet<UploadDeviceWithPropertyImport>(fs);//导入的文件转化为带入结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            ImportPreviewOutput<UploadDeviceImport> DeviceImportPreview = new();
            foreach (var item in importDic)
            {
                //导入的Sheet数据，
                if (item.Key == "上传设备")
                {
                    //多个不同类型的Sheet返回的值为object，需要进行类型转换
                    var import = item.Value.Adapt<ImportResult<UploadDeviceImport>>();
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
                                ImportPreview.RowErrors.Add(new DataRowErrorInfo() { RowIndex = i + 2, FieldErrors = error });
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
        [OperDesc("导入上传设备表", IsRecordPar = false)]
        public async Task Import(Dictionary<string, ImportPreviewOutputBase> input)
        {
            var collectDevices = new List<UploadDevice>();
            foreach (var item in input)
            {
                if (item.Key == "上传设备")
                {
                    var collectDeviceImports = ((ImportPreviewOutput<UploadDeviceImport>)item.Value).Data;
                    collectDevices = collectDeviceImports.Adapt<List<UploadDevice>>();
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
                }
            }
            await Context.Storageable(collectDevices).ExecuteCommandAsync();
            _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_UploadDevice, "");//cache删除

        }

        #endregion
    }
}