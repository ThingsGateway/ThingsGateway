#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Text;

using ThingsGateway.Admin.Core;
using ThingsGateway.Admin.Core.JsonExtensions;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 种子数据工具类
/// </summary>
public class SeedDataUtil
{
    /// <summary>
    /// json转化为种子列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="jsonName"></param>
    /// <returns></returns>
    public static List<T> GetSeedData<T>(string jsonName)
    {
        var seedData = new List<T>();//种子数据结果
        var basePath = AppContext.BaseDirectory;//获取项目目录
        var json = basePath.CombinePath("SeedData", "Json", jsonName);//获取文件路径
        var dataString = ReadFile(json);//读取文件
        if (!string.IsNullOrEmpty(dataString))//如果有内容
        {
            //字段没有数据的替换成null
            dataString = dataString.Replace("\"\"", "null");
            //将json字符串转为实体，这里extjson可以正常转换为字符串
            var seedDataRecord = dataString.ToJsonWithT<SeedDataRecords<T>>();

            //遍历seedDataRecord
            for (int i = 0; i < seedDataRecord.Records.Count; i++)
            {
                #region 处理ExtJosn

                //获取extjson属性
                var propertyExtJosn = typeof(T).GetProperty(nameof(PrimaryKeyEntity.ExtJson));
                if (propertyExtJosn != null)
                {
                    //获取extjson的值
                    var extJson = propertyExtJosn.GetValue(seedDataRecord.Records[i])?.ToString();
                    // 如果extjson不为空并且包含NullableDictionary表示序列化失败了
                    if (!string.IsNullOrEmpty(extJson) && extJson.Contains("NullableDictionary"))
                    {
                        //设置extjson为seedDataRecord对应的值
                        extJson = propertyExtJosn.GetValue(seedDataRecord.Records[i])?.ToString();
                        //seedDataRecord赋值seedDataRecord的extjson
                        propertyExtJosn.SetValue(seedDataRecord.Records[i], extJson);
                    }
                }

                #endregion 处理ExtJosn

                #region 处理ConfigValue

                //获取extjson属性
                var propertyConfigValue = typeof(T).GetProperty(nameof(SysConfig.ConfigValue));
                if (propertyConfigValue != null)
                {
                    //获取extjson的值
                    var configValue = propertyConfigValue.GetValue(seedDataRecord.Records[i])?.ToString();
                    // 如果extjson不为空并且包含NullableDictionary表示序列化失败了
                    if (!string.IsNullOrEmpty(configValue) && configValue.Contains("NullableDictionary"))
                    {
                        //设置extjson为seedDataRecord对应的值
                        configValue = propertyConfigValue.GetValue(seedDataRecord.Records[i])?.ToString();
                        //seedDataRecord赋值seedDataRecord的extjson
                        propertyConfigValue.SetValue(seedDataRecord.Records[i], configValue);
                    }
                }

                #endregion 处理ConfigValue
            }
            //种子数据赋值
            seedData = seedDataRecord.Records;
        }

        return seedData;
    }

    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public static string ReadFile(string Path)
    {
        if (!File.Exists(Path))
        {
            return null;
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        StreamReader streamReader = new(Path, Encoding.GetEncoding("utf-8"));
        string result = streamReader.ReadToEnd();
        streamReader.Close();
        streamReader.Dispose();
        return result;
    }
}

/// <summary>
/// 种子数据格式实体类,遵循Navicat导出json格式
/// </summary>
/// <typeparam name="T"></typeparam>

public class SeedDataRecords<T>
{
    /// <summary>
    /// 数据
    /// </summary>
    public List<T> Records { get; set; }
}