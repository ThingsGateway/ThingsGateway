#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
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