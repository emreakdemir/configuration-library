using System;
using System.Threading;
using System.Threading.Tasks;
using HotConfig.Domain.Abstract.Base;
using HotConfig.Extensions;
using HotConfig.Redis.Abstract;
using StackExchange.Redis;

namespace HotConfig.Redis.Concrete
{

    public class RedisValueManager : IRedisValueManager, IDisposable
    {
        private readonly object _lockObject = new object();
        private readonly Lazy<IConnectionMultiplexer> _connection;
        private readonly bool _ownsConnection;

        public IConnectionMultiplexer Connection => this._connection.Value;
        public IDatabase Database => this.Connection.GetDatabase();

        public bool Set<T>(string key, T model, TimeSpan? expiresIn = null) where T : IModel
        {
            var result = false;

            if (model == null)
                return false;
            try
            {
                result = this.Database.StringSet(key, model.ToJson(), expiresIn, When.Always);
            }
            catch (Exception)
            {
                //TODO: Log
                result = false;
            }

            return result;
        }

        public T Get<T>(string key) where T : IModel
        {
            var result = default(T);
            string value = this.Database.StringGet(key);
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = value.FromJson<T>();
            }

            return result;
        }

        public T Get<T>(string key, Func<Task<T>> loadFrom, TimeSpan? expiresIn = null) where T : IModel
        {
            var result = default(T);
            try
            {
                result = this.Get<T>(key);

                if (result == null)
                {
                    lock (this._lockObject)
                    {
                        result = this.Get<T>(key);
                        if (result == null)
                        {
                            result = loadFrom().Result;
                            if (result != null)
                            {
                                this.Set(key, result, expiresIn);
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

        public RedisValueManager(string connectionString)
        {
            this._ownsConnection = true;
            // Connect on first use (the previous client pool was lazy too); retry on the next call if connecting fails.
            this._connection = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString),
                                                                LazyThreadSafetyMode.PublicationOnly);
        }

        public RedisValueManager(IConnectionMultiplexer connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            this._connection = new Lazy<IConnectionMultiplexer>(() => connection);
        }

        public void Dispose()
        {
            if (this._ownsConnection && this._connection.IsValueCreated)
                this._connection.Value.Dispose();
        }

    }
}
