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

using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using MQTTnet.Internal;
using MQTTnet.Protocol;

using System.Collections.Concurrent;

namespace MQTTnet.Extensions.Rpc
{

    public sealed class MqttRpcClient : IDisposable
    {
        readonly IMqttClient _mqttClient;

        readonly ConcurrentDictionary<string, AsyncTaskCompletionSource<byte[]>> _waitingCalls = new ConcurrentDictionary<string, AsyncTaskCompletionSource<byte[]>>();

        public MqttRpcClient(IMqttClient mqttClient)
        {
            _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));

            _mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
        }

        public void Dispose()
        {
            _mqttClient.ApplicationMessageReceivedAsync -= HandleApplicationMessageReceivedAsync;

            foreach (var tcs in _waitingCalls)
            {
                tcs.Value.TrySetCanceled();
            }

            _waitingCalls.Clear();
        }

        public async Task<byte[]> ExecuteAsync(MqttRpcTopicPair mqttRpcTopicPair, byte[] payload, MqttQualityOfServiceLevel qualityOfServiceLevel, TimeSpan timeout)
        {
            using var timeoutToken = new CancellationTokenSource(timeout);
            try
            {
                return await ExecuteAsync(mqttRpcTopicPair, payload, qualityOfServiceLevel, timeoutToken.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException exception)
            {
                if (timeoutToken.IsCancellationRequested)
                {
                    throw new MqttCommunicationTimedOutException(exception);
                }

                throw;
            }
        }

        public async Task<byte[]> ExecuteAsync(MqttRpcTopicPair mqttRpcTopicPair, byte[] payload, MqttQualityOfServiceLevel qualityOfServiceLevel, CancellationToken cancellationToken = default)
        {
            if (mqttRpcTopicPair == null)
            {
                throw new ArgumentNullException(nameof(mqttRpcTopicPair));
            }

            var requestTopic = mqttRpcTopicPair.RequestTopic;
            var responseTopic = mqttRpcTopicPair.ResponseTopic;

            if (string.IsNullOrWhiteSpace(requestTopic))
            {
                throw new MqttProtocolViolationException("RPC request topic is empty.");
            }

            if (string.IsNullOrWhiteSpace(responseTopic))
            {
                throw new MqttProtocolViolationException("RPC response topic is empty.");
            }

            var requestMessageBuilder = new MqttApplicationMessageBuilder().WithTopic(requestTopic).WithPayload(payload).WithQualityOfServiceLevel(qualityOfServiceLevel);

            if (_mqttClient.Options.ProtocolVersion == MqttProtocolVersion.V500)
            {
                requestMessageBuilder.WithResponseTopic(responseTopic);
            }

            var requestMessage = requestMessageBuilder.Build();

            try
            {
                var awaitable = new AsyncTaskCompletionSource<byte[]>();

                if (!_waitingCalls.TryAdd(responseTopic, awaitable))
                {
                    throw new InvalidOperationException();
                }

                var subscribeOptions = new MqttClientSubscribeOptionsBuilder().WithTopicFilter(responseTopic, qualityOfServiceLevel).Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken).ConfigureAwait(false);
                await _mqttClient.PublishAsync(requestMessage, cancellationToken).ConfigureAwait(false);

                using (cancellationToken.Register(
                           () =>
                           {
                               awaitable.TrySetCanceled();
                           }))
                {
                    return await awaitable.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                _waitingCalls.TryRemove(responseTopic, out _);
                await _mqttClient.UnsubscribeAsync(responseTopic, CancellationToken.None).ConfigureAwait(false);
            }
        }

        Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            if (!_waitingCalls.TryRemove(eventArgs.ApplicationMessage.Topic, out var awaitable))
            {
                return CompletedTask.Instance;
            }

            var payloadBuffer = eventArgs.ApplicationMessage.PayloadSegment.ToArray();

            awaitable.TrySetResult(payloadBuffer);

            // Set this message to handled to that other code can avoid execution etc.
            eventArgs.IsHandled = true;

            return CompletedTask.Instance;
        }
    }
}