//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

using System.Globalization;
using System.Text.RegularExpressions;

using ThingsGateway.Core.Json.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，额外实现脚本切换实体
/// </summary>
public abstract partial class BusinessBaseWithCacheIntervalScript<VarModel, DevModel, AlarmModel> : BusinessBaseWithCacheIntervalAlarmModel<VarModel, DevModel, AlarmModel> where DevModel : class where VarModel : class where AlarmModel : class
{
    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _businessPropertyWithCacheIntervalScript;

    protected abstract BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript { get; }

    public virtual List<string> Match(string input)
    {
        // 生成缓存键，以确保缓存的唯一性
        var cacheKey = $"{nameof(BusinessBaseWithCacheIntervalScript<VarModel, DevModel, AlarmModel>)}-{CultureInfo.CurrentUICulture.Name}-Match-{input}";

        // 尝试从缓存中获取匹配结果，如果缓存中不存在则创建新的匹配结果
        var strings = App.CacheService.GetOrAdd(cacheKey, entry =>
        {
            List<string> strings = new List<string>();

            // 使用正则表达式查找输入字符串中的所有匹配项
            Regex regex = new Regex(@"\$\{(.+?)\}");
            MatchCollection matches = regex.Matches(input);

            // 遍历匹配结果，将匹配到的字符串添加到列表中
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    string productKey = match.Groups[1].Value;
                    strings.Add(productKey);
                }
            }

            // 返回匹配结果列表
            return strings;
        });

        // 返回匹配结果列表
        return strings;
    }

    #region 封装方法
    protected List<TopicArray> GetAlarmTopicArrays(IEnumerable<AlarmModel> item)
    {
        IEnumerable<dynamic>? data = Application.DynamicModelExtension.GetDynamicModel<AlarmModel>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel);
        List<TopicArray> topicJsonList = new List<TopicArray>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.AlarmTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = data.GroupByKeys(topics);

                foreach (var group in groups)
                {
                    // 上传主题
                    // 获取预定义的报警主题
                    string topic = _businessPropertyWithCacheIntervalScript.AlarmTopic;

                    // 将主题中的占位符替换为分组键对应的值
                    for (int i = 0; i < topics.Count; i++)
                    {
                        topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                    }

                    // 上传内容
                    if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
                    {
                        var json = Serialize(group.Select(a => a).ToList().ToList());
                        // 将主题和 JSON 内容添加到列表中
                        topicJsonList.Add(new(topic, json));
                    }
                    else
                    {
                        // 如果不是报警列表，则将每个分组元素分别转换为 JSON 字符串
                        foreach (var gro in group)
                        {
                            var json = Serialize(gro);
                            // 将主题和 JSON 内容添加到列表中
                            topicJsonList.Add(new(topic, json));
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
            {
                var json = Serialize(data.Select(a => a).ToList());
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    var json = Serialize(group);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json));
                }
            }
        }

        return topicJsonList;
    }

    protected static ArraySegment<byte> Serialize(object value)
    {
        var block = new ValueByteBlock(1024 * 64);
        try
        {
            //将数据序列化到内存块
            FastBinaryFormatter.Serialize(ref block, value);
            block.SeekToStart();
            return block.Memory.GetArray();
        }
        finally
        {
            block.Dispose();
        }
    }

    protected List<TopicJson> GetAlarms(IEnumerable<AlarmModel> item)
    {
        IEnumerable<dynamic>? data = Application.DynamicModelExtension.GetDynamicModel<AlarmModel>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel);
        List<TopicJson> topicJsonList = new List<TopicJson>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.AlarmTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = data.GroupByKeys(topics);

                foreach (var group in groups)
                {
                    // 上传主题
                    // 获取预定义的报警主题
                    string topic = _businessPropertyWithCacheIntervalScript.AlarmTopic;

                    // 将主题中的占位符替换为分组键对应的值
                    for (int i = 0; i < topics.Count; i++)
                    {
                        topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                    }

                    // 上传内容
                    if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
                    {
                        // 如果是报警列表，则将整个分组转换为 JSON 字符串
                        string json = group.Select(a => a).ToList().ToJsonNetString();
                        // 将主题和 JSON 内容添加到列表中
                        topicJsonList.Add(new(topic, json));
                    }
                    else
                    {
                        // 如果不是报警列表，则将每个分组元素分别转换为 JSON 字符串
                        foreach (var gro in group)
                        {
                            string json = JsonExtensions.ToJsonNetString(gro);
                            // 将主题和 JSON 内容添加到列表中
                            topicJsonList.Add(new(topic, json));
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
            {
                string json = data.Select(a => a).ToList().ToJsonNetString();
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    string json = JsonExtensions.ToJsonNetString(group);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json));
                }
            }
        }

        return topicJsonList;
    }

    protected List<TopicJson> GetDeviceData(IEnumerable<DevModel> item)
    {
        IEnumerable<dynamic>? data = Application.DynamicModelExtension.GetDynamicModel<DevModel>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel);
        List<TopicJson> topicJsonList = new List<TopicJson>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.DeviceTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = data.GroupByKeys(topics).ToList();
                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        // 上传主题
                        // 获取预定义的设备主题
                        string topic = _businessPropertyWithCacheIntervalScript.DeviceTopic;

                        // 将主题中的占位符替换为分组键对应的值
                        for (int i = 0; i < topics.Count; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                        }

                        // 上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsDeviceList)
                        {
                            // 如果是设备列表，则将整个分组转换为 JSON 字符串
                            string json = group.Select(a => a).ToList().ToJsonNetString();
                            // 将主题和 JSON 内容添加到列表中
                            topicJsonList.Add(new(topic, json));
                        }
                        else
                        {
                            // 如果不是设备列表，则将每个分组元素分别转换为 JSON 字符串
                            foreach (var gro in group)
                            {
                                string json = JsonExtensions.ToJsonNetString(gro);
                                // 将主题和 JSON 内容添加到列表中
                                topicJsonList.Add(new(topic, json));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsDeviceList)
            {
                string json = data.Select(a => a).ToList().ToJsonNetString();
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    string json = JsonExtensions.ToJsonNetString(group);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json));
                }
            }
        }
        return topicJsonList;
    }

    protected List<TopicArray> GetDeviceTopicArray(IEnumerable<DevModel> item)
    {
        IEnumerable<dynamic>? data = Application.DynamicModelExtension.GetDynamicModel<DevModel>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel);
        List<TopicArray> topicJsonList = new List<TopicArray>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.DeviceTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = data.GroupByKeys(topics).ToList();
                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        // 上传主题
                        // 获取预定义的设备主题
                        string topic = _businessPropertyWithCacheIntervalScript.DeviceTopic;

                        // 将主题中的占位符替换为分组键对应的值
                        for (int i = 0; i < topics.Count; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                        }

                        // 上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsDeviceList)
                        {
                            // 如果是设备列表，则将整个分组转换为 JSON 字符串
                            var json = Serialize(group.Select(a => a).ToList());
                            // 将主题和 JSON 内容添加到列表中
                            topicJsonList.Add(new(topic, json));
                        }
                        else
                        {
                            // 如果不是设备列表，则将每个分组元素分别转换为 JSON 字符串
                            foreach (var gro in group)
                            {
                                var json = Serialize(gro);
                                // 将主题和 JSON 内容添加到列表中
                                topicJsonList.Add(new(topic, json));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsDeviceList)
            {
                var json = Serialize(data.Select(a => a).ToList());
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    var json = Serialize(group);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json));
                }
            }
        }
        return topicJsonList;
    }

    protected List<TopicJson> GetVariable(IEnumerable<VarModel> item)
    {
        IEnumerable<dynamic>? data = Application.DynamicModelExtension.GetDynamicModel<VarModel>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptVariableModel);
        List<TopicJson> topicJsonList = new List<TopicJson>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.VariableTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = data.GroupByKeys(topics).ToList();
                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        // 上传主题
                        // 获取预定义的变量主题
                        string topic = _businessPropertyWithCacheIntervalScript.VariableTopic;

                        // 将主题中的占位符替换为分组键对应的值
                        for (int i = 0; i < topics.Count; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                        }

                        // 上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsVariableList)
                        {
                            // 如果是变量列表，则将整个分组转换为 JSON 字符串
                            string json = group.Select(a => a).ToList().ToJsonNetString();
                            // 将主题和 JSON 内容添加到列表中
                            topicJsonList.Add(new(topic, json));
                        }
                        else
                        {
                            // 如果不是变量列表，则将每个分组元素分别转换为 JSON 字符串
                            foreach (var gro in group)
                            {
                                string json = JsonExtensions.ToJsonNetString(gro);
                                // 将主题和 JSON 内容添加到列表中
                                topicJsonList.Add(new(topic, json));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsVariableList)
            {
                string json = data.Select(a => a).ToList().ToJsonNetString();
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.VariableTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    string json = JsonExtensions.ToJsonNetString(group);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.VariableTopic, json));
                }
            }
        }
        return topicJsonList;
    }


    protected List<TopicArray> GetVariableTopicArray(IEnumerable<VarModel> item)
    {
        IEnumerable<dynamic>? data = Application.DynamicModelExtension.GetDynamicModel<VarModel>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptVariableModel);
        List<TopicArray> topicJsonList = new List<TopicArray>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.VariableTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = data.GroupByKeys(topics).ToList();
                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        // 上传主题
                        // 获取预定义的变量主题
                        string topic = _businessPropertyWithCacheIntervalScript.VariableTopic;

                        // 将主题中的占位符替换为分组键对应的值
                        for (int i = 0; i < topics.Count; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                        }

                        // 上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsVariableList)
                        {
                            // 如果是变量列表，则将整个分组转换为 JSON 字符串
                            var json = Serialize(group.Select(a => a).ToList());
                            // 将主题和 JSON 内容添加到列表中
                            topicJsonList.Add(new(topic, json));
                        }
                        else
                        {
                            // 如果不是变量列表，则将每个分组元素分别转换为 JSON 字符串
                            foreach (var gro in group)
                            {
                                var json = Serialize(gro);
                                // 将主题和 JSON 内容添加到列表中
                                topicJsonList.Add(new(topic, json));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsVariableList)
            {
                var json = Serialize(data.Select(a => a).ToList());
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.VariableTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    var json = Serialize(group);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.VariableTopic, json));
                }
            }
        }
        return topicJsonList;
    }

    #endregion 封装方法
}
