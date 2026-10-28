using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeSide.Extensions
{
    /// <summary>
    /// Writes dates as <c>new Date(milliseconds)</c>, like the legacy JSON.NET JavaScript date converter. Write-only.
    /// </summary>
    internal sealed class JavaScriptDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("The JavaScript date constructor syntax is not valid JSON and cannot be read.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utc = value.Kind == DateTimeKind.Unspecified ? value : value.ToUniversalTime();
            var milliseconds = (long) (utc - DateTime.UnixEpoch).TotalMilliseconds;
            writer.WriteRawValue("new Date(" + milliseconds.ToString(CultureInfo.InvariantCulture) + ")");
        }
    }
}
