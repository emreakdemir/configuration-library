using System;
using System.Threading.Tasks;
using HotConfig.Domain.Abstract.Base;
using StackExchange.Redis;

namespace HotConfig.Redis.Abstract
{
    public interface IRedisValueManager
    {
        IConnectionMultiplexer Connection { get; }
        IDatabase Database { get; }
        bool Set<T>(string key, T model, TimeSpan? expiresIn = null) where T : IModel;
        T Get<T>(string key) where T : IModel;
        T Get<T>(string key, Func<Task<T>> loadFrom, TimeSpan? expiresIn = null) where T : IModel;
    }
}
