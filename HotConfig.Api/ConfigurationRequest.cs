using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HotConfig.Api
{
    /// <summary>Body of create and update calls.</summary>
    public sealed class ConfigurationRequest
    {
        public const int MaxApplicationName = 100;
        public const int MaxName = 100;
        public const int MaxValue = 500;

        // Canonical names are the .NET type names, because ConfigurationReader.GetValue<T> matches Type against typeof(T).Name.
        private static readonly Dictionary<string, (string Canonical, Type Clr)> SupportedTypes = BuildTypes();

        public static IReadOnlyCollection<string> TypeNames { get; } = SupportedTypes.Values.Select(v => v.Canonical).Distinct().ToList();

        public string ApplicationName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        /// <summary>Defaults to true on create and keeps the stored flag on update.</summary>
        public bool? IsActive { get; set; }

        /// <summary>Validates the request. Returns the field errors, or an empty dictionary when valid.</summary>
        public Dictionary<string, string[]> Validate(out string canonicalType)
        {
            var errors = new Dictionary<string, string[]>();
            canonicalType = null;

            Require(errors, nameof(this.ApplicationName), this.ApplicationName, MaxApplicationName);
            Require(errors, nameof(this.Name), this.Name, MaxName);

            if (string.IsNullOrWhiteSpace(this.Type))
                errors[nameof(this.Type)] = new[] { "Type is required." };
            else if (!SupportedTypes.TryGetValue(this.Type.Trim(), out var type))
                errors[nameof(this.Type)] = new[] { $"Type must be one of: {string.Join(", ", TypeNames)}." };
            else
            {
                canonicalType = type.Canonical;
                if (!string.IsNullOrEmpty(this.Value) && !CanConvert(this.Value, type.Clr))
                    errors[nameof(this.Value)] = new[] { $"Value is not a valid {type.Canonical}." };
            }

            if (string.IsNullOrEmpty(this.Value))
                errors[nameof(this.Value)] = new[] { "Value is required." };
            else if (this.Value.Length > MaxValue)
                errors[nameof(this.Value)] = new[] { $"Value must be at most {MaxValue} characters." };

            return errors;
        }

        private static void Require(Dictionary<string, string[]> errors, string field, string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
                errors[field] = new[] { $"{field} is required." };
            else if (value.Trim().Length > max)
                errors[field] = new[] { $"{field} must be at most {max} characters." };
        }

        private static bool CanConvert(string value, Type type)
        {
            try
            {
                Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception exception) when (exception is FormatException || exception is InvalidCastException || exception is OverflowException)
            {
                return false;
            }
        }

        private static Dictionary<string, (string, Type)> BuildTypes()
        {
            var map = new Dictionary<string, (string, Type)>(StringComparer.OrdinalIgnoreCase);
            void Add(Type clr, params string[] aliases)
            {
                map[clr.Name] = (clr.Name, clr);
                foreach (var alias in aliases)
                    map[alias] = (clr.Name, clr);
            }

            Add(typeof(string));
            Add(typeof(int), "int");
            Add(typeof(long), "long");
            Add(typeof(bool), "bool");
            Add(typeof(double));
            Add(typeof(decimal));
            Add(typeof(DateTime));
            return map;
        }
    }
}
