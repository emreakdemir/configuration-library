using System;

namespace CodeSide.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string value, T defaultValue = default(T))
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return defaultValue;

                if (!value.IsNumeric())
                    return defaultValue;

                if (!int.TryParse(value, out var numeric))
                    return defaultValue;

                var type = typeof(T).GetUnderlyingType();

                if (Enum.IsDefined(type, value))
                    return (T) Enum.Parse(type, value, true);

                if (Enum.IsDefined(type, numeric))
                    return (T) Enum.ToObject(type, numeric);
            }
            catch
            {
                // ignored
            }

            return defaultValue;
        }
    }
}