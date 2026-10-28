using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotConfig.Extensions
{
    public static class ObjectExtensions
    {

        public static T ConvertTo<T>(this object source, T defaultValue = default(T))
        {
            var type = typeof(T);

            if (source == null || source == DBNull.Value || (source is string s && string.IsNullOrEmpty(s)))
                return defaultValue;

            type = type.GetUnderlyingType();

            if (type.IsEnum)
                return source.ToString().ToEnum<T>();

            if (type == typeof(Guid))
            {
                if (source is string strSource)
                    source = new Guid(strSource);
                if (source is byte[] bytes)
                    source = new Guid(bytes);
            }

            return (T) Convert.ChangeType(source, type, CultureInfo.InvariantCulture);
        }

        public static bool IsNumeric(this object value)
        {
            switch (value)
            {
                case null:
                case DateTime _:
                    return false;
                case short _:
                case int _:
                case long _:
                case decimal _:
                case float _:
                case double _:
                case bool _:
                    return true;
                default:
                    try
                    {
                        if (value is string strValue)
                            double.Parse(strValue);
                        else
                            double.Parse(value.ToString());

                        return true;
                    }
                    catch
                    {
                        // ignored
                    }

                    return false;
            }
        }

        public static string ToJson(this object obj, bool compress = true, bool ignoreNulls = true, bool ignoreDefaultValues = false, string dateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffzzz", bool stringEnumConverter = false, bool useJavaScriptDateTimeConverter = false)
        {
            if (obj == null)
                return string.Empty;

            var options = new JsonSerializerOptions { WriteIndented = !compress };

            if (useJavaScriptDateTimeConverter)
                options.Converters.Add(new JavaScriptDateTimeConverter());
            else
            {
                if (string.IsNullOrWhiteSpace(dateFormatString))
                {
                    var enCulture = new CultureInfo("en-US");
                    dateFormatString = enCulture.DateTimeFormat.FullDateTimePattern;
                }

                options.Converters.Add(new FormattedDateTimeConverter(dateFormatString));
            }

            if (stringEnumConverter)
                options.Converters.Add(new JsonStringEnumConverter());
            if (ignoreDefaultValues)
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            else if (ignoreNulls)
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            // Serialize by runtime type, as the previous serializer did.
            return JsonSerializer.Serialize(obj, obj.GetType(), options);
        }

        public static T FromJson<T>(this string value) => JsonSerializer.Deserialize<T>(value, FromJsonOptions);

        // The previous serializer matched property names case-insensitively and read dates with the standard parser.
        private static readonly JsonSerializerOptions FromJsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }
}
