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

using System.Text.RegularExpressions;

using ThingsGateway.Core.Extension.Json;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，额外实现脚本切换实体
/// </summary>
public abstract class BusinessBaseWithCacheIntervalScript<T, T2, T3> : BusinessBaseWithCacheInterval<T, T2, T3>
{
    /// <summary>
    /// <inheritdoc cref="DriverPropertys"/>
    /// </summary>
    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _businessPropertyWithCacheIntervalScript;

    protected abstract BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript { get; }

    public virtual List<string> Match(string input)
    {
        List<string> strings = new List<string>();
        Regex regex = new Regex(@"\$\{(.+?)\}");
        MatchCollection matches = regex.Matches(input);
        foreach (Match match in matches)
        {
            if (match.Success)
            {
                string productKey = match.Groups[1].Value;
                strings.Add(productKey);
            }
        }
        return strings;
    }

    #region 封装方法

    protected List<TopicJson> GetAlarms(IEnumerable<AlarmVariable> item)
    {
        IEnumerable<dynamic>? data = item.GetDynamicModel(_businessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel);
        List<TopicJson> topicJsonList = new List<TopicJson>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.AlarmTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = CSharpScriptEngineExtension.GroupByKeys(data, topics).ToList();
                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        //上传主题
                        string topic = _businessPropertyWithCacheIntervalScript.AlarmTopic;
                        for (int i = 0; i < topics.Count; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i].ToString());
                        }
                        //上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
                        {
                            string json = JsonExtension.ToJsonString(group.Select(a => a), true);
                            topicJsonList.Add(new(topic, json));
                        }
                        else
                        {
                            foreach (var gro in group)
                            {
                                string json = JsonExtension.ToJsonString(gro, true);
                                topicJsonList.Add(new(topic, json));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
            {
                string json = JsonExtension.ToJsonString(data, true);
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    string json = JsonExtension.ToJsonString(group, true);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json));
                }
            }
        }

        return topicJsonList;
    }

    protected List<TopicJson> GetDeviceData(IEnumerable<DeviceData> item)
    {
        IEnumerable<dynamic>? data = item.GetDynamicModel(_businessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel);
        List<TopicJson> topicJsonList = new List<TopicJson>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.DeviceTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = CSharpScriptEngineExtension.GroupByKeys(data, topics).ToList();
                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        //上传主题
                        string topic = _businessPropertyWithCacheIntervalScript.DeviceTopic;
                        for (int i = 0; i < topics.Count; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i].ToString());
                        }
                        //上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsDeviceList)
                        {
                            string json = JsonExtension.ToJsonString(group.Select(a => a), true);
                            topicJsonList.Add(new(topic, json));
                        }
                        else
                        {
                            foreach (var gro in group)
                            {
                                string json = JsonExtension.ToJsonString(gro, true);
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
                string json = JsonExtension.ToJsonString(data, true);
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    string json = JsonExtension.ToJsonString(group, true);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json));
                }
            }
        }
        return topicJsonList;
    }

    protected List<TopicJson> GetVariable(IEnumerable<VariableData> item)
    {
        IEnumerable<dynamic>? data = item.GetDynamicModel(_businessPropertyWithCacheIntervalScript.BigTextScriptVariableModel);
        List<TopicJson> topicJsonList = new List<TopicJson>();
        var topics = Match(_businessPropertyWithCacheIntervalScript.VariableTopic);
        if (topics.Count > 0)
        {
            {
                //获取分组最终结果
                var groups = CSharpScriptEngineExtension.GroupByKeys(data, topics).ToList();
                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        //上传主题
                        string topic = _businessPropertyWithCacheIntervalScript.VariableTopic;
                        for (int i = 0; i < topics.Count; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i].ToString());
                        }
                        //上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsVariableList)
                        {
                            string json = JsonExtension.ToJsonString(group.Select(a => a), true);
                            topicJsonList.Add(new(topic, json));
                        }
                        else
                        {
                            foreach (var gro in group)
                            {
                                string json = JsonExtension.ToJsonString(gro, true);
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
                string json = JsonExtension.ToJsonString(data, true);
                topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.VariableTopic, json));
            }
            else
            {
                foreach (var group in data)
                {
                    string json = JsonExtension.ToJsonString(group, true);
                    topicJsonList.Add(new(_businessPropertyWithCacheIntervalScript.VariableTopic, json));
                }
            }
        }
        return topicJsonList;
    }

    protected class TopicJson
    {
        public TopicJson(string topic, string json)
        {
            Topic = topic; Json = json;
        }

        public string Topic { get; set; }
        public string Json { get; set; }
    }

    #endregion 封装方法
}