using Furion.FriendlyException;
using Furion.LinqBuilder;

using Microsoft.AspNetCore.Components.Forms;

using MiniExcelLibs;


using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

using ThingsGateway.Core;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <inheritdoc cref="IVariableService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class VariableService : DbRepository<CollectDeviceVariable>, IVariableService
    {
        private readonly SysCacheService _sysCacheService;
        private readonly ICollectDeviceService _collectDeviceService;
        private readonly IUploadDeviceService _uploadDeviceService;
        private readonly IDriverPluginService _driverPluginService;
        private readonly FileService _fileService;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <inheritdoc cref="IVariableService"/>
        public VariableService(SysCacheService sysCacheService,
            ICollectDeviceService collectDeviceService, FileService fileService,
            IUploadDeviceService uploadDeviceService, IDriverPluginService driverPluginService,
        IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _sysCacheService = sysCacheService;
            _fileService = fileService;
            _collectDeviceService = collectDeviceService;
            _uploadDeviceService = uploadDeviceService;
            _driverPluginService = driverPluginService;
        }

        /// <inheritdoc/>
        [OperDesc("添加变量")]
        public async Task AddAsync(CollectDeviceVariable input)
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
        public async Task DeleteAsync(List<BaseIdInput> input)
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
        public async Task ClearAsync()
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
        public async Task EditAsync(CollectDeviceVariable input)
        {
            var account_Id = GetIdByName(input.Name);
            if (account_Id > 0 && account_Id != input.Id)
                throw Oops.Bah($"存在重复的名称:{input.Name}");

            if (await Context.Updateable(input).ExecuteCommandAsync() > 0)//修改数据
                DeleteVariableFromCache(input.Id);
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<CollectDeviceVariable>> PageAsync(VariablePageInput input)
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
        public async Task<List<CollectVariableRunTime>> GetCollectDeviceVariableRuntimeAsync(long devId = 0)
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

        //查找出列表中的所有重复元素及其重复次数
        private static Dictionary<string, int> QueryRepeatElementAndCountOfList(IEnumerable<IDictionary<string, object>> list)
        {
            Dictionary<string, int> DicTmp = new Dictionary<string, int>();
            if (list != null && list.Count() > 0)
            {
                DicTmp = list.GroupBy(x => ((ExpandoObject)x).ConvertToEntity<CollectDeviceVariable>().Name)
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
        public const string CollectDeviceSheetName = "变量";
        /// <inheritdoc/>
        [OperDesc("导出变量表", IsRecordPar = false)]
        public async Task<MemoryStream> ExportFileAsync()
        {

            var devDatas = await GetListAsync();
            return await ExportFileAsync(devDatas);
        }
        /// <inheritdoc/>
        [OperDesc("导出变量表", IsRecordPar = false)]
        public async Task<MemoryStream> ExportFileAsync(List<CollectDeviceVariable> collectDeviceVariables)
        {

            var devDatas = collectDeviceVariables;

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

                //设备实体没有包含名称，手动插入
                devExport.Add("设备", _collectDeviceService.GetNameById(devData.DeviceId));
                //添加完整设备信息
                devExports.Add(devExport);


                foreach (var item in devData.VariablePropertys ?? new())
                {
                    //插件属性
                    //单个设备的行数据
                    Dictionary<string, object> driverInfo = new Dictionary<string, object>();
                    foreach (var item1 in item.Value)
                    {
                        //添加对应属性数据
                        driverInfo.Add(item1.Description, item1.Value);
                    }

                    //没有包含设备名称，手动插入
                    driverInfo.Add("变量", devData.Name);
                    var uploadDevice = _uploadDeviceService.GetDeviceById(item.Key);
                    if (uploadDevice != null)
                    {
                        driverInfo.Add("上传设备", uploadDevice?.Name);

                        //插件名称去除首部ThingsGateway.作为表名
                        var pluginName = _driverPluginService.GetNameById(uploadDevice.PluginId).Replace(PluginLeftName, "");
                        if (devicePropertys.ContainsKey(pluginName))
                        {
                            devicePropertys[pluginName].Add(driverInfo);
                        }
                        else
                        {
                            devicePropertys.Add(pluginName, new() { driverInfo });
                        }
                    }


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
            ImportPreviewOutput<CollectDeviceVariable> deviceImportPreview = new();
            foreach (var sheetName in sheetNames)
            {
                //单页数据
                var rows = fs.Query(useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();
                if (sheetName == CollectDeviceSheetName)
                {
                    ImportPreviewOutput<CollectDeviceVariable> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);
                    deviceImportPreview = importPreviewOutput;

                    var DicTmp = QueryRepeatElementAndCountOfList(rows);
                    if (DicTmp.Count > 0)
                    {
                        throw new Exception("发现重复名称" + Environment.NewLine + DicTmp.Select(a => a.Key).ToJson());
                    }
                    List<CollectDeviceVariable> devices = new List<CollectDeviceVariable>();
                    foreach (var item in rows)
                    {
                        var device = ((ExpandoObject)item).ConvertToEntity<CollectDeviceVariable>();
                        devices.Add(device);
                        //转化设备名称
                        var value = item.FirstOrDefault(a => a.Key == "设备");
                        if (_collectDeviceService.GetIdByName(value.Value.ToString()) == null)
                        {
                            //找不到对应的设备
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.ErrorStr = "设备名称不存在";
                            return ImportPreviews;
                        }
                        else
                        {
                            //插件ID和设备ID都需要手动补录
                            device.DeviceId = _collectDeviceService.GetIdByName(value.Value.ToString()).ToLong();
                            device.Id = this.GetIdByName(device.Name) == 0 ? YitIdHelper.NextId() : this.GetIdByName(device.Name);
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
                    var driver = (UpLoadBase)pluginSingletonService.GetDriver(YitIdHelper.NextId(), driverPlugin);
                    var propertys = driver.VariablePropertys.GetType().GetAllProps().Where(a => a.GetCustomAttribute<VariablePropertyAttribute>() != null);
                    foreach (var item in rows)
                    {

                        List<DependencyProperty> devices = new List<DependencyProperty>();
                        foreach (var item1 in item)
                        {
                            var propertyInfo = propertys.FirstOrDefault(p => p.FindDisplayAttribute(a => a.GetCustomAttribute<VariablePropertyAttribute>()?.Name) == item1.Key);
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
                        var variableName = item.FirstOrDefault(a => a.Key == "变量").Value?.ToString();
                        var uploadDevName = item.FirstOrDefault(a => a.Key == "上传设备").Value?.ToString();

                        var uploadDevice = _uploadDeviceService.GetCacheList().FirstOrDefault(a => a.Name == uploadDevName);

                        if (deviceImportPreview.Data?.Any(it => it.Name == variableName) == true)
                        {
                            var id = this.GetIdByName(variableName);
                            var deviceId = id != 0 ? id : deviceImportPreview.Data.FirstOrDefault(it => it.Name == variableName).Id;
                            deviceImportPreview.Data.FirstOrDefault(a => a.Id == deviceId).VariablePropertys.AddOrUpdate(uploadDevice.Id, devices);
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
            var collectDevices = new List<CollectDeviceVariable>();
            foreach (var item in input)
            {
                if (item.Key == CollectDeviceSheetName)
                {
                    var collectDeviceImports = ((ImportPreviewOutput<CollectDeviceVariable>)item.Value).Data;
                    collectDevices = collectDeviceImports.Adapt<List<CollectDeviceVariable>>();
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
                var x = await Context.Storageable(collectDevices).ToStorageAsync();
                await x.BulkCopyAsync();//不存在插入
                await x.BulkUpdateAsync();//存在更新
            }
            else
            {
                //其他数据库使用普通插入/更新
                await Context.Storageable(collectDevices).ExecuteCommandAsync();
            }
            DeleteVariableFromCache();

        }

        #endregion

    }
}