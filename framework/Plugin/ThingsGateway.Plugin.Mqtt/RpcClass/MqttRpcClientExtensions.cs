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

using MQTTnet.Protocol;

using System.Text;

namespace MQTTnet.Extensions.Rpc
{
    public static class MqttRpcClientExtensions
    {
        public static Task<byte[]> ExecuteAsync(this MqttRpcClient client, MqttRpcTopicPair mqttRpcTopicPair, string payload, MqttQualityOfServiceLevel qualityOfServiceLevel, TimeSpan timeout)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var buffer = Encoding.UTF8.GetBytes(payload ?? string.Empty);

            return client.ExecuteAsync(timeout, mqttRpcTopicPair, buffer, qualityOfServiceLevel);
        }
    }
}