using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotConfig.Business.Base;
using HotConfig.Domain.Concrete.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HotConfig.Tests
{
    public class ProviderTests : IClassFixture<RedisFixture>
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(200);
        private readonly RedisFixture _redis;

        public ProviderTests(RedisFixture redis) => this._redis = redis;

        public class Features
        {
            public string Banner { get; set; }
            public int Max { get; set; }
        }

        private IConfigurationRoot Build(string app, IConfigurationBusiness business = null)
        {
            var source = new HotConfigConfigurationSource(new HotConfigOptions
                                                          {
                                                              ApplicationName = app,
                                                              Redis = this._redis.ConnectionString,
                                                              RefreshInterval = Interval
                                                          }) { Business = business };
            return new ConfigurationBuilder().Add(source).Build();
        }

        private static async Task Eventually(Func<bool> condition)
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (!condition())
            {
                Assert.True(DateTime.UtcNow < deadline, "Condition not met within 10 s.");
                await Task.Delay(50);
            }
        }

        [Fact]
        public void Loads_values_and_sections()
        {
            var app = RedisFixture.NewApp();
            this._redis.Put(app, "Features:Banner", "hello");
            this._redis.Put(app, "Features:Max", "5", "Int32");

            var config = this.Build(app);

            Assert.Equal("hello", config["features:banner"]);
            Assert.Equal(5, config.GetSection("Features").Get<Features>().Max);
        }

        [Fact]
        public void Missing_key_returns_null()
        {
            var config = this.Build(RedisFixture.NewApp());
            Assert.Null(config["nope"]);
        }

        [Fact]
        public void Inactive_record_is_ignored()
        {
            var app = RedisFixture.NewApp();
            this._redis.Put(app, "A", "1", isActive: false);
            this._redis.Put(app, "B", "2");

            var config = this.Build(app);

            Assert.Null(config["A"]);
            Assert.Equal("2", config["B"]);
        }

        [Fact]
        public async Task Changed_value_reaches_options_monitor_within_one_interval()
        {
            var app = RedisFixture.NewApp();
            this._redis.Put(app, "Banner", "v1");
            var config = this.Build(app);
            using var services = new ServiceCollection().Configure<Features>(config).BuildServiceProvider();
            var monitor = services.GetRequiredService<IOptionsMonitor<Features>>();
            Assert.Equal("v1", monitor.CurrentValue.Banner);

            var changes = 0;
            using var _ = monitor.OnChange(_ => Interlocked.Increment(ref changes));
            this._redis.Put(app, "Banner", "v2");

            await Eventually(() => monitor.CurrentValue.Banner == "v2");
            Assert.True(changes >= 1);
        }

        [Fact]
        public async Task Removed_and_inactivated_records_disappear()
        {
            var app = RedisFixture.NewApp();
            this._redis.Put(app, "Gone", "1");
            this._redis.Put(app, "Off", "1");
            var config = this.Build(app);
            Assert.Equal("1", config["Gone"]);
            Assert.Equal("1", config["Off"]);

            this._redis.Remove(app, "Gone");
            this._redis.Put(app, "Off", "1", isActive: false);

            await Eventually(() => config["Gone"] == null && config["Off"] == null);
        }

        [Fact]
        public async Task MySql_seeding_copies_active_records_to_redis_before_each_read()
        {
            var app = RedisFixture.NewApp();
            var business = Substitute.For<IConfigurationBusiness>();
            IEnumerable<ConfigurationModel> rows = new[] { new ConfigurationModel { ApplicationName = app, Name = "Seeded", Type = "String", Value = "one", IsActive = true } };
            business.FilterAsync(app, Arg.Any<bool>()).Returns(_ => Task.FromResult(rows));

            var config = this.Build(app, business);
            Assert.Equal("one", config["Seeded"]);

            rows = new[] { new ConfigurationModel { ApplicationName = app, Name = "Seeded", Type = "String", Value = "two", IsActive = true } };
            await Eventually(() => config["Seeded"] == "two");
        }

        [Fact]
        public void First_load_throws_when_redis_is_unreachable()
        {
            var source = new HotConfigConfigurationSource(new HotConfigOptions
                                                          {
                                                              ApplicationName = "x",
                                                              Redis = "localhost:1,abortConnect=true,connectTimeout=500"
                                                          });
            Assert.ThrowsAny<Exception>(() => new ConfigurationBuilder().Add(source).Build());
        }

        [Fact]
        public void Options_are_validated()
        {
            Assert.Throws<ArgumentException>(() => new HotConfigConfigurationProvider(new HotConfigConfigurationSource(new HotConfigOptions { Redis = "x" })));
        }
    }

#pragma warning disable CS0618 // legacy wrapper under test
    public class LegacyReaderTests : IClassFixture<RedisFixture>
    {
        private readonly RedisFixture _redis;

        public LegacyReaderTests(RedisFixture redis) => this._redis = redis;

        private ConfigurationReader Create(string app) =>
            new ConfigurationReader(app, new ConnectionStringConfigModel { RedisConnectionString = this._redis.ConnectionString }, 200);

        [Fact]
        public void Returns_typed_values()
        {
            var app = RedisFixture.NewApp();
            this._redis.Put(app, "S", "text");
            this._redis.Put(app, "I", "42", "Int32");
            this._redis.Put(app, "B", "true", "Boolean");
            this._redis.Put(app, "D", "1.5", "Double");

            using var reader = this.Create(app);

            Assert.Equal("text", reader.GetValue<string>("S"));
            Assert.Equal(42, reader.GetValue<int>("I"));
            Assert.Equal(42, reader.GetValue<int?>("i"));
            Assert.True(reader.GetValue<bool>("B"));
            Assert.Equal(1.5, reader.GetValue<double>("D"));
        }

        [Fact]
        public void Type_mismatch_missing_and_inactive_return_default()
        {
            var app = RedisFixture.NewApp();
            this._redis.Put(app, "I", "42", "Int32");
            this._redis.Put(app, "Off", "7", "Int32", isActive: false);

            using var reader = this.Create(app);

            Assert.Null(reader.GetValue<string>("I"));
            Assert.Equal(0, reader.GetValue<int>("Missing"));
            Assert.Equal(0, reader.GetValue<int>("Off"));
        }

        [Fact]
        public void Constructor_validates_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationReader("a", null, 100));
            Assert.Throws<ArgumentException>(() => new ConfigurationReader(" ", new ConnectionStringConfigModel(), 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConfigurationReader("a", new ConnectionStringConfigModel(), 0));
        }
    }
}
