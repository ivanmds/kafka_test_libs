﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Bankly.Sdk.Kafka.DefaultValues
{
    internal static class DefaultSerializerSettings
    {
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            },
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                    new StringEnumConverter(),
                    new KeyValuePairConverter()
            }
        };

        public static JsonSerializerSettings JsonSettings => _jsonSettings;
    }
}
