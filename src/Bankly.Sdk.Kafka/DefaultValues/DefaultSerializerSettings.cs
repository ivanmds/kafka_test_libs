using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Bankly.Sdk.Kafka.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Bankly.Sdk.Kafka.DefaultValues
{
    internal static class DefaultSerializerSettings
    {
        public static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
        {
                ContractResolver = new EmptyCollectionContractResolver
                {
                        NamingStrategy = new CamelCaseNamingStrategy
                        {
                                ProcessDictionaryKeys = false
                        }
                },
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                        new CurrencyCodeConverter(),
                        new StringEnumConverter(),
                        new KeyValuePairConverter()
                }
        };
    }

    internal class EmptyCollectionContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            base.CreateProperty(member, memberSerialization);

            var property = base.CreateProperty(member, memberSerialization);

            var shouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = obj => (shouldSerialize == null || shouldSerialize(obj)) && !IsEmptyCollection(property, obj);
            return property;
        }

        private static bool IsEmptyCollection(JsonProperty property, object target)
        {
            var value = property.ValueProvider?.GetValue(target);
            if(value is null || (value is ICollection collection && collection.Count == 0)) { return true; }

            if(!typeof(IEnumerable).IsAssignableFrom(property.PropertyType)) { return false; }

            var countProp = property.PropertyType?.GetProperty("Count");
            if(countProp is null) { return false; }

            var count = (int?)countProp.GetValue(value, null);
            return count is null || count == 0;
        }
    }
}
