using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using StackExchange.Redis;
using Testcontainers.MySql;
using Testcontainers.Redis;
using Xunit;

namespace HotConfig.Tests
{
    /// <summary>Hosts the admin API against throw-away MySQL and Redis containers (one pair for all API tests).</summary>
    public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public const string ApiKey = "test-api-key";

        private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.4").WithDatabase("hotconfig").Build();
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7-alpine").Build();
        private ConnectionMultiplexer _redis;

        public string RedisConnectionString => this._redisContainer.GetConnectionString();
        public string MySqlConnectionString => this._mysql.GetConnectionString();
        public IDatabase Redis => this._redis.GetDatabase();

        public async Task InitializeAsync()
        {
            await Task.WhenAll(this._mysql.StartAsync(), this._redisContainer.StartAsync());
            this._redis = await ConnectionMultiplexer.ConnectAsync(this.RedisConnectionString);

            await using var connection = new MySqlConnection(this.MySqlConnectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "schema.sql")), connection);
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>A client that carries the API key.</summary>
        public System.Net.Http.HttpClient CreateAuthorizedClient()
        {
            var client = this.CreateClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:MySql"] = this.MySqlConnectionString,
                ["ConnectionStrings:Redis"] = this.RedisConnectionString,
                ["HotConfig:ApiKey"] = ApiKey
            }));
        }

        public static string NewApp() => "app-" + Guid.NewGuid().ToString("N");

        async Task IAsyncLifetime.DisposeAsync()
        {
            this._redis?.Dispose();
            await this._mysql.DisposeAsync();
            await this._redisContainer.DisposeAsync();
        }
    }
}
