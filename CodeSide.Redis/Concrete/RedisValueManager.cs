using System;
using System.Threading.Tasks;
using CodeSide.Domain.Abstract.Base;
using CodeSide.Extensions;
using CodeSide.Redis.Abstract;
using ServiceStack.Redis;

namespace CodeSide.Redis.Concrete
{

    public class RedisValueManager : IRedisValueManager
    {
        private readonly object _lockObject = new object();
        public RedisManagerPool RedisManagerPool { get; }
        public IRedisClient Client => this.RedisManagerPool.GetClient();

        public bool Set<T>(string key, T model, TimeSpan? expiresIn = null) where T : IModel
        {
            var result = false;

            if (model == null)
                return false;
            try
            {
                using (var client = this.Client)
                {
                    result = expiresIn.HasValue ? client.Set(key, model, expiresIn.Value) : client.Set(key, model);
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
                result = false;
            }

            return result;
        }

        public T Get<T>(string key) where T : IModel
        {
            var result = default(T);
            using (var client = this.Client)
            {
                var value = client.GetValue(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result = value.FromJson<T>();
                }
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
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        public RedisValueManager(string connectionString)
        {
            try
            {
                this.RedisManagerPool = new RedisManagerPool(connectionString);
            }
            catch (Exception exception)
            {
                //TODO: LOG
            }
        }

    }
}