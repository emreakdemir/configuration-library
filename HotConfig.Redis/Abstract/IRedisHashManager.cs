using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotConfig.Domain.Abstract.Model;
using StackExchange.Redis;

namespace HotConfig.Redis.Abstract
{
    public interface IRedisHashManager
    {
        IConnectionMultiplexer Connection { get; }
        IDatabase Database { get; }
        string HashKey { get; }
        bool Set<T>(T cacheModel) where T : ICacheModel;
        bool Set<T>(IEnumerable<T> values) where T : ICacheModel;
        T Get<T>(string key) where T : ICacheModel;
        T Get<T>(string key, Func<Task<T>> loadFrom) where T : ICacheModel;
    }
}
