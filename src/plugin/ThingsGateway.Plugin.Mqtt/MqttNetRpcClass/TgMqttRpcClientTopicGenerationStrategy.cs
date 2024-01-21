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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MQTTnet.Extensions.Rpc;

namespace ThingsGateway.Plugin.Mqtt;

public sealed class TgMqttRpcClientTopicGenerationStrategy : IMqttRpcClientTopicGenerationStrategy
{
    public const string RpcTopic = "ThingsGateway.Rpc/+/{0}";

    public MqttRpcTopicPair CreateRpcTopics(TopicGenerationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.MethodName.Contains("/") || context.MethodName.Contains("+") || context.MethodName.Contains("#"))
        {
            throw new ArgumentException("The method name cannot contain /, + or #.");
        }

        var requestTopic = $"ThingsGateway/{Guid.NewGuid():N}/{context.MethodName}";
        var responseTopic = requestTopic + "/response";

        return new MqttRpcTopicPair
        {
            RequestTopic = requestTopic,
            ResponseTopic = responseTopic
        };
    }
}