﻿using System.Collections.Generic;
using Kafka.DefaultValues;

namespace Kafka.Values
{
    public class HeaderValue
    {
        private readonly IDictionary<string, string> _header;

        public HeaderValue()
        {
            _header = new Dictionary<string, string>();
        }

        public void AddCorrelationId(string value)
        {
            PutKeyValue(KeyValue.Create(DefaultHeader.KeyCorrelationId, value));
        }

        public void AddIsNotification()
        {
            PutKeyValue(KeyValue.Create(DefaultHeader.KeyIsNotification, "true"));
        }

        public bool IsNotification()
        {
            return _header.ContainsKey(DefaultHeader.KeyIsNotification);
        }

        public void AddEventName(string eventName)
        {
            PutKeyValue(KeyValue.Create(DefaultHeader.KeyEventName, eventName));
        }

        public string GetEventName()
        {
            var eventName = string.Empty;
            _header.TryGetValue(DefaultHeader.KeyEventName, out eventName);
            
            if (string.IsNullOrWhiteSpace(eventName))
                eventName = DefaultHeader.KeyDefaultEvenName;

            return eventName;
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
    }
}
