//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;

using ThingsGateway.Foundation.Common.Json.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，额外实现脚本切换实体
/// </summary>
public abstract partial class BusinessBaseWithCacheIntervalScript : BusinessBaseWithCacheInterval
{
    protected sealed override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _businessPropertyWithCacheIntervalScript;

    protected abstract BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript { get; }



    protected override Task DisposeAsync(bool disposing)
    {
        try
        {
            var exexcuteExpressions = CSharpScriptEngineExtension.Do<IDynamicModel>(_businessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel);
            exexcuteExpressions?.TryDispose();
            CSharpScriptEngineExtension.Remove(_businessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel);
        }
        catch
        {
        }
        try
        {
            var exexcuteExpressions = CSharpScriptEngineExtension.Do<IDynamicModel>(_businessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel);
            exexcuteExpressions?.TryDispose();
            CSharpScriptEngineExtension.Remove(_businessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel);
        }
        catch
        {
        }
        try
        {
            var exexcuteExpressions = CSharpScriptEngineExtension.Do<IDynamicModel>(_businessPropertyWithCacheIntervalScript.BigTextScriptPluginEventDataModel);
            exexcuteExpressions?.TryDispose();
            CSharpScriptEngineExtension.Remove(_businessPropertyWithCacheIntervalScript.BigTextScriptPluginEventDataModel);
        }
        catch
        {
        }
        try
        {
            var exexcuteExpressions = CSharpScriptEngineExtension.Do<IDynamicModel>(_businessPropertyWithCacheIntervalScript.BigTextScriptVariableModel);
            exexcuteExpressions?.TryDispose();
            CSharpScriptEngineExtension.Remove(_businessPropertyWithCacheIntervalScript.BigTextScriptVariableModel);
        }
        catch
        {
        }
        return base.DisposeAsync(disposing);
    }

    public virtual string[] Match(string input)
    {
        // 生成缓存键，以确保缓存的唯一性
        var cacheKey = $"{nameof(BusinessBaseWithCacheIntervalScript)}-{CultureInfo.CurrentUICulture.Name}-Match-{input}";

        // 尝试从缓存中获取匹配结果，如果缓存中不存在则创建新的匹配结果
        var strings = App.CacheService.GetOrAdd(cacheKey, entry =>
        {
            List<string> strings = new List<string>();

            // 使用正则表达式查找输入字符串中的所有匹配项
            Regex regex = TopicRegex();
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
            return strings.ToArray();
        });

        // 返回匹配结果列表
        return strings;
    }

    #region 封装方法

    protected IEnumerable<TopicArray> GetAlarmTopicArrays(IEnumerable<AlarmVariable> item)
    {
        var data = Application.DynamicModelExtension.GetDynamicModel<AlarmVariable>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel, LogMessage);
        var topics = Match(_businessPropertyWithCacheIntervalScript.AlarmTopic);
        if (topics.Length > 0)
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
                    for (int i = 0; i < topics.Length; i++)
                    {
                        topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                    }

                    // 上传内容
                    if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
                    {
                        var gList = group.ToList();
                        if (gList.Count > 0)
                        {
                            var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                            // 将主题和 JSON 内容添加到列表中
                            yield return new(topic, json, gList.Count);
                        }
                    }
                    else
                    {
                        // 如果不是报警列表，则将每个分组元素分别转换为 JSON 字符串
                        foreach (var gro in group)
                        {
                            var json = gro.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                            // 将主题和 JSON 内容添加到列表中
                            yield return new(topic, json, 1);
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsAlarmList)
            {
                var gList = data.ToList();
                if (gList.Count > 0)
                {
                    var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json, gList.Count);
                }
            }
            else
            {
                foreach (var group in data)
                {
                    var json = group.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.AlarmTopic, json, 1);
                }
            }
        }
    }
    protected IEnumerable<TopicArray> GetPluginEventDataTopicArrays(IEnumerable<PluginEventData> item)
    {
        var data = Application.DynamicModelExtension.GetDynamicModel<PluginEventData>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptPluginEventDataModel, LogMessage);
        var topics = Match(_businessPropertyWithCacheIntervalScript.PluginEventDataTopic);
        if (topics.Length > 0)
        {
            {
                //获取分组最终结果
                var groups = data.GroupByKeys(topics);

                foreach (var group in groups)
                {
                    // 上传主题
                    // 获取预定义的报警主题
                    string topic = _businessPropertyWithCacheIntervalScript.PluginEventDataTopic;

                    // 将主题中的占位符替换为分组键对应的值
                    for (int i = 0; i < topics.Length; i++)
                    {
                        topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                    }

                    // 上传内容
                    if (_businessPropertyWithCacheIntervalScript.IsPluginEventDataList)
                    {
                        var gList = group.ToList();
                        if (gList.Count > 0)
                        {
                            var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                            // 将主题和 JSON 内容添加到列表中
                            yield return new(topic, json, gList.Count);
                        }
                    }
                    else
                    {
                        // 如果不是报警列表，则将每个分组元素分别转换为 JSON 字符串
                        foreach (var gro in group)
                        {
                            var json = gro.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                            // 将主题和 JSON 内容添加到列表中
                            yield return new(topic, json, 1);
                        }
                    }
                }
            }
        }
        else
        {
            if (_businessPropertyWithCacheIntervalScript.IsPluginEventDataList)
            {
                var gList = data.ToList();
                if (gList.Count > 0)
                {
                    var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.PluginEventDataTopic, json, gList.Count);
                }
            }
            else
            {
                foreach (var group in data)
                {
                    var json = group.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.PluginEventDataTopic, json, 1);
                }
            }
        }
    }
    protected IEnumerable<TopicArray> GetDeviceTopicArray(IEnumerable<DeviceBasicData> item)
    {
        var data = Application.DynamicModelExtension.GetDynamicModel<DeviceBasicData>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel, LogMessage);
        var topics = Match(_businessPropertyWithCacheIntervalScript.DeviceTopic);
        if (topics.Length > 0)
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
                        for (int i = 0; i < topics.Length; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                        }

                        // 上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsDeviceList)
                        {
                            // 如果是设备列表，则将整个分组转换为 JSON 字符串
                            var gList = group.Select(a => a).ToList();
                            if (gList.Count > 0)
                            {
                                var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                                // 将主题和 JSON 内容添加到列表中
                                yield return new(topic, json, gList.Count);
                            }
                        }
                        else
                        {
                            // 如果不是设备列表，则将每个分组元素分别转换为 JSON 字符串
                            foreach (var gro in group)
                            {
                                var json = gro.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                                // 将主题和 JSON 内容添加到列表中
                                yield return new(topic, json, 1);
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
                var gList = data.ToList();
                if (gList.Count > 0)
                {
                    var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json, gList.Count);
                }
            }
            else
            {
                foreach (var group in data)
                {
                    var json = group.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.DeviceTopic, json, 1);
                }
            }
        }
    }

    protected IEnumerable<TopicArray> GetVariableScriptTopicArray(IEnumerable<VariableBasicData> item)
    {
        var data = Application.DynamicModelExtension.GetDynamicModel<VariableBasicData>(item, _businessPropertyWithCacheIntervalScript.BigTextScriptVariableModel, LogMessage);
        var topics = Match(_businessPropertyWithCacheIntervalScript.VariableTopic);
        if (topics.Length > 0)
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
                        for (int i = 0; i < topics.Length; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                        }

                        // 上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsVariableList)
                        {
                            // 如果是变量列表，则将整个分组转换为 JSON 字符串
                            var gList = group.Select(a => a).ToList();

                            if (gList.Count > 0)
                            {
                                var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                                // 将主题和 JSON 内容添加到列表中
                                yield return new(topic, json, gList.Count);
                            }
                        }
                        else
                        {
                            // 如果不是变量列表，则将每个分组元素分别转换为 JSON 字符串
                            foreach (var gro in group)
                            {
                                var json = gro.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                                // 将主题和 JSON 内容添加到列表中
                                yield return new(topic, json, 1);
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
                var gList = data.ToList();
                if (gList.Count > 0)
                {
                    var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.VariableTopic, json, gList.Count);
                }
            }
            else
            {
                foreach (var group in data)
                {
                    var json = group.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.VariableTopic, json, 1);
                }
            }
        }
    }

    protected IEnumerable<TopicArray> GetVariableBasicDataTopicArray(IEnumerable<VariableBasicData> item)
    {
        IEnumerable<VariableBasicData>? data = null;
        if (!_businessPropertyWithCacheIntervalScript.BigTextScriptVariableModel.IsNullOrWhiteSpace())
        {
            foreach (var v in GetVariableScriptTopicArray(item))
                yield return v;

            yield break;  // 提前结束
        }
        else
        {
            data = item;
        }

        var topics = Match(_businessPropertyWithCacheIntervalScript.VariableTopic);
        if (topics.Length > 0)
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
                        for (int i = 0; i < topics.Length; i++)
                        {
                            topic = topic.Replace(@"${" + topics[i] + @"}", group.Key[i]?.ToString());
                        }

                        // 上传内容
                        if (_businessPropertyWithCacheIntervalScript.IsVariableList)
                        {
                            // 如果是变量列表，则将整个分组转换为 JSON 字符串
                            var gList = group.Select(a => a).ToList();
                            if (gList.Count > 0)
                            {
                                var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                                // 将主题和 JSON 内容添加到列表中
                                yield return new(topic, json, gList.Count);
                            }
                        }
                        else
                        {
                            // 如果不是变量列表，则将每个分组元素分别转换为 JSON 字符串
                            foreach (var gro in group)
                            {
                                var json = gro.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                                // 将主题和 JSON 内容添加到列表中
                                yield return new(topic, json, 1);
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
                var gList = data.ToList();
                if (gList.Count > 0)
                {
                    var json = gList.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.VariableTopic, json, gList.Count);
                }
            }
            else
            {
                foreach (var group in data)
                {
                    var json = group.ToSystemTextJsonUtf8Bytes(_businessPropertyWithCacheIntervalScript.JsonFormattingIndented, _businessPropertyWithCacheIntervalScript.JsonIgnoreNull);
                    yield return new(_businessPropertyWithCacheIntervalScript.VariableTopic, json, 1);
                }
            }
        }
    }

    protected string GetDetailLogString(TopicArray topicArray, int queueCount)
    {
        if (queueCount > 0)
            return $"Up Topic：{topicArray.Topic}{Environment.NewLine}PayLoad：{Encoding.UTF8.GetString(topicArray.Payload)} {Environment.NewLine} VarModelQueue:{queueCount}";
        else
            return $"Up Topic：{topicArray.Topic}{Environment.NewLine}PayLoad：{Encoding.UTF8.GetString(topicArray.Payload)}";
    }

    protected string GetCountLogString(TopicArray topicArray, int queueCount)
    {
        if (queueCount > 0)
            return $"Up Topic：{topicArray.Topic}{Environment.NewLine}Count：{topicArray.Count} {Environment.NewLine} VarModelQueue:{queueCount}";
        else
            return $"Up Topic：{topicArray.Topic}{Environment.NewLine}Count：{topicArray.Count}";
    }
    [GeneratedRegex(@"\$\{(.+?)\}")]
    private static partial Regex TopicRegex();
    #endregion 封装方法


}
