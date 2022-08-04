using System;
using Newtonsoft.Json;

namespace Bankly.Sdk.Kafka.Contracts
{
    internal class CurrencyCodeConverter : JsonConverter<Currency>
    {
        public override void WriteJson(JsonWriter writer, Currency value, JsonSerializer serializer)
        {
            if (value != null)
                writer.WriteValue(value.Code);
            else
                writer.WriteNull();
        }

        public override Currency ReadJson(JsonReader reader, Type objectType, Currency existingValue,
                                          bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value switch
            {
                string code => new Currency(code),
                int number => new Currency(number),
                _ => null
            };
        }
    }
}
