using System;
using System.Threading.Tasks;
using CodeSide.Domain.Abstract.Base;
using ServiceStack.Redis;

namespace CodeSide.Redis.Abstract
{
    public interface IRedisValueManager
    {
        RedisManagerPool RedisManagerPool { get; }
        IRedisClient Client { get; }
        bool Set<T>(string key, T model, TimeSpan? expiresIn = null) where T : IModel;
        T Get<T>(string key) where T : IModel;
        T Get<T>(string key, Func<Task<T>> loadFrom, TimeSpan? expiresIn = null) where T : IModel;
    }
}