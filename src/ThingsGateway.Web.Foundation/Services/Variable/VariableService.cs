using Furion.FriendlyException;
using Furion.LinqBuilder;

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
    /// <inheritdoc cref="IVariableService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class VariableService : DbRepository<CollectDeviceVariable>, IVariableService
    {
        private readonly SysCacheService _sysCacheService;
        private readonly ICollectDeviceService _collectDeviceService;
        private readonly IUploadDeviceService _uploadDeviceService;
        private readonly FileService _fileService;

        /// <inheritdoc cref="IVariableService"/>
        public VariableService(SysCacheService sysCacheService,
            ICollectDeviceService collectDeviceService, FileService fileService,
            IUploadDeviceService uploadDeviceService
            )
        {
            _sysCacheService = sysCacheService;
            _fileService = fileService;
            _collectDeviceService = collectDeviceService;
            _uploadDeviceService = uploadDeviceService;
        }

        /// <inheritdoc/>
        [OperDesc("添加变量")]
        public async Task Add(CollectDeviceVariable input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0)
                throw Oops.Bah($"存在重复的名称:{input.Name}");
            var result = await InsertReturnEntityAsync(input);//添加数据
        }

        /// <inheritdoc/>
        public long GetIdByName(string name)
        {
            //先从Cache拿
            var id = _sysCacheService.Get<long>(ThingsGatewayCacheConst.Cache_DeviceVariableName, name);
            if (id == 0)
            {
                //单查获取对应ID
                id = Context.Queryable<CollectDeviceVariable>().Where(it => it.Name == name).Select(it => it.Id).First();
                if (id != 0)
                {
                    //插入Cache
                    _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableName, name, id);
                }
            }
            return id;
        }
        /// <inheritdoc/>
        public string GetNameById(long Id)
        {
            //先从Cache拿
            var name = _sysCacheService.Get<string>(ThingsGatewayCacheConst.Cache_DeviceVariableId, Id.ToString());
            if (name.IsNullOrEmpty())
            {
                //单查获取用户账号对应ID
                name = Context.Queryable<CollectDeviceVariable>().Where(it => it.Id == Id).Select(it => it.Name).First();
                if (!name.IsNullOrEmpty())
                {
                    //插入Cache
                    _sysCacheService.Set(ThingsGatewayCacheConst.Cache_DeviceVariableId, Id.ToString(), name);
                }
            }
            return name;
        }


        /// <inheritdoc/>
        [OperDesc("删除变量")]
        public async Task Delete(List<BaseIdInput> input)
        {
            //获取所有ID
            var ids = input.Select(it => it.Id).ToList();
            if (ids.Count > 0)
            {
                var result = await DeleteByIdsAsync(ids.Cast<object>().ToArray());
                if (result)
                {
                    DeleteVariableFromCache(ids);
                }
            }
        }

        /// <inheritdoc/>
        [OperDesc("清空变量")]
        public async Task Clear()
        {
            var result = await Context.Deleteable<CollectDeviceVariable>().ExecuteCommandAsync() > 0;
            if (result)
            {
                DeleteVariableFromCache(null);
            }
        }

        /// <inheritdoc />
        public void DeleteVariableFromCache(long id)
        {
            DeleteVariableFromCache(new List<long> { id });
        }

        /// <inheritdoc />
        public void DeleteVariableFromCache(List<long> ids = null)
        {
            _sysCacheService.RemoveByPrefixKey(ThingsGatewayCacheConst.Cache_DeviceVariableGroup);
            if (ids == null)
            {
                _sysCacheService.RemoveByPrefixKey(ThingsGatewayCacheConst.Cache_DeviceVariableId);
                _sysCacheService.RemoveByPrefixKey(ThingsGatewayCacheConst.Cache_DeviceVariableName);
                return;
            }
            var variableIds = ids.Select(it => it.ToString()).ToArray();//id转string列表
            foreach (var item in variableIds)
            {
                string name = _sysCacheService.Get<string>(ThingsGatewayCacheConst.Cache_DeviceVariableId, item);
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_DeviceVariableId, item);
                _sysCacheService.Remove(ThingsGatewayCacheConst.Cache_DeviceVariableName, name);
            }
        }

        /// <inheritdoc/>
        [OperDesc("编辑变量")]
        public async Task Edit(CollectDeviceVariable input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0 && account_Id != input.Id)
                throw Oops.Bah($"存在重复的名称:{input.Name}");

            if (await Context.Updateable(input).ExecuteCommandAsync() > 0)//修改数据
                DeleteVariableFromCache(input.Id);
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<CollectDeviceVariable>> Page(VariablePageInput input)
        {
            long? devid = 0;
            if (!string.IsNullOrEmpty(input.DeviceName))
            {
                devid = _collectDeviceService.GetIdByName(input.DeviceName);
            }
            var query = Context.Queryable<CollectDeviceVariable>()
             .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
             .WhereIF(!string.IsNullOrEmpty(input.DeviceName), u => u.DeviceId == (devid ?? 0))
             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
             .OrderBy(u => u.Id)//排序
             .Select((u) => new CollectDeviceVariable { Id = u.Id.SelectAll() })
                ;
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }


        /// <inheritdoc/>
        public async Task<List<CollectVariableRunTime>> GetCollectDeviceVariableRuntime(long devId = 0)
        {
            if (devId == 0)
            {
                var deviceVariables = await GetListAsync();
                var runtime = deviceVariables.Adapt<List<CollectVariableRunTime>>();
                foreach (var device in runtime)
                {
                    var deviceName = _collectDeviceService.GetNameById(device.DeviceId);
                    device.DeviceName = deviceName;
                }
                return runtime;
            }
            else
            {
                var deviceVariables = await GetListAsync(a => a.DeviceId == devId);
                var runtime = deviceVariables.Adapt<List<CollectVariableRunTime>>();
                foreach (var device in runtime)
                {
                    var deviceName = _collectDeviceService.GetNameById(device.DeviceId);
                    device.DeviceName = deviceName;
                }
                return runtime;
            }

        }




        #region 导入导出
        /// <inheritdoc/>
        [OperDesc("导出变量模板", IsRecordPar = false)]
        public async Task<MemoryStream> Template()
        {
            IImporter Importer = new ExcelImporter();
            var byteArray = await Importer.GenerateTemplateBytes<CollectDeviceVariableWithPropertyImport>();

            var result = new MemoryStream(byteArray);
            return result;
        }


        /// <inheritdoc/>
        [OperDesc("导出变量表", IsRecordPar = false)]
        public async Task<MemoryStream> ExportFile()
        {
            var devDatas = await GetListAsync();

            var devExports = devDatas.Adapt<List<CollectDeviceVariableExport>>();
            //需要手动改正设备名称
            devExports.ForEach(it => it.DeviceName = _collectDeviceService.GetNameById(it.DeviceId));
            List<VariablePropertyExport> devicePropertys = new List<VariablePropertyExport>();
            foreach (var devData in devDatas)
            {
                var propertyExcels = devData.VariablePropertys.Adapt<Dictionary<long, List<VariablePropertyExport>>>();
                if (propertyExcels != null)
                {
                    //需要手动改正设备名称
                    foreach (var property in propertyExcels)
                    {
                        var upDevName = _uploadDeviceService.GetNameById(property.Key);
                        property.Value.ForEach(it => it.DeviceName = upDevName);
                        property.Value.ForEach(it => it.VariableName = devData.Name);
                        devicePropertys.AddRange(property.Value);
                    }
                }


            }

            var exporter = new ExcelExporter();
            if (devExports.Count > 0)
            {
                exporter = exporter.Append(devExports);
                if (devicePropertys.Count > 0)
                {
                    exporter = exporter.SeparateBySheet()
        .Append(devicePropertys);
                }
                else
                {
                    devicePropertys.Add(new());
                    exporter = exporter.SeparateBySheet()
.Append(devicePropertys);
                }

                var byteArray = await exporter.ExportAppendDataAsByteArray();
                var result = new MemoryStream(byteArray);

                return result;

            }
            else
            {
                throw new("没有任何数据可导出");
            }


        }

        /// <inheritdoc/>
        [OperDesc("导出变量表", IsRecordPar = false)]
        public async Task<MemoryStream> ExportFile(List<CollectDeviceVariable> collectDeviceVariables)
        {
            var devDatas = collectDeviceVariables;

            var devExports = devDatas.Adapt<List<CollectDeviceVariableExport>>();
            //需要手动改正设备名称
            devExports.ForEach(it => it.DeviceName = _collectDeviceService.GetNameById(it.DeviceId));
            List<VariablePropertyExport> devicePropertys = new List<VariablePropertyExport>();
            foreach (var devData in devDatas)
            {
                var propertyExcels = devData.VariablePropertys.Adapt<Dictionary<long, List<VariablePropertyExport>>>();
                if (propertyExcels != null)
                {
                    //需要手动改正设备名称
                    foreach (var property in propertyExcels)
                    {
                        var upDevName = _uploadDeviceService.GetNameById(property.Key);
                        property.Value.ForEach(it => it.DeviceName = upDevName);
                        property.Value.ForEach(it => it.VariableName = devData.Name);
                        devicePropertys.AddRange(property.Value);
                    }

                }


            }
            var exporter = new ExcelExporter();
            if (devExports.Count > 0)
            {
                exporter = exporter.Append(devExports);
                if (devicePropertys.Count > 0)
                {
                    exporter = exporter.SeparateBySheet()
        .Append(devicePropertys);
                }
                else
                {
                    devicePropertys.Add(new());
                    exporter = exporter.SeparateBySheet()
  .Append(devicePropertys);
                }

                var byteArray = await exporter.ExportAppendDataAsByteArray();
                var result = new MemoryStream(byteArray);
                return result;
            }
            else
            {
                throw new("没有任何数据可导出");
            }

        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, ImportPreviewOutputBase>> Preview(IBrowserFile file)
        {
            _fileService.ImportVerification(file);
            var Importer = new ExcelImporter();
            using var fs = new MemoryStream();
            using var stream = file.OpenReadStream(5120000);
            await stream.CopyToAsync(fs);
            var importDic = await Importer.ImportMultipleSheet<CollectDeviceVariableWithPropertyImport>(fs);//导入的文件转化为带入结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            ImportPreviewOutput<CollectDeviceVariableImport> DeviceImportPreview = new();
            foreach (var item in importDic)
            {
                //导入的Sheet数据，
                if (item.Key == "变量")
                {
                    //多个不同类型的Sheet返回的值为object，需要进行类型转换
                    var import = item.Value.Adapt<ImportResult<CollectDeviceVariableImport>>();
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
                            if (_collectDeviceService.GetIdByName(data.DeviceName) == null)
                            {
                                //找不到对应的插件
                                data.HasError = true;
                                error.TryAdd(data.Description(it => it.DeviceName), "不存在这个设备");
                                DeviceImportPreview.HasError = true;
                                DeviceImportPreview.RowErrors.Add(new DataRowErrorInfo() { RowIndex = i, FieldErrors = error });
                            }
                            else
                            {
                                data.DeviceId = _collectDeviceService.GetIdByName(data.DeviceName).ToLong();
                                var id = this.GetIdByName(data.Name);
                                data.Id = id == 0 ? YitIdHelper.NextId() : id;
                            }
                        }

                        data.ErrorInfo = error;

                    }

                    ImportPreviews.Add(item.Key, DeviceImportPreview);
                }
                if (item.Key == "变量上传属性")
                {
                    //多个不同类型的Sheet返回的值为object，需要进行类型转换
                    var import = item.Value.Adapt<ImportResult<VariablePropertyImport>>();
                    ImportPreviewOutput<VariablePropertyImport> ImportPreview = _fileService.TemplateDataVerification(import);//验证数据完整度
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
                            data.VariableId = this.GetIdByName(data.VariableName);
                            if (data.VariableId == 0 && !DeviceImportPreview.Data?.Any(it => it.Name == data.VariableName && it.HasError == false) == true)
                            {
                                //找不到对应的变量
                                data.HasError = true;
                                error.TryAdd(data.Description(it => it.VariableName), "不存在这个变量");
                                ImportPreview.HasError = true;
                                ImportPreview.RowErrors.Add(new DataRowErrorInfo() { RowIndex = i + 1, FieldErrors = error });
                            }
                            else
                            {
                                data.VariableId = data.VariableId == 0 ? DeviceImportPreview.Data.FirstOrDefault(it => it.Name == data.VariableName).Id : data.VariableId;
                            }

                            var devId = _uploadDeviceService.GetIdByName(data.DeviceName);
                            if (devId == null)
                            {
                                //找不到对应的设备
                                data.HasError = true;
                                error.TryAdd(data.Description(it => it.DeviceName), "不存在这个设备");
                                ImportPreview.HasError = true;
                                ImportPreview.RowErrors.Add(new DataRowErrorInfo() { RowIndex = i + 1, FieldErrors = error });
                            }
                            else
                            {
                                data.DeviceId = devId ?? 0;
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
        [OperDesc("导入变量表", IsRecordPar = false)]
        public async Task Import(Dictionary<string, ImportPreviewOutputBase> input)
        {
            var collectVariables = new List<CollectDeviceVariable>();
            foreach (var item in input)
            {
                if (item.Key == "变量")
                {
                    var collectDeviceImports = ((ImportPreviewOutput<CollectDeviceVariableImport>)item.Value).Data;
                    collectVariables = collectDeviceImports.Adapt<List<CollectDeviceVariable>>();
                    break;
                }
            }
            foreach (var item in input)
            {
                if (item.Key == "变量上传属性")
                {
                    var propertys = ((ImportPreviewOutput<VariablePropertyImport>)item.Value).Data;
                    foreach (var collectVariable in collectVariables.Where(a => propertys.Select(b => b.VariableId).ToList().Contains(a.Id)).ToList())
                    {
                        collectVariable.VariablePropertys = propertys.Where(it => it.VariableId == collectVariable.Id).GroupBy(a => a.DeviceId).ToDictionary(a => a.Key).Adapt<Dictionary<long, List<DependencyProperty>>>();
                    }
                    break;
                }
            }
            if (Context.CurrentConnectionConfig.DbType == DbType.Sqlite
                || Context.CurrentConnectionConfig.DbType == DbType.SqlServer
                || Context.CurrentConnectionConfig.DbType == DbType.MySql
                || Context.CurrentConnectionConfig.DbType == DbType.PostgreSQL
                )
            {
                //大量数据插入/更新
                var x = await Context.Storageable(collectVariables).ToStorageAsync();
                await x.BulkCopyAsync();//不存在插入
                await x.BulkUpdateAsync();//存在更新
            }
            else
            {
                //其他数据库使用普通插入/更新
                await Context.Storageable(collectVariables).ExecuteCommandAsync();
            }
            DeleteVariableFromCache();

        }

        #endregion

    }
}