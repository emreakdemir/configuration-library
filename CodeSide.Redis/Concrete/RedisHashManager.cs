using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeSide.Domain.Abstract.Model;
using CodeSide.Redis.Abstract;
using CodeSide.Extensions;
using ServiceStack.Redis;

namespace CodeSide.Redis.Concrete
{
    public class RedisHashManager : IRedisHashManager
    {
        public RedisManagerPool RedisManagerPool { get; }

        public IRedisClient Client => this.RedisManagerPool.GetClient();
        public string HashKey { get; }
        private readonly object _lockObject = new object();

        public bool Set<T>(T cacheModel) where T : ICacheModel
        {
            bool result;

            if (cacheModel.Model == null)
                return false;
            try
            {
                using (var client = this.Client)
                {
                    result = client.SetEntryInHash(this.HashKey, cacheModel.Key.ToLower(), cacheModel.ToJson());
                }
            }
            catch (Exception exception)
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
                using (var client = this.Client)
                {
                    foreach (var cacheModel in values.Where(v => v.Model != null).ToList())
                    {
                        result = client.SetEntryInHash(this.HashKey, cacheModel.Key.ToLower(), cacheModel.ToJson()) && result;
                    }
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
                result = false;
            }

            return result;
        }

        public T Get<T>(string key) where T : ICacheModel
        {
            var result = default(T);
            using (var client = this.Client)
            {
                client.Hashes[this.HashKey].TryGetValue(key.ToLower(), out var value);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result = value.FromJson<T>();
                }
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
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        public RedisHashManager(string hashKey, string connectionString)
        {
            try
            {
                this.RedisManagerPool = new RedisManagerPool(connectionString);
                this.HashKey = hashKey;
            }
            catch (Exception exception)
            {
                //TODO: LOG
            }
        }

    }
}