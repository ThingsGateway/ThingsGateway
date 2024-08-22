//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;


using Mapster;

using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

using SqlSugar;

using System.Data;
using System.Reflection;

using ThingsGateway.Core;

namespace ThingsGateway;

public static class ExportExcelExtensions
{
    public static Dictionary<string, object> ExportExcel<T>(this IEnumerable<T>? data, string sheetName)
    {
        //总数据
        Dictionary<string, object> sheets = new();
        //通道页
        List<Dictionary<string, object>> valveLogExports = new();

        #region 列名称

        var type = typeof(T);
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
            Dictionary<string, object> valveLogExport = new();
            foreach (var item in propertyInfos)
            {
                //描述
                var desc = type.GetPropertyDisplayName(item.Name);
                //数据源增加
                valveLogExport.Add(desc ?? item.Name, item.GetValue(device)?.ToString());
            }

            //添加完整设备信息
            valveLogExports.Add(valveLogExport);
        }
        //添加设备页
        sheets.Add(sheetName, valveLogExports);
        return sheets;
    }


    public static async Task ExportExcel<T>(this Stream stream, object input, bool isDynamicExcelColumn) where T : class
    {
        var config = new OpenXmlConfiguration();
        if (isDynamicExcelColumn)
        {
            var type = typeof(T);
            var data = type.GetRuntimeProperties();
            List<DynamicExcelColumn> dynamicExcelColumns = new();
            int index = 0;
            foreach (var item in data)
            {
                var ignore = item.GetCustomAttribute<IgnoreExcelAttribute>() != null;
                //描述
                var desc = type.GetPropertyDisplayName(item.Name);
                //数据源增加
                dynamicExcelColumns.Add(new DynamicExcelColumn(item.Name) { Ignore = ignore, Index = index, Name = desc ?? item.Name, Width = 30 });
                if (!ignore)
                    index++;
            }
            config.DynamicColumns = dynamicExcelColumns.ToArray();
        }
        config.TableStyles = TableStyles.None;
        await MiniExcel.SaveAsAsync(stream, input, configuration: config).ConfigureAwait(false);
    }
}
