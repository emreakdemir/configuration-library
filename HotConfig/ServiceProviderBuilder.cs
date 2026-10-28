using System;
using HotConfig.Business;
using HotConfig.Business.Base;
using HotConfig.Data.Dapper;
using HotConfig.Data.Dapper.Abstract;
using HotConfig.Domain.Concrete.Model;
using HotConfig.Redis.Abstract;
using HotConfig.Redis.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace HotConfig
{
    internal class ServiceProviderBuilder
    {
        internal IServiceProvider ServiceProvider { get; }

        internal ServiceProviderBuilder(ConnectionStringConfigModel connectionStringConfig, string applicationName)
        {
            var services = new ServiceCollection();

            services.AddTransient<IConfigurationRepository>(_ => new ConfigurationRepository(connectionStringConfig.MySqlConnectionString));
            services.AddTransient<IConfigurationBusiness, ConfigurationBusiness>();
            services.AddSingleton<IRedisHashManager>(_ => new RedisHashManager(applicationName, connectionStringConfig.RedisConnectionString));

            this.ServiceProvider = services.BuildServiceProvider();
        }

    }
}
