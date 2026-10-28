using System;
using System.Threading.Tasks;
using HotConfig.Domain.Concrete.Model;
using HotConfig.Redis.Concrete;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace HotConfig.Tests
{
    /// <summary>One Redis container per test class.</summary>
    public sealed class RedisFixture : IAsyncLifetime
    {
        private readonly RedisContainer _container = new RedisBuilder("redis:7-alpine").Build();
        private ConnectionMultiplexer _connection;

        public string ConnectionString => this._container.GetConnectionString();

        public async Task InitializeAsync()
        {
            await this._container.StartAsync();
            this._connection = await ConnectionMultiplexer.ConnectAsync(this.ConnectionString);
        }

        public async Task DisposeAsync()
        {
            this._connection?.Dispose();
            await this._container.DisposeAsync();
        }

        public static string NewApp() => "app-" + Guid.NewGuid().ToString("N");

        /// <summary>Writes a record the way RedisHashManager does (field = lower-case name).</summary>
        public void Put(string app, string name, string value, string type = "String", bool isActive = true)
        {
            var manager = new RedisHashManager(app, this._connection);
            manager.Set(new CacheModel
                        {
                            Key = name,
                            Model = new ConfigurationModel { ApplicationName = app, Name = name, Type = type, Value = value, IsActive = isActive }
                        });
        }

        public void Remove(string app, string name) => this._connection.GetDatabase().HashDelete(app, name.ToLower());
    }
}
