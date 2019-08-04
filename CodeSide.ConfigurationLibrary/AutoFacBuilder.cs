using Autofac;
using CodeSide.Business;
using CodeSide.Business.Base;
using CodeSide.Data.Dapper;
using CodeSide.Data.Dapper.Abstract;
using CodeSide.Domain.Concrete.Model;
using CodeSide.Redis.Abstract;
using CodeSide.Redis.Concrete;

namespace CodeSide.ConfigurationLibrary
{
    internal class AutoFacBuilder
    {
        internal IContainer Container { get; }

        internal AutoFacBuilder(ConnectionStringConfigModel connectionStringConfig, string applicationName)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConfigurationRepository>()
                   .As<IConfigurationRepository>()
                   .WithParameter(new TypedParameter(typeof(string), connectionStringConfig.MySqlConnectionString))
                   .InstancePerDependency();
            builder.RegisterType<ConfigurationBusiness>()
                   .As<IConfigurationBusiness>()
                   .InstancePerDependency();
            builder.RegisterType<RedisHashManager>()
                   .As<IRedisHashManager>()
                   .UsingConstructor(typeof(string), typeof(string))
                   .WithParameters(new[]
                                   {
                                       new NamedParameter("hashKey", applicationName),
                                       new NamedParameter("connectionString", connectionStringConfig.RedisConnectionString)
                                   })
                   .SingleInstance();

            this.Container = builder.Build();
        }

    }
}