using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotConfig.Domain.Concrete.Model;
using HotConfig.Extensions;
using StackExchange.Redis;

namespace HotConfig.Api
{
    /// <summary>
    /// Writes records into the Redis layout that <c>HotConfigConfigurationProvider</c> reads: one hash per application,
    /// field = lower-case record name, value = JSON <see cref="CacheModel"/> holding the <see cref="ConfigurationModel"/>.
    /// </summary>
    public sealed class ConfigurationCache
    {
        private readonly IConnectionMultiplexer _redis;

        public ConfigurationCache(IConnectionMultiplexer redis) => this._redis = redis;

        /// <summary>Active records are written; inactive ones are removed.</summary>
        public async Task ApplyAsync(ConfigurationModel record)
        {
            if (record.IsActive)
                await this.Database.HashSetAsync(record.ApplicationName, Field(record.Name), Serialize(record));
            else
                await this.RemoveAsync(record);
        }

        public Task RemoveAsync(ConfigurationModel record) => this.Database.HashDeleteAsync(record.ApplicationName, Field(record.Name));

        /// <summary>Rewrites the hash of one application from the given active records and drops stale fields.</summary>
        public async Task<(int Written, int Removed)> ReplaceAsync(string applicationName, IReadOnlyCollection<ConfigurationModel> activeRecords)
        {
            var database = this.Database;
            var wanted = activeRecords.ToDictionary(r => Field(r.Name), r => r, System.StringComparer.Ordinal);

            var stale = (await database.HashKeysAsync(applicationName)).Where(k => !wanted.ContainsKey(k.ToString())).ToArray();
            if (stale.Length > 0)
                await database.HashDeleteAsync(applicationName, stale);
            if (wanted.Count > 0)
                await database.HashSetAsync(applicationName, wanted.Select(w => new HashEntry(w.Key, Serialize(w.Value))).ToArray());

            return (wanted.Count, stale.Length);
        }

        private IDatabase Database => this._redis.GetDatabase();

        private static string Field(string name) => name.ToLower();

        private static string Serialize(ConfigurationModel record) => new CacheModel { Key = record.Name, Model = record }.ToJson();
    }
}
