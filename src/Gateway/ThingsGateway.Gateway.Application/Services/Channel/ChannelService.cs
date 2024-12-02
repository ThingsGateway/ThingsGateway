//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using MiniExcelLibs;

using SqlSugar;

using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Text;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation.Extension.Dynamic;
using ThingsGateway.FriendlyException;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

internal sealed class ChannelService : BaseService<Channel>, IChannelService
{
    private readonly IDispatchService<Channel> _dispatchService;
    private ISysUserService _sysUserService;
    private ISysUserService SysUserService
    {
        get
        {
            if (_sysUserService == null)
            {
                _sysUserService = App.GetService<ISysUserService>();
            }
            return _sysUserService;
        }
    }
    /// <inheritdoc cref="IChannelService"/>
    public ChannelService(
    IDispatchService<Channel>? dispatchService
        )
    {
        _dispatchService = dispatchService;
    }

    /// <inheritdoc/>
    [OperDesc("SaveChannel", localizerType: typeof(Channel), isRecordPar: false)]
    public async Task<bool> BatchEditAsync(IEnumerable<Channel> models, Channel oldModel, Channel model)
    {
        var differences = models.GetDiffProperty(oldModel, model);
        if (differences?.Count > 0)
        {
            using var db = GetDB();

            var result = (await db.Updateable(models.ToList()).UpdateColumns(differences.Select(a => a.Key).ToArray()).ExecuteCommandAsync().ConfigureAwait(false)) > 0;
            if (result)
            {
                DeleteChannelFromCache();
            }
            return result;
        }
        else
        {
            return true;
        }
    }

    [OperDesc("ClearChannel", localizerType: typeof(Channel))]
    public async Task ClearChannelAsync()
    {
        var deviceService = App.RootServices.GetRequiredService<IDeviceService>();
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);

        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            var data = GetAll()
            .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
            .WhereIf(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId).Select(a => a.Id).ToList();
            await db.Deleteable<Channel>(data).ExecuteCommandAsync().ConfigureAwait(false);
            await deviceService.DeleteByChannelIdAsync(data, db).ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            DeleteChannelFromCache();
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    [OperDesc("DeleteChannel", localizerType: typeof(Channel))]
    public async Task<bool> DeleteChannelAsync(IEnumerable<long> ids)
    {
        var deviceService = App.RootServices.GetRequiredService<IDeviceService>();

        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            await db.Deleteable<Channel>().Where(a => ids.Contains(a.Id)).ExecuteCommandAsync().ConfigureAwait(false);
            await deviceService.DeleteByChannelIdAsync(ids, db).ConfigureAwait(false);
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            DeleteChannelFromCache();
            return true;
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc />
    public void DeleteChannelFromCache()
    {
        App.CacheService.Remove(ThingsGatewayCacheConst.Cache_Channel);//删除通道缓存
        _dispatchService.Dispatch(new());
    }

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public List<Channel> GetAll()
    {
        var key = ThingsGatewayCacheConst.Cache_Channel;
        var channels = App.CacheService.Get<List<Channel>>(key);
        if (channels == null)
        {
            using var db = GetDB();
            channels = db.Queryable<Channel>().ToList();
            App.CacheService.Set(key, channels);
        }
        return channels;
    }

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public async Task<List<Channel>> GetAllByOrgAsync()
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return GetAll()
              .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.CreateOrgId))
              .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId).ToList();
    }

    public Channel? GetChannelById(long id)
    {
        var data = GetAll();
        return data?.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="filterKeyValueAction">查询条件</param>
    public async Task<QueryData<Channel>> PageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null)
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return await QueryAsync(option, a => a
        .WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText!))
         .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
         .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
       , filterKeyValueAction).ConfigureAwait(false);
    }

    /// <summary>
    /// 保存通道
    /// </summary>
    /// <param name="input">通道</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveChannel", localizerType: typeof(Channel))]
    public async Task<bool> SaveChannelAsync(Channel input, ItemChangedType type)
    {

        //验证
        CheckInput(input);

        if (type == ItemChangedType.Update)
            await SysUserService.CheckApiDataScopeAsync(input.CreateOrgId, input.CreateUserId).ConfigureAwait(false);

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {
            DeleteChannelFromCache();
            return true;
        }
        return false;
    }

    private void CheckInput(Channel input)
    {

        if (input.ChannelType == ChannelTypeEnum.TcpClient)
        {
            if (string.IsNullOrEmpty(input.RemoteUrl))
                throw Oops.Bah(Localizer["RemoteUrlNotNull"]);
        }
        else if (input.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (string.IsNullOrEmpty(input.BindUrl))
                throw Oops.Bah(Localizer["BindUrlNotNull"]);
        }
        else if (input.ChannelType == ChannelTypeEnum.UdpSession)
        {
            if (string.IsNullOrEmpty(input.BindUrl) && string.IsNullOrEmpty(input.RemoteUrl))
                throw Oops.Bah(Localizer["BindUrlOrRemoteUrlNotNull"]);
        }
        else if (input.ChannelType == ChannelTypeEnum.SerialPort)
        {
            if (string.IsNullOrEmpty(input.PortName))
                throw Oops.Bah(Localizer["PortNameNotNull"]);
            if (input.BaudRate == null)
                throw Oops.Bah(Localizer["BaudRateNotNull"]);
            if (input.DataBits == null)
                throw Oops.Bah(Localizer["DataBitsNotNull"]);
            if (input.Parity == null)
                throw Oops.Bah(Localizer["ParityNotNull"]);
            if (input.StopBits == null)
                throw Oops.Bah(Localizer["StopBitsNotNull"]);
        }
    }

    #region API查询

    public async Task<SqlSugarPagedList<Channel>> PageAsync(ChannelPageInput input)
    {
        using var db = GetDB();
        var query = await GetPageAsync(db, input).ConfigureAwait(false);
        return await query.ToPagedListAsync(input.Current, input.Size).ConfigureAwait(false);//分页
    }

    /// <inheritdoc/>
    private async Task<ISugarQueryable<Channel>> GetPageAsync(SqlSugarClient db, ChannelPageInput input)
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        ISugarQueryable<Channel> query = db.Queryable<Channel>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
                .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
                .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
         .WhereIF(input.ChannelType != null, u => u.ChannelType == input.ChannelType);
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    #endregion API查询

    #region 导出

    /// <inheritdoc/>
    [OperDesc("ExportChannel", isRecordPar: false, localizerType: typeof(Channel))]
    public async Task<Dictionary<string, object>> ExportChannelAsync(QueryPageOptions options, FilterKeyValueAction filterKeyValueAction = null)
    {
        var data = await PageAsync(options, filterKeyValueAction).ConfigureAwait(false);
        return ExportChannelCore(data.Items);
    }

    /// <inheritdoc/>
    [OperDesc("ExportChannel", isRecordPar: false, localizerType: typeof(Channel))]
    public async Task<MemoryStream> ExportMemoryStream(List<Channel> data)
    {
        Dictionary<string, object> sheets = ExportChannelCore(data);
        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets).ConfigureAwait(false);
        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    private static Dictionary<string, object> ExportChannelCore(IEnumerable<Channel>? data)
    {
        //总数据
        Dictionary<string, object> sheets = new();
        //通道页
        List<Dictionary<string, object>> channelExports = new();

        #region 列名称

        var type = typeof(Channel);
        var propertyInfos = type.GetRuntimeProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null)
             .OrderBy(
            a =>
            {
                var order = a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? int.MaxValue; ;
                if (order < 0)
                {
                    order = order + 10000000;
                }
                else if (order == 0)
                {
                    order = 10000000;
                }
                return order;
            }
            )
            ;

        #endregion 列名称

        foreach (var device in data)
        {
            Dictionary<string, object> channelExport = new();
            foreach (var item in propertyInfos)
            {
                //描述
                var desc = type.GetPropertyDisplayName(item.Name);
                //数据源增加
                channelExport.Add(desc ?? item.Name, item.GetValue(device)?.ToString());
            }

            //添加完整设备信息
            channelExports.Add(channelExport);
        }
        //添加设备页
        sheets.Add(ExportString.ChannelName, channelExports);
        return sheets;
    }


    #endregion 导出

    #region 导入

    /// <inheritdoc/>
    [OperDesc("ImportChannel", isRecordPar: false, localizerType: typeof(Channel))]
    public async Task ImportChannelAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var channels = new List<Channel>();
        foreach (var item in input)
        {
            if (item.Key == ExportString.ChannelName)
            {
                var collectChannelImports = ((ImportPreviewOutput<Channel>)item.Value).Data;
                channels = new List<Channel>(collectChannelImports.Values);
                break;
            }
        }
        var upData = channels.Where(a => a.IsUp).ToList();
        var insertData = channels.Where(a => !a.IsUp).ToList();
        using var db = GetDB();
        await db.Fastest<Channel>().PageSize(100000).BulkCopyAsync(insertData).ConfigureAwait(false);
        await db.Fastest<Channel>().PageSize(100000).BulkUpdateAsync(upData).ConfigureAwait(false);
        DeleteChannelFromCache();
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await browserFile.StorageLocal().ConfigureAwait(false);
        try
        {
            var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);

            var sheetNames = MiniExcel.GetSheetNames(path);
            var channelDicts = GetAll().ToDictionary(a => a.Name);
            //导入检验结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            //设备页
            ImportPreviewOutput<Channel> channelImportPreview = new();
            foreach (var sheetName in sheetNames)
            {
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();

                #region sheet

                if (sheetName == ExportString.ChannelName)
                {
                    int row = 1;
                    ImportPreviewOutput<Channel> importPreviewOutput = new();
                    ImportPreviews.Add(sheetName, importPreviewOutput);
                    channelImportPreview = importPreviewOutput;
                    List<Channel> channels = new();
                    var type = typeof(Channel);
                    // 获取目标类型的所有属性，并根据是否需要过滤 IgnoreExcelAttribute 进行筛选
                    var channelProperties = type.GetRuntimeProperties().Where(a => (a.GetCustomAttribute<IgnoreExcelAttribute>() == null) && a.CanWrite)
                                                .ToDictionary(a => type.GetPropertyDisplayName(a.Name));

                    rows.ForEach(item =>
                    {
                        try
                        {
                            var channel = ((ExpandoObject)item!).ConvertToEntity<Channel>(channelProperties);
                            if (channel == null)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, Localizer["ImportNullError"]));
                                return;
                            }

                            // 进行对象属性的验证
                            var validationContext = new ValidationContext(channel);
                            var validationResults = new List<ValidationResult>();
                            validationContext.ValidateProperty(validationResults);

                            // 构建验证结果的错误信息
                            StringBuilder stringBuilder = new();
                            foreach (var validationResult in validationResults.Where(v => !string.IsNullOrEmpty(v.ErrorMessage)))
                            {
                                foreach (var memberName in validationResult.MemberNames)
                                {
                                    stringBuilder.Append(validationResult.ErrorMessage!);
                                }
                            }

                            // 如果有验证错误，则添加错误信息到导入预览结果并返回
                            if (stringBuilder.Length > 0)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, stringBuilder.ToString()));
                                return;
                            }

                            if (channelDicts.TryGetValue(channel.Name, out var collectChannel))
                            {
                                channel.Id = collectChannel.Id;
                                channel.CreateOrgId = collectChannel.CreateOrgId;
                                channel.CreateUserId = collectChannel.CreateUserId;
                                channel.IsUp = true;
                            }
                            else
                            {
                                channel.Id = CommonUtils.GetSingleId();
                                channel.IsUp = false;
                                channel.CreateOrgId = UserManager.OrgId;
                                channel.CreateUserId = UserManager.UserId;
                            }

                            if (channel.IsUp && ((dataScope != null && dataScope?.Count > 0 && !dataScope.Contains(channel.CreateOrgId)) || dataScope?.Count == 0 && channel.CreateUserId != UserManager.UserId))
                            {
                                importPreviewOutput.Results.Add((row++, false, "Operation not permitted"));
                            }
                            else
                            {
                                channels.Add(channel);
                                importPreviewOutput.Results.Add((row++, true, null));
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            importPreviewOutput.HasError = true;
                            importPreviewOutput.Results.Add((row++, false, ex.Message));
                            return;
                        }
                    });
                    importPreviewOutput.Data = channels.ToDictionary(a => a.Name);
                }

                #endregion sheet
            }

            return ImportPreviews;
        }
        finally
        {
            FileUtility.Delete(path);
        }
    }

    #endregion 导入
}

public class ChannelPageInput : BasePageInput
{
    /// <inheritdoc/>
    public ChannelTypeEnum? ChannelType { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; }
}
