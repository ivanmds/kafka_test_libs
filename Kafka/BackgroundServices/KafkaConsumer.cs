using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Consumers;
using Bankly.Sdk.Kafka.DefaultValues;
using Bankly.Sdk.Kafka.Values;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bankly.Sdk.Kafka.BackgroundServices
{
    internal static class KafkaConsumer
    {
        private const int DefaultMaxPollIntervalMs = 300000;
        public static async Task ExecuteAsync(IServiceProvider services, ListenerConfiguration listenerConfiguration, IProducerMessage producerMessage, string processkey, CancellationToken stoppingToken)
        {
            var maxPollIntervalMs = listenerConfiguration.RetryTime is null ? DefaultMaxPollIntervalMs 
                : listenerConfiguration.RetryTime.GetMilliseconds + DefaultMaxPollIntervalMs;

            var conf = new ConsumerConfig
            {
                GroupId = listenerConfiguration.GroupId,
                BootstrapServers = listenerConfiguration.KafkaBuilder.KafkaConnection.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                MaxPollIntervalMs = maxPollIntervalMs
            };

            IConsumer<string, string> kafkaConsumer = new ConsumerBuilder<string, string>(conf).Build();
            kafkaConsumer.Subscribe(listenerConfiguration.TopicName);

            try
            {
                using var scope = services.CreateScope();
                await Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = kafkaConsumer.Consume(stoppingToken);

                        if (result is null)
                            continue;

                        var msgBody = result.Message.Value;
                        var header = ParseHeader(result.Message.Headers);

                        var consumerKey = listenerConfiguration.GetConsumerKey(header.GetEventName());
                        var consumerType = RegistryTypes.Recover(consumerKey);
                        var consumer = consumerType == null ? null : scope.ServiceProvider.GetService(consumerType);
                        var context = Context.Create(header);

                        try
                        {
                            if (consumer is null)
                            {
                                kafkaConsumer.Commit();

                                var skippedConsumer = scope.ServiceProvider.GetService<ISkippedMessage>();
                                if (skippedConsumer != null)
                                {
                                    await skippedConsumer.AlertAsync(context, msgBody);
                                }

                                var skipTopicName = GetTopicNameSkipped(listenerConfiguration.GroupId, listenerConfiguration.SourceTopicName);
                                await producerMessage.ProduceAsync(skipTopicName, new { Message = msgBody }, header, stoppingToken);
                            }
                            else
                            {
                                var sleep = header.GetRetryAt();
                                if (sleep > 0)
                                    await Task.Delay(sleep);

                                var methodGetTypeMessage = consumerType.GetMethod("GetTypeMessage");
                                var typeMessage = (Type)methodGetTypeMessage.Invoke(consumer, null);
                                var msgParsed = typeMessage.Name == "String" ? msgBody : JsonConvert.DeserializeObject(msgBody, typeMessage, DefaultSerializerSettings.JsonSettings);

                                var methodBeforeConsume = consumerType.GetMethod("BeforeConsume");
                                methodBeforeConsume.Invoke(consumer, new[] { context, msgParsed });

                                try
                                {
                                    var methodConsume = consumerType.GetMethod("ConsumeAsync");
                                    var methodConsumeResult = (Task)methodConsume.Invoke(consumer, new[] { context, msgParsed });
                                    methodConsumeResult.Wait();
                                }
                                catch (Exception ex)
                                {
                                    //if (listenerConfiguration.RetryConfiguration?.First != null && header.GetCurrentAttempt() == 0)
                                    //{
                                    //    var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                    //        listenerConfiguration.SourceTopicName,
                                    //        listenerConfiguration.GroupId,
                                    //        listenerConfiguration.RetryConfiguration.First.Minute);

                                    //    header.AddRetryAt(listenerConfiguration.RetryConfiguration.First.Minute, 1);
                                    //    header.AddWillRetry(true);
                                    //    await producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    //}
                                    //else if (listenerConfiguration.RetryConfiguration?.Second != null && header.GetCurrentAttempt() == 1)
                                    //{
                                    //    var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                    //        listenerConfiguration.SourceTopicName,
                                    //        listenerConfiguration.GroupId,
                                    //        listenerConfiguration.RetryConfiguration.Second.Minute);

                                    //    header.AddRetryAt(listenerConfiguration.RetryConfiguration.Second.Minute, 2);
                                    //    header.AddWillRetry(true);
                                    //    await producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    //}
                                    //else if (listenerConfiguration.RetryConfiguration?.Third != null && header.GetCurrentAttempt() == 2)
                                    //{
                                    //    var retryTopicName = ListenerConfiguration.GetRetryTopicName(
                                    //        listenerConfiguration.SourceTopicName,
                                    //        listenerConfiguration.GroupId,
                                    //        listenerConfiguration.RetryConfiguration.Third.Minute);

                                    //    header.AddRetryAt(listenerConfiguration.RetryConfiguration.Third.Minute, 3);
                                    //    header.AddWillRetry(true);
                                    //    await producerMessage.ProduceAsync(retryTopicName, msgParsed, header, stoppingToken);
                                    //}
                                    //else
                                    //{
                                    //    header.AddWillRetry(false);
                                    //}

                                    throw ex;
                                }

                                var methodAfterConsume = consumerType.GetMethod("AfterConsume");
                                methodAfterConsume.Invoke(consumer, new[] { context, msgParsed });

                                kafkaConsumer.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            kafkaConsumer.Commit();
                            if (header.GetWillRetry() is false)
                            {
                                var dlqTopicName = GetTopicNameDeadLetter(listenerConfiguration.GroupId, listenerConfiguration.SourceTopicName);
                                await producerMessage.ProduceAsync(dlqTopicName, new { MessageJson = msgBody, Error = ex }, header, stoppingToken);
                            }
                            var methodErrorConsume = consumerType.GetMethod("ErrorConsume");
                            methodErrorConsume.Invoke(consumer, new[] { context as object, ex });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                kafkaConsumer.Dispose();
                throw ex;
            }
        }

        private static HeaderValue ParseHeader(Headers headers)
        {
            var headerValue = HeaderValue.Create();

            if (headers is null)
                return headerValue;

            foreach (var kv in headers)
            {
                var value = Encoding.Default.GetString(kv.GetValueBytes());
                headerValue.PutKeyValue(kv.Key, value);
            }

            return headerValue;
        }

        private static string GetTopicNameSkipped(string groupId, string currentTopicName)
        {
            return $"skipped.{groupId}.{currentTopicName}";
        }

        private static string GetTopicNameDeadLetter(string groupId, string currentTopicName)
        {
            return $"dlq.{groupId}.{currentTopicName}";
        }
    }
}
