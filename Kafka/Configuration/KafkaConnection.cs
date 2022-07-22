namespace Bankly.Sdk.Kafka.Configuration
{
    public class KafkaConnection
    {
        private readonly string _bootstrapServers;
        private readonly bool _isPlaintext;

        public KafkaConnection(string bootstrapServers, bool isPlaintext)
        {
            _bootstrapServers = bootstrapServers;
            _isPlaintext = isPlaintext;
        }

        public string BootstrapServers => _bootstrapServers;
        public bool IsPlaintext => _isPlaintext;

        public static KafkaConnection Create(string bootstrapServers, bool isPlaintext = true)
            => new KafkaConnection(bootstrapServers, isPlaintext);
    }
}
