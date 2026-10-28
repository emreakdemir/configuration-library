using System;
using CodeSide.Business;
using CodeSide.Business.Base;
using CodeSide.Data.Dapper;
using CodeSide.Data.Dapper.Abstract;
using CodeSide.Domain.Concrete.Model;
using CodeSide.Redis.Abstract;
using CodeSide.Redis.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace CodeSide.ConfigurationLibrary
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
