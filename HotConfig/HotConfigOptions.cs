using System;

namespace HotConfig
{
    /// <summary>Settings for the HotConfig configuration provider.</summary>
    public class HotConfigOptions
    {
        /// <summary>Application whose configuration records are read. It is also the Redis hash key.</summary>
        public string ApplicationName { get; set; }

        /// <summary>StackExchange.Redis connection string.</summary>
        public string Redis { get; set; }

        /// <summary>How often the provider re-reads Redis. Default: 30 seconds.</summary>
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Optional MySQL connection string. When set, the active records of the application are copied
        /// from MySQL to Redis before each read (legacy behaviour).
        /// </summary>
        public string MySql { get; set; }
    }
}
