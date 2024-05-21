namespace Bankly.Sdk.Kafka.Configuration
{
    public class KafkaConnection
    {
        private readonly string _bootstrapServers;
        private readonly bool _isPlaintext;
        private readonly string _urlSchemaRegistryServer;

        public KafkaConnection(string bootstrapServers, bool isPlaintext, string urlSchemaRegistryServer)
        {
            _bootstrapServers = bootstrapServers;
            _isPlaintext = isPlaintext;
            _urlSchemaRegistryServer = urlSchemaRegistryServer; 
        }

        public string BootstrapServers => _bootstrapServers;
        public bool IsPlaintext => _isPlaintext;
        public string UrlSchemaRegistryServer => _urlSchemaRegistryServer;

        public static KafkaConnection Create(string bootstrapServers, bool isPlaintext = true, string urlSchemaRegistryServer = null)
            => new KafkaConnection(bootstrapServers, isPlaintext, urlSchemaRegistryServer);
    }
}
