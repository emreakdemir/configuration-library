using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeSide.Domain.Abstract.Model;
using CodeSide.Redis.Abstract;
using CodeSide.Extensions;
using StackExchange.Redis;

namespace CodeSide.Redis.Concrete
{
    public class RedisHashManager : IRedisHashManager, IDisposable
    {
        private readonly Lazy<IConnectionMultiplexer> _connection;
        private readonly bool _ownsConnection;

        public IConnectionMultiplexer Connection => this._connection.Value;
        public IDatabase Database => this.Connection.GetDatabase();
        public string HashKey { get; }
        private readonly object _lockObject = new object();

        public bool Set<T>(T cacheModel) where T : ICacheModel
        {
            bool result;

            if (cacheModel.Model == null)
                return false;
            try
            {
                result = this.Database.HashSet(this.HashKey, cacheModel.Key.ToLower(), cacheModel.ToJson());
            }
            catch (Exception)
            {
                //TODO: Log
                result = false;
            }

            return result;
        }

        public bool Set<T>(IEnumerable<T> values) where T : ICacheModel
        {
            var result = true;
            try
            {
                var database = this.Database;
                foreach (var cacheModel in values.Where(v => v.Model != null).ToList())
                {
                    result = database.HashSet(this.HashKey, cacheModel.Key.ToLower(), cacheModel.ToJson()) && result;
                }
            }
            catch (Exception)
            {
                //TODO: Log
                result = false;
            }

            return result;
        }

        public T Get<T>(string key) where T : ICacheModel
        {
            var result = default(T);
            string value = this.Database.HashGet(this.HashKey, key.ToLower());
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = value.FromJson<T>();
            }

            return result;
        }

        public T Get<T>(string key, Func<Task<T>> loadFrom) where T : ICacheModel
        {
            var result = default(T);
            try
            {
                result = this.Get<T>(key);

                if (result == null || result.IsExpired)
                {
                    lock (this._lockObject)
                    {
                        result = this.Get<T>(key);
                        if (result == null || result.IsExpired)
                        {
                            result = loadFrom().Result;
                            if (result != null)
                            {
                                this.Set(result);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //TODO: Log
            }

            return result;
        }

        public RedisHashManager(string hashKey, string connectionString)
        {
            this.HashKey = hashKey;
            this._ownsConnection = true;
            // Connect on first use (the previous client pool was lazy too); retry on the next call if connecting fails.
            this._connection = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString),
                                                                LazyThreadSafetyMode.PublicationOnly);
        }

        public RedisHashManager(string hashKey, IConnectionMultiplexer connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            this.HashKey = hashKey;
            this._connection = new Lazy<IConnectionMultiplexer>(() => connection);
        }

        public void Dispose()
        {
            if (this._ownsConnection && this._connection.IsValueCreated)
                this._connection.Value.Dispose();
        }

    }
}
