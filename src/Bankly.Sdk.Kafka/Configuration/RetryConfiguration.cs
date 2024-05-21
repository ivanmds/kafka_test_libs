using System;
using System.Collections.Generic;
using Bankly.Sdk.Kafka.Exceptions;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class RetryConfiguration
    {
        private readonly List<RetryTime> _retryTimes = new List<RetryTime>();
        private readonly List<string> _when = new List<string>();
        private readonly List<string> _whenNot = new List<string>();

        public RetryConfiguration Add(RetryTime retryTime)
        {
            if(_retryTimes.Count > 5)
                throw new MaxRetryTimeConfiguredException("Inform until 5 retryTime");

            if(_retryTimes.Contains(retryTime))
                throw new DuplicatedRetryTimeException($"The retryTime {retryTime.Seconds}s already informed.");

            _retryTimes.Add(retryTime);
            return this;
        }

        public RetryConfiguration When<TException>() where TException : Exception
        {
            var exception = typeof(TException);
            var exceptionFullName = exception.FullName;

            if(_whenNot.Count > 0)
                throw new WhenRetryConflitException("The configuration to 'whenNot' alread started.");

            if(_when.Contains(exceptionFullName))
                throw new WhenRetryConflitException($"The {exception.GetType().FullName} already configured.");

            _when.Add(exceptionFullName);

            return this;
        }

        public RetryConfiguration WhenNot<TException>() where TException : Exception
        {
            var exception = typeof(TException);
            var exceptionFullName = exception.FullName;

            if(_when.Count > 0)
                throw new WhenRetryConflitException("The configuration to 'when' alread started.");

            if(_whenNot.Contains(exceptionFullName))
                throw new WhenRetryConflitException($"The {exception.GetType().FullName} already configured.");

            _whenNot.Add(exceptionFullName);

            return this;
        }

        internal RetryTime GetValidRetryTime(int attempt, Exception ex)
        {
            var exception = ex.InnerException ?? ex;
            var exceptionFullName = exception.GetType().FullName;

            if(_when.Count > 0 && _whenNot.Count == 0 && _when.Contains(exceptionFullName) == false)
                return null;

            if(_whenNot.Count > 0 && _when.Count == 0 && _whenNot.Contains(exceptionFullName))
                return null;

            if(attempt <= 0 || attempt > _retryTimes.Count)
                throw new Exception("Attempt invalid");

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
