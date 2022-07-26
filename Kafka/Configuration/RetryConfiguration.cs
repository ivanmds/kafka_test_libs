using System;

namespace Bankly.Sdk.Kafka.Configuration
{
    public class RetryConfiguration
    {
        public RetryConfiguration(RetryTime first, RetryTime? second = null, RetryTime? third = null)
        {
            First = first;
            Second = second;

            if (second == null && third != null)
                throw new Exception("The second field should be informed before inform the third field");

            if (second != null)
            {
                if (second.Minute < first.Minute)
                    throw new Exception("The second should be greater than first");
            }

            if (third != null)
            {
                if (third.Minute < second.Minute)
                    throw new Exception("The third should be greater than second");
            }

            Third = third;  
        }

        public RetryTime First { get; private set; }
        public RetryTime? Second { get; private set; }
        public RetryTime? Third { get; private set; }

        public static RetryConfiguration Create(RetryTime first, RetryTime? second = null, RetryTime? third = null)
            => new RetryConfiguration(first, second, third);
    }

    public class RetryTime
    {
        public RetryTime(int minute)
        {
            Minute = minute;
        }

        public int Minute { get; private set; }

        public static RetryTime Create(int minute) 
            => new RetryTime(minute);
    }
}
