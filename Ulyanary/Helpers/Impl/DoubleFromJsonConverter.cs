using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ulyanary.Helpers.Impl
{
    public class DoubleFromJsonConverter : JsonConverter<double>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(double) == typeToConvert;
        }

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonString = reader.GetString();
            double.TryParse(jsonString, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out var tempDouble);
            return tempDouble;
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
