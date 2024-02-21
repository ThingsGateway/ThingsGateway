//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.FriendlyException;

using Mapster;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

using MiniExcelLibs;

using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;

using ThingsGateway.Cache;
using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <inheritdoc cref="IChannelService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class ChannelService : DbRepository<Channel>, IChannelService
{
    protected readonly IFileService _fileService;
    protected readonly IServiceScope _serviceScope;
    protected readonly ISimpleCacheService _simpleCacheService;
    protected readonly IImportExportService _importExportService;

    /// <inheritdoc cref="IChannelService"/>
    public ChannelService(
    IServiceScopeFactory serviceScopeFactory,
    IFileService fileService,
    ISimpleCacheService simpleCacheService,
    IImportExportService importExportService
        )
    {
        _fileService = fileService;
        _serviceScope = serviceScopeFactory.CreateScope();
        _simpleCacheService = simpleCacheService;
        _importExportService = importExportService;
    }

    [OperDesc("添加通道")]
    public async Task AddAsync(ChannelAddInput input)
    {
        var model_Id = GetIdByName(input.Name);
        if (model_Id > 0 && model_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}"); //缓存不要求全部数据，最后会由数据库约束报错
        await InsertAsync(input);//添加数据
        DeleteChannelFromRedis();
    }

    [OperDesc("复制通道", IsRecordPar = false)]
    public async Task CopyAsync(IEnumerable<Channel> input, int count)
    {
        List<Channel> newDevs = new();

        for (int i = 0; i < count; i++)
        {
            var newDev = input.Adapt<List<Channel>>();

            newDev.ForEach(a =>
            {
                a.Id = YitIdHelper.NextId();
                a.Name = $"{Regex.Replace(a.Name, @"\d", "")}{a.Id}";
            });
            newDevs.AddRange(newDev);
        }
        await Context.Fastest<Channel>().PageSize(50000).BulkCopyAsync(newDevs);
        DeleteChannelFromRedis();
    }

    [OperDesc("删除通道")]
    public async Task DeleteAsync(List<BaseIdInput> input)
    {
        var ids = input.Select(a => a.Id).ToList();

        var deviceService = _serviceScope.ServiceProvider.GetService<IDeviceService>();
        deviceService.NewContent = NewContent;
        //事务
        var result = await NewContent.UseTranAsync(async () =>
        {
            await Context.Deleteable<Channel>().Where(a => ids.Contains(a.Id)).ExecuteCommandAsync();
            await deviceService.DeleteByChannelIdAsync(input);
        });
        if (result.IsSuccess)//如果成功了
        {
            DeleteChannelFromRedis();
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    [OperDesc("清空通道")]
    public async Task ClearAsync()
    {
        var deviceService = _serviceScope.ServiceProvider.GetService<IDeviceService>();
        deviceService.NewContent = NewContent;
        //事务
        var result = await NewContent.UseTranAsync(async () =>
        {
            var data = GetCacheList();
            await Context.Deleteable<Channel>().ExecuteCommandAsync();
            await deviceService.DeleteByChannelIdAsync(data.Adapt<List<BaseIdInput>>());
        });
        if (result.IsSuccess)//如果成功了
        {
            DeleteChannelFromRedis();
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
    public void DeleteChannelFromRedis()
    {
        _simpleCacheService.Remove(ThingsGatewayCacheConst.Cache_Channel);//删除通道缓存
    }

    [OperDesc("编辑通道")]
    public async Task EditAsync(ChannelEditInput input)
    {
        var model_Id = GetIdByName(input.Name);
        if (model_Id > 0 && model_Id != input.Id)
            throw Oops.Bah($"存在重复的名称:{input.Name}");//缓存不要求全部数据，最后会由数据库约束报错

        if (await Context.Updateable(input.Adapt<Channel>()).ExecuteCommandAsync() > 0)//修改数据
            DeleteChannelFromRedis();
    }

    public Channel? GetChannelById(long id)
    {
        var data = GetCacheList();
        return data?.FirstOrDefault(x => x.Id == id);
    }

    public List<Channel> GetCacheList()
    {
        var channel = _simpleCacheService.HashGetAll<Channel>(ThingsGatewayCacheConst.Cache_Channel);
        if (channel == null || channel.Count == 0)
        {
            var data = GetList();
            _simpleCacheService.HashSet(ThingsGatewayCacheConst.Cache_Channel, data.ToDictionary(a => a.Id.ToString()));
            return data;
        }
        return channel.Values.ToList();
    }

    public long? GetIdByName(string name)
    {
        var data = GetCacheList();
        return data?.FirstOrDefault(x => x.Name == name)?.Id;
    }

    public string? GetNameById(long id)
    {
        var data = GetCacheList();
        return data?.FirstOrDefault(x => x.Id == id)?.Name;
    }

    public Task<SqlSugarPagedList<Channel>> PageAsync(ChannelPageInput input)
    {
        var query = GetPage(input);
        return query.ToPagedListAsync(input.Current, input.Size);//分页
    }

    /// <inheritdoc/>
    private ISugarQueryable<Channel> GetPage(ChannelPageInput input)
    {
        ISugarQueryable<Channel> query = Context.Queryable<Channel>()
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(input.ChannelType != null, u => u.ChannelType == input.ChannelType);
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    #region 导出

    /// <inheritdoc/>
    [OperDesc("导出通道配置", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(IDataReader? input = null)
    {
        if (input != null)
        {
            return await _importExportService.ExportAsync<Channel>(input, "Channel");
        }
        //导出
        var data = (GetCacheList())?.OrderBy(a => a.ChannelType);
        return await Export(data);
    }

    /// <inheritdoc/>
    [OperDesc("导出通道配置", IsRecordPar = false)]
    public async Task<FileStreamResult> ExportFileAsync(ChannelInput input)
    {
        var data = (await GetPage(input.Adapt<ChannelPageInput>()).ExportIgnoreColumns().ToListAsync())?.OrderBy(a => a.ChannelType);
        return await Export(data);
    }

    /// <inheritdoc/>
    [OperDesc("导出通道配置", IsRecordPar = false)]
    public async Task<MemoryStream> ExportMemoryStream(List<Channel> data)
    {
        Dictionary<string, object> sheets = ExportCore(data);
        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(sheets);
        return memoryStream;
    }

    private async Task<FileStreamResult> Export(IEnumerable<Channel>? data)
    {
        Dictionary<string, object> sheets = ExportCore(data);

        return await _importExportService.ExportAsync<Channel>(sheets, "Channel", false);
    }

    private static Dictionary<string, object> ExportCore(IEnumerable<Channel>? data)
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
               return a.GetCustomAttribute<DataTableAttribute>()?.Order ?? 999999;
           }
           );

        #endregion 列名称

        foreach (var device in data)
        {
            Dictionary<string, object> channelExport = new();
            foreach (var item in propertyInfos)
            {
                //描述
                var desc = item.FindDisplayAttribute();
                //数据源增加
                channelExport.Add(desc ?? item.Name, item.GetValue(device)?.ToString());
            }

            //添加完整设备信息
            channelExports.Add(channelExport);
        }
        //添加设备页
        sheets.Add(ExportConst.ChannelName, channelExports);
        return sheets;
    }

    #endregion 导出

    #region 导入

    /// <inheritdoc/>
    [OperDesc("导入通道表", IsRecordPar = false)]
    public async Task ImportAsync(Dictionary<string, ImportPreviewOutputBase> input)
    {
        var channels = new List<Channel>();
        foreach (var item in input)
        {
            if (item.Key == ExportConst.ChannelName)
            {
                var collectChannelImports = ((ImportPreviewOutput<Channel>)item.Value).Data;
                channels = new List<Channel>(collectChannelImports.Values);
                break;
            }
        }
        var upData = channels.Where(a => a.IsUp).ToList();
        var insertData = channels.Where(a => !a.IsUp).ToList();
        await Context.Fastest<Channel>().PageSize(100000).BulkCopyAsync(insertData);
        await Context.Fastest<Channel>().PageSize(100000).BulkUpdateAsync(upData);
        DeleteChannelFromRedis();
    }

    public async Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile)
    {
        var path = await _importExportService.UploadFileAsync(browserFile);
        try
        {
            var sheetNames = MiniExcel.GetSheetNames(path);
            var channelDicts = GetCacheList().ToDictionary(a => a.Name);
            //导入检验结果
            Dictionary<string, ImportPreviewOutputBase> ImportPreviews = new();
            //设备页
            ImportPreviewOutput<Channel> channelImportPreview = new();
            foreach (var sheetName in sheetNames)
            {
                var rows = MiniExcel.Query(path, useHeaderRow: true, sheetName: sheetName).Cast<IDictionary<string, object>>();

                #region sheet

                if (sheetName == ExportConst.ChannelName)
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
                            var channel = ((ExpandoObject)item).ConvertToEntity<Channel>(true);
                            if (channel == null)
                            {
                                importPreviewOutput.HasError = true;
                                importPreviewOutput.Results.Add((row++, false, "无法识别任何信息"));
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