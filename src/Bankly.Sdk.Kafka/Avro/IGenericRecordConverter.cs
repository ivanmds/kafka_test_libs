using Avro.Generic;
using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Avro
{
    public interface IGenericRecordConverter
    {
        Task<GenericRecord> ParseToGenericRecordAsync<TMessage>(TMessage message, string topicName) where TMessage : class;
    }
}
