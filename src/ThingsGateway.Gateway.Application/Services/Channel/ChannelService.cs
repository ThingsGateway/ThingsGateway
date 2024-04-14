
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using Mapster;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

using MiniExcelLibs;

using SqlSugar;

using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Text;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

public class ChannelService : BaseService<Channel>, IChannelService
{
    protected readonly IFileService _fileService;
    protected readonly IImportExportService _importExportService;

    /// <inheritdoc cref="IChannelService"/>
    public ChannelService(
    IFileService fileService,
    IImportExportService importExportService
        )
    {
        _fileService = fileService;
        _importExportService = importExportService;
    }

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    public Task<QueryData<Channel>> PageAsync(QueryPageOptions option)
    {
        return QueryAsync(option, a => a.WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText!)));
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
    /// 保存通道
    /// </summary>
    /// <param name="input">通道</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveChannel", localizerType: typeof(Channel))]
    public async Task<bool> SaveChannelAsync(Channel input, ItemChangedType type)
    {
        //验证
        CheckInput(input);
        if (await base.SaveAsync(input, type))
        {
            DeleteChannelFromCache();
            return true;
        }
        return false;
    }

    [OperDesc("DeleteChannel", localizerType: typeof(Channel))]
    public async Task<bool> DeleteChannelAsync(IEnumerable<long> ids)
    {
        var deviceService = App.RootServices.GetRequiredService<IDeviceService>();
        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            await db.Deleteable<Channel>().Where(a => ids.Contains(a.Id)).ExecuteCommandAsync();
            await deviceService.DeleteByChannelIdAsync(ids, db);
        });
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

    [OperDesc("ClearChannel", localizerType: typeof(Channel))]
    public async Task ClearChannelAsync()
    {
        var deviceService = App.RootServices.GetRequiredService<IDeviceService>();
        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            var data = GetAll();
            await db.Deleteable<Channel>().ExecuteCommandAsync();
            await deviceService.DeleteByChannelIdAsync(data.Select(a => a.Id), db);
        });
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

    public IChannel GetChannel(Channel channel, TouchSocketConfig config)
    {
        return config.GetChannel(channel.ChannelType, channel.RemoteUrl, channel.BindUrl, channel.Adapt<SerialPortOption>());
    }

    /// <inheritdoc />
    public void DeleteChannelFromCache()
    {
        App.CacheService.Remove(ThingsGatewayCacheConst.Cache_Channel);//删除通道缓存
    }

    public Channel? GetChannelById(long id)
    {
        var data = GetAll();
        return data?.FirstOrDefault(x => x.Id == id);
    }

    public long? GetIdByName(string name)
    {
        var data = GetAll();
        return data?.FirstOrDefault(x => x.Name == name)?.Id;
    }

    public string? GetNameById(long id)
    {
        var data = GetAll();
        return data?.FirstOrDefault(x => x.Id == id)?.Name;
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
        else if (input.ChannelType == ChannelTypeEnum.SerialPortClient)
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

    public Task<SqlSugarPagedList<Channel>> PageAsync(ChannelPageInput input)
    {
        using var db = GetDB();
        var query = GetPage(db, input);
        return query.ToPagedListAsync(input.Current, input.Size);//分页
    }

    /// <inheritdoc/>
    private ISugarQueryable<Channel> GetPage(SqlSugarClient db, ChannelPageInput input)
    {
        ISugarQueryable<Channel> query = db.Queryable<Channel>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
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
    public async Task<FileStreamResult> ExportChannelAsync(IDataReader? input = null)
    {
        if (input != null)
        {
            return await _importExportService.ExportAsync<Channel>(input, "Channel");
        }
        //导出
        var data = (GetAll())?.OrderBy(a => a.ChannelType);
        return await ExportChannel(data);
    }

    /// <inheritdoc/>
    [OperDesc("ExportChannel", isRecordPar: false, localizerType: typeof(Channel))]
    public async Task<FileStreamResult> ExportChannelAsync(QueryPageOptions options)
    {
        var data = await PageAsync(options);
        return await ExportChannel(data.Items);
    }

    /// <inheritdoc/>
    [OperDesc("ExportChannel", isRecordPar: false, localizerType: typeof(Channel))]
    public async Task<MemoryStream> ExportMemoryStream(List<Channel> data)
    {
        Dictionary<string, object> sheets = ExportChannelCore(data);
        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        return memoryStream;
    }

    private async Task<FileStreamResult> ExportChannel(IEnumerable<Channel>? data)
    {
        Dictionary<string, object> sheets = ExportChannelCore(data);

        return await _importExportService.ExportAsync<Channel>(sheets, "Channel", false);
    }

    private static Dictionary<string, object> ExportChannelCore(IEnumerable<Channel>? data)
    {
        //总数据
        Dictionary<string, object> sheets = new();
        //通道页
        List<Dictionary<string, object>> channelExports = new();

        #region 列名称

        var type = typeof(Channel);
        var propertyInfos = type.GetProperties().Where(a => a.GetCustomAttribute<IgnoreExcelAttribute>() == null).OrderBy(
           a =>
           {
               return a.GetCustomAttribute<AutoGenerateColumnAttribute>()?.Order ?? 999999;
           }
           );

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
        await db.Fastest<Channel>().PageSize(100000).BulkCopyAsync(insertData);
        await db.Fastest<Channel>().PageSize(100000).BulkUpdateAsync(upData);
        DeleteChannelFromCache();
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await _importExportService.UploadFileAsync(browserFile);
        try
        {
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

                    rows.ForEach(item =>
                    {
                        try
                        {
                            var channel = ((ExpandoObject)item!).ConvertToEntity<Channel>(true);
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
                                channel.IsUp = true;
                            }
                            else
                            {
                                channel.Id = YitIdHelper.NextId();
                                channel.IsUp = false;
                            }

                            channels.Add(channel);
                            importPreviewOutput.Results.Add((row++, true, null));
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
    public string Name { get; set; }

    /// <inheritdoc/>
    public ChannelTypeEnum? ChannelType { get; set; }
}