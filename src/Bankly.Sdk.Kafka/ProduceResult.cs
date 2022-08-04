namespace Bankly.Sdk.Kafka
{
    public class ProduceResult
    {
        public ProduceResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; private set; }

        public static ProduceResult Create(bool isSuccess)
            => new ProduceResult(isSuccess);
    }
}
