using System;
using System.Collections.Generic;
using Bankly.Sdk.Kafka.DefaultValues;

namespace Bankly.Sdk.Kafka.Values
{
    public class HeaderValue
    {
        private readonly IDictionary<string, string> _header;

        public HeaderValue()
        {
            _header = new Dictionary<string, string>();
        }

        public void AddCorrelationId(string value)
            => PutKeyValue(KeyValue.Create(DefaultHeader.KeyCorrelationId, value));

        public string GetCorrelationId()
          => GetValue(DefaultHeader.KeyCorrelationId);

        public void AddResponseTopic(string topicName)
            => PutKeyValue(KeyValue.Create(DefaultHeader.KeyResponseTopic, topicName));

        internal string GetResponseTopic()
            => GetValue(DefaultHeader.KeyResponseTopic);


        internal void AddIsNewClient()
            => PutKeyValue(KeyValue.Create(DefaultHeader.KeyIsNewClient, "true"));

        internal bool GetIsNewClient()
           => _header.ContainsKey(DefaultHeader.KeyIsNewClient);

        internal void AddIsNotification()
            => PutKeyValue(KeyValue.Create(DefaultHeader.KeyIsNotification, "true"));

        internal bool GetIsNotification()
           => _header.ContainsKey(DefaultHeader.KeyIsNotification);

        

        internal void AddEventName(string eventName)
            => PutKeyValue(KeyValue.Create(DefaultHeader.KeyEventName, eventName));

        internal string GetEventName()
        {
            var eventName = GetValue(DefaultHeader.KeyEventName);

            if (string.IsNullOrWhiteSpace(eventName))
                eventName = DefaultHeader.KeyDefaultEvenName;

            return eventName;
        }

        internal void AddWillRetry(bool value)
            => PutKeyValue(KeyValue.Create(DefaultHeader.KeyWillRetry, value.ToString()));

        internal bool GetWillRetry()
        {
            var value = GetValue(DefaultHeader.KeyWillRetry);
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return bool.Parse(value);
        }

        internal void AddRetryAt(int seconds, int attempt)
        {
            var retryAt = DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeMilliseconds();
            PutKeyValue(KeyValue.Create(DefaultHeader.KeyRetryAt, retryAt.ToString()));
            PutKeyValue(KeyValue.Create(DefaultHeader.KeyCurrentAttempt, attempt.ToString()));
        }

        internal int GetRetryAt()
        {
            var value = GetValue(DefaultHeader.KeyRetryAt);
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            var retryWhen = long.Parse(value);
            var dtNowMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return (int)(retryWhen - dtNowMilliseconds);
        }

        internal int GetCurrentAttempt()
        {
            var value = GetValue(DefaultHeader.KeyCurrentAttempt);
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return int.Parse(value);
        }

        
        public IEnumerable<KeyValue> GetKeyValues()
        {
            foreach (var kv in _header)
                yield return KeyValue.Create(kv.Key, kv.Value);
        }

        public void PutKeyValue(KeyValue keyValue)
        {
            if (_header.ContainsKey(keyValue.Key))
                _header[keyValue.Key] = keyValue.Value;
            else
                _header.Add(keyValue.Key, keyValue.Value);
        }

        public void PutKeyValue(string key, string value)
            => PutKeyValue(new KeyValue(key, value));


        public static HeaderValue Create(KeyValue keyValue)
        {
            var header = new HeaderValue();
            header.PutKeyValue(keyValue);

            return header;
        }

        public static HeaderValue Create(string key, string value)
            => Create(new KeyValue(key, value));

        public static HeaderValue Create()
            => new HeaderValue();

        public string GetValue(string key)
        {
            var value = string.Empty;

            if (_header.ContainsKey(key))
                value = _header[key];

            return value;
        }
    }

    public class KeyValue
    {
        private readonly string _key;
        private readonly string _value;

        public KeyValue(string key, string value)
        {
            _key = key;
            _value = value;
        }

        public string Key => _key;
        public string Value => _value;

        public static KeyValue Create(string key, string value)
            => new KeyValue(key, value);

        internal static KeyValue Create(object keyWillRetry)
        {
            throw new NotImplementedException();
        }
    }
}
