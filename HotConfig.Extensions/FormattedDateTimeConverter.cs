using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotConfig.Extensions
{
    internal sealed class FormattedDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string _format;

        public FormattedDateTimeConverter(string format)
        {
            this._format = format;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(this._format, CultureInfo.InvariantCulture));
        }
    }
}
