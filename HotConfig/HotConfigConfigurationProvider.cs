using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HotConfig.Business;
using HotConfig.Business.Base;
using HotConfig.Data.Dapper;
using HotConfig.Domain.Concrete.Model;
using HotConfig.Extensions;
using HotConfig.Redis.Concrete;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace HotConfig
{
    /// <summary>
    /// Reads the active records of one application from a Redis hash. Record names (which may contain ':')
    /// become configuration keys.
    /// </summary>
    /// <remarks>
    /// The first load throws when Redis cannot be read. Later refresh failures are swallowed and the last good
    /// data stays in place.
    /// </remarks>
    public sealed class HotConfigConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly HotConfigOptions _options;
        private readonly RedisHashManager _redis;
        private readonly IConfigurationBusiness _business;
        private readonly object _refreshLock = new object();
        private Timer _timer;
        private volatile bool _disposed;
        private volatile Dictionary<string, ConfigurationModel> _records = new Dictionary<string, ConfigurationModel>(StringComparer.OrdinalIgnoreCase);

        public HotConfigConfigurationProvider(HotConfigConfigurationSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            this._options = source.Options;
            if (string.IsNullOrWhiteSpace(this._options.ApplicationName))
                throw new ArgumentException("ApplicationName is required.", nameof(source));
            if (string.IsNullOrWhiteSpace(this._options.Redis))
                throw new ArgumentException("Redis connection string is required.", nameof(source));
            if (this._options.RefreshInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(source), "RefreshInterval must be positive.");

            this._redis = new RedisHashManager(this._options.ApplicationName, this._options.Redis);
            this._business = source.Business;
            if (this._business == null && !string.IsNullOrWhiteSpace(this._options.MySql))
                this._business = new ConfigurationBusiness(new ConfigurationRepository(this._options.MySql));
        }

        /// <summary>Finds the record behind a configuration key, including its <c>Type</c> column.</summary>
        public bool TryGetRecord(string name, out ConfigurationModel record) => this._records.TryGetValue(name, out record);

        public override void Load()
        {
            var records = this.ReadRecords();
            this.Apply(records, raiseReload: false);

            lock (this._refreshLock)
            {
                if (!this._disposed && this._timer == null)
                    this._timer = new Timer(_ => this.Refresh(), null, this._options.RefreshInterval, Timeout.InfiniteTimeSpan);
            }
        }

        private void Refresh()
        {
            try
            {
                if (this._disposed)
                    return;
                this.Apply(this.ReadRecords(), raiseReload: true);
            }
            catch (Exception)
            {
                // Keep the last good data.
            }
            finally
            {
                lock (this._refreshLock)
                {
                    if (!this._disposed)
                        this._timer?.Change(this._options.RefreshInterval, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private Dictionary<string, ConfigurationModel> ReadRecords()
        {
            this.Seed();

            var result = new Dictionary<string, ConfigurationModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in this._redis.Database.HashGetAll(this._redis.HashKey))
            {
                if (entry.Value.IsNullOrEmpty)
                    continue;

                var record = ((string) entry.Value).FromJson<ConfigurationCacheModel>()?.Model;
                if (record != null && record.IsActive && !string.IsNullOrEmpty(record.Name))
                    result[record.Name] = record;
            }

            return result;
        }

        private void Seed()
        {
            if (this._business == null)
                return;

            var active = this._business.FilterAsync(this._options.ApplicationName).GetAwaiter().GetResult();
            if (active == null)
                return;

            var expireDate = DateTime.Now + this._options.RefreshInterval;
            this._redis.Set(active.Select(a => new CacheModel { Model = a, Key = a.Name, ExpireDate = expireDate }).ToList());
        }

        private void Apply(Dictionary<string, ConfigurationModel> records, bool raiseReload)
        {
            var data = records.ToDictionary(r => r.Key, r => r.Value.Value, StringComparer.OrdinalIgnoreCase);
            var changed = data.Count != this.Data.Count
                          || data.Any(d => !this.Data.TryGetValue(d.Key, out var current) || current != d.Value);

            this._records = records;
            if (!changed)
                return;

            this.Data = data;
            if (raiseReload)
                this.OnReload();
        }

        public void Dispose()
        {
            lock (this._refreshLock)
            {
                this._disposed = true;
                this._timer?.Dispose();
                this._timer = null;
            }

            this._redis.Dispose();
        }
    }
}
