using System;
using System.Collections.Generic;

namespace Bankly.Sdk.Kafka
{
    internal static class Binds
    {
        private static readonly IDictionary<string, Type> _bindStringType;
        private static readonly IDictionary<string, string> _bindStringString;

        static Binds()
        {
            _bindStringType = new Dictionary<string, Type>();
            _bindStringString = new Dictionary<string, string>();
        }

        public static void AddType(string key, Type value)
        {
            if (!_bindStringType.ContainsKey(key))
                _bindStringType.Add(key, value);
        }

        public static Type? GetType(string key)
        {
            if (_bindStringType.ContainsKey(key))
                return _bindStringType[key];

            return null;
        }

        public static void AddString(string key, string value)
        {
            if (_bindStringString.ContainsKey(key))
                throw new ArgumentException($"The key {key} alreay registered");
            else
                _bindStringString.Add(key, value);
        }

        public static string? GetString(string key)
        {
            if (_bindStringString.ContainsKey(key))
                return _bindStringString[key];

            return null;
        }
    }
}
