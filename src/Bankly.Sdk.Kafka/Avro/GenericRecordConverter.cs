using Avro;
using Avro.Generic;
using Bankly.Sdk.Kafka.Configuration;
using Bankly.Sdk.Kafka.Exceptions;
using Confluent.SchemaRegistry;
using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Avro
{
    public class GenericRecordConverter : IGenericRecordConverter
    {
        private readonly KafkaConnection _kafkaConnection;
        private bool hasConnectionToSchameRegistry = false;
        private CachedSchemaRegistryClient _cachedSchemaRegistryClient;

        public GenericRecordConverter(KafkaConnection kafkaConnection)
        {
            _kafkaConnection = kafkaConnection;

            if (string.IsNullOrEmpty(_kafkaConnection.UrlSchemaRegistryServer) == false)
            {
                hasConnectionToSchameRegistry = true;
                _cachedSchemaRegistryClient = new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = kafkaConnection.UrlSchemaRegistryServer
                });
            }
        }
        
        public async Task<GenericRecord> ParseToGenericRecordAsync<TMessage>(TMessage message, string topicName) where TMessage : class
        {
            if (hasConnectionToSchameRegistry == false)
                throw new ConnectionSchemaRegistryServerException();

            var subjectName = $"{topicName}-value";
            var latestSchema = await _cachedSchemaRegistryClient.GetLatestSchemaAsync(subjectName);
            var recordSchema = (RecordSchema)RecordSchema.Parse(await _cachedSchemaRegistryClient.GetSchemaAsync(latestSchema.Subject, latestSchema.Version));

            return message.ParseToGenericRecord(recordSchema);
        }
    }
}
