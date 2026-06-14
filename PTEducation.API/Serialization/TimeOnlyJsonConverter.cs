using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PTEducation.API.Serialization
{
    public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private static readonly string[] Formats = new[] { "HH:mm:ss", "HH:mm" };

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (value is null)
                {
                    throw new JsonException("TimeOnly value cannot be null.");
                }

                if (TimeOnly.TryParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    return parsed;
                }

                if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                {
                    return parsed;
                }

                throw new JsonException($"Invalid TimeOnly format: '{value}'. Expected HH:mm or HH:mm:ss.");
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                var hour = GetIntProperty(root, "hour", "Hour");
                var minute = GetIntProperty(root, "minute", "Minute");
                var second = GetIntProperty(root, "second", "Second") ?? 0;

                if (hour is null || minute is null)
                {
                    throw new JsonException("TimeOnly object must include hour and minute.");
                }

                return new TimeOnly(hour.Value, minute.Value, second);
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing TimeOnly.");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        }

        private static int? GetIntProperty(JsonElement root, string lowerName, string upperName)
        {
            if (root.TryGetProperty(lowerName, out var lower) && lower.ValueKind == JsonValueKind.Number)
            {
                return lower.GetInt32();
            }

            if (root.TryGetProperty(upperName, out var upper) && upper.ValueKind == JsonValueKind.Number)
            {
                return upper.GetInt32();
            }

            return null;
        }
    }
}
