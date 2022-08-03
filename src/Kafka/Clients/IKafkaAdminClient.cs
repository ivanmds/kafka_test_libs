using System.Threading.Tasks;

namespace Bankly.Sdk.Kafka.Clients
{
    internal interface IKafkaAdminClient
    {
        Task PutTopicAsync(string topicName);
    }
}
