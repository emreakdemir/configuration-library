using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CodeSide.Extensions
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
            var formatting = Formatting.Indented;
            if (compress)
                formatting = Formatting.None;
            JsonSerializerSettings settings = null;
            if (useJavaScriptDateTimeConverter)
                settings = new JsonSerializerSettings { Converters = new List<JsonConverter> { new JavaScriptDateTimeConverter() } };
            else
            {
                if (string.IsNullOrWhiteSpace(dateFormatString))
                {
                    var enCulture = new CultureInfo("en-US");
                    dateFormatString = enCulture.DateTimeFormat.FullDateTimePattern;
                }
                settings = new JsonSerializerSettings { DateFormatString = dateFormatString };
            }
            if (stringEnumConverter)
                settings.Converters.Add(new StringEnumConverter());
            if (ignoreNulls)
                settings.NullValueHandling = NullValueHandling.Ignore;
            if (ignoreDefaultValues)
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;

            return obj != null ? JsonConvert.SerializeObject(obj, formatting, settings) : string.Empty;
        }
        public static T FromJson<T>(this string value) => JsonConvert.DeserializeObject<T>(value);
    }
}