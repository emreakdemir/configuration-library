using System;
using System.Linq;
using HotConfig.Domain.Abstract.Base;
using HotConfig.Domain.Concrete.Model;
using HotConfig.Extensions;
using Microsoft.Extensions.Configuration;

namespace HotConfig
{
    /// <summary>Legacy reader. A thin wrapper over <see cref="HotConfigConfigurationProvider"/>.</summary>
    [Obsolete("Use builder.Configuration.AddHotConfig(...) and IOptionsMonitor<T>. See README.")]
    public class ConfigurationReader : IConfigurationReader, IDisposable
    {
        private readonly HotConfigConfigurationProvider _provider;

        /// <summary>Returns the typed value of an active record, or default when it is missing or its Type does not match.</summary>
        public TType GetValue<TType>(string key)
        {
            var result = default(TType);
            try
            {
                if (this._provider.TryGetRecord(key, out var record)
                    && typeof(TType).GetUnderlyingType().Name.ToLower().Equals(record.Type?.ToLower()))
                {
                    result = record.Value.ConvertTo<TType>();
                }
            }
            catch (Exception)
            {
                // Legacy contract: never throw from GetValue.
            }

            return result;
        }

        /// <param name="applicationName">Application whose records are read.</param>
        /// <param name="connectionStringConfig">Redis and (optional) MySQL connection strings.</param>
        /// <param name="refreshInterval">Refresh interval in milliseconds.</param>
        public ConfigurationReader(string applicationName, ConnectionStringConfigModel connectionStringConfig, int refreshInterval)
        {
            if (connectionStringConfig == null)
                throw new ArgumentNullException(nameof(connectionStringConfig));
            if (string.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(applicationName));
            if (refreshInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            var root = new ConfigurationBuilder().AddHotConfig(o =>
            {
                o.ApplicationName = applicationName;
                o.Redis = connectionStringConfig.RedisConnectionString;
                o.MySql = connectionStringConfig.MySqlConnectionString;
                o.RefreshInterval = TimeSpan.FromMilliseconds(refreshInterval);
            }).Build();

            this._provider = ((IConfigurationRoot) root).Providers.OfType<HotConfigConfigurationProvider>().Single();
        }

        public void Dispose() => this._provider.Dispose();
    }
}
