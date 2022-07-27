using System.Collections.Generic;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class RetryConfiguration
    {
        private readonly List<RetryTime> _retryTimes = new List<RetryTime>();

        public RetryConfiguration Add(RetryTime retryTime)
        {
            if (_retryTimes.Count > 5)
                throw new System.Exception("Inform until 5 retryTime");

            if (_retryTimes.Contains(retryTime))
                throw new System.Exception($"The retryTime {retryTime.Seconds}s already informed.");

            return this;
        }

        public RetryTime GetRetryTimeByAttempt(int attempt)
        {
            if (attempt <= 0 || attempt > _retryTimes.Count)
                throw new System.Exception("Attempt invalid");

            var index = attempt - 1;

            return _retryTimes[index];
        }

        public IReadOnlyCollection<RetryTime> GetRetries() => _retryTimes;

        public static RetryConfiguration Create()
            => new RetryConfiguration();
    }

    public class RetryTime : IEqualityComparer<RetryTime>
    {
        public RetryTime(int seconds)
        {
            Seconds = seconds;
        }

        public int Seconds { get; private set; }

        public int GetMilliseconds => Seconds * 1000;

        public static RetryTime Create(int seconds)
            => new RetryTime(seconds);


        public bool Equals(RetryTime x, RetryTime y)
            => x.Seconds == y.Seconds;

        public int GetHashCode(RetryTime obj)
            => $"{obj.Seconds}".GetHashCode();
    }
}
