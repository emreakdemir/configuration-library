using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSide.Domain.Abstract.Model;
using ServiceStack.Redis;

namespace CodeSide.Redis.Abstract
{
    public interface IRedisHashManager
    {
        RedisManagerPool RedisManagerPool { get; }
        IRedisClient Client { get; }
        string HashKey { get; }
        bool Set<T>(T cacheModel) where T : ICacheModel;
        bool Set<T>(IEnumerable<T> values) where T : ICacheModel;
        T Get<T>(string key) where T : ICacheModel;
        T Get<T>(string key, Func<Task<T>> loadFrom) where T : ICacheModel;
    }
}