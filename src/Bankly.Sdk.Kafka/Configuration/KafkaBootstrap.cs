using System;
using Bankly.Sdk.Kafka.Avro;
using Bankly.Sdk.Kafka.Clients;
using Bankly.Sdk.Kafka.Metrics;
using Bankly.Sdk.Kafka.Traces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bankly.Sdk.Kafka.Configuration
{
    public static class KafkaBootstrap
    {
        public static KafkaConfiguration AddKafka(this IServiceCollection services, KafkaConnection kafkaConnection)
        {
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(10));
            var kafkaBuilder = KafkaBuilder.Create(kafkaConnection);

            services.AddSingleton<IMetricService, MetricService>();
            services.AddSingleton<ITraceService, TraceService>();
            services.AddSingleton(kafkaConnection);
            services.AddSingleton(kafkaBuilder);
            services.AddSingleton<IProducerMessage, ProducerMessage>();
            services.AddSingleton<IGenericRecordConverter, GenericRecordConverter>();


            var kafkaAdminClient = new KafkaAdminClient(kafkaConnection);
            services.AddSingleton((IKafkaAdminClient)kafkaAdminClient);

            var kafkaConfiguration = new KafkaConfiguration(services, kafkaBuilder);
            return kafkaConfiguration;
        }
    }
}
