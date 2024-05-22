using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avro;
using Avro.Generic;

namespace Bankly.Sdk.Kafka.Avro
{
    internal static class GenericRecordExtension
    {
        internal static JsonObject ParseToJson(this GenericRecord generic)
        {
            return BuildJson(generic);
        }

        private static JsonObject BuildJson(GenericRecord generic)
        {
            var json = new JsonObject();
            var schema = generic.Schema;

            foreach (var field in schema)
            {
                var value = generic[field.Name];
                if (value is GenericRecord)
                {
                    var jsonResult = BuildJson(value as GenericRecord);
                    json.Add(field.Name, jsonResult);
                }
                else
                {
                    var jsonValue = JsonValue.Create(value);
                    json.Add(field.Name, jsonValue);
                }
            }

            return json;
        }

        internal static GenericRecord ParseToGenericRecord(this object value, RecordSchema schema)
        {
            var jsonString = JsonSerializer.Serialize(value);
            var json = JsonSerializer.Deserialize<JsonObject>(jsonString);
            return BuildGenericRecord(json, schema);
        }

        private static GenericRecord BuildGenericRecord(JsonObject jsonObject, RecordSchema schema)
        {
            var record = new GenericRecord(schema);

            foreach (var field in schema.Fields)
            {
                JsonNode jsonValue = null;
                jsonObject?.TryGetPropertyValue(field.Name, out jsonValue);
                object value = null;

                if (field.Schema is RecordSchema)
                    value = BuildGenericRecord(jsonValue as JsonObject, (RecordSchema)field.Schema);
                else
                    value = ConvertType(field.Schema, jsonValue);

                record.Add(field.Name, value);
            }

            return record;

        }

        private static object ConvertType(Schema schema, JsonNode jsonValue)
        {
            bool isPrimitiveSchema = schema is PrimitiveSchema;
            bool isUnionSchema = schema is UnionSchema;

            if (isPrimitiveSchema)
                return ConvertPrimitiveSchema(schema as PrimitiveSchema, jsonValue);
            else if (isUnionSchema)
                return ConvertTypeUnion(schema as UnionSchema, jsonValue);
            else
                return null;
        }

        private static object ConvertPrimitiveSchema(PrimitiveSchema schema, JsonNode jsonValue)
        {
            var value = jsonValue?.ToString();

            if (value == null || schema == null)
                return null;

            var tag = schema.Tag;
            switch (tag)
            {
                case Schema.Type.String: return value;
                case Schema.Type.Int: return int.Parse(value);
                case Schema.Type.Long: return long.Parse(value);
                case Schema.Type.Float: return float.Parse(value);
                case Schema.Type.Double: return double.Parse(value);
                case Schema.Type.Boolean: return bool.Parse(value);
                default: return null;
            }
        }

        private static object ConvertTypeUnion(UnionSchema unionSchema, JsonNode jsonValue)
        {
            var schema = unionSchema.Schemas.FirstOrDefault(s =>
                                s.Tag == Schema.Type.String ||
                                s.Tag == Schema.Type.Int ||
                                s.Tag == Schema.Type.Long ||
                                s.Tag == Schema.Type.Float ||
                                s.Tag == Schema.Type.Double ||
                                s.Tag == Schema.Type.Boolean
                                );
            return ConvertPrimitiveSchema(schema as PrimitiveSchema, jsonValue);
        }
    }
}
