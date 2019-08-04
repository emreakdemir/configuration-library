using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using CodeSide.Business.Base;
using CodeSide.Domain.Abstract.Base;
using CodeSide.Domain.Concrete.Model;
using CodeSide.Extensions;
using CodeSide.Redis.Abstract;

namespace CodeSide.ConfigurationLibrary
{
    public class ConfigurationReader : IConfigurationReader
    {
        private IRedisHashManager RedisManager { get; }
        private IConfigurationBusiness ConfigurationBusiness { get; }
        private TimeSpan RefreshInterval { get; }
        private string ApplicationName { get; }

        public TType GetValue<TType>(string key)
        {
            var result = default(TType);
            try
            {
                var item = this.RedisManager.Get(key, async () => await this.GetConfiguration(key));
                if (item != null &&  typeof(TType).GetUnderlyingType().Name.ToLower().Equals(item.Model.Type.ToLower()))
                {
                    result = item.Model.Value.ConvertTo<TType>();
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        private async Task<ConfigurationCacheModel> GetConfiguration(string name)
        {
            var configuration = await this.ConfigurationBusiness.GetAsync(this.ApplicationName, name);

            if (configuration == null)
                return null;

            return new ConfigurationCacheModel
                   {
                       Model = configuration,
                       Key = configuration.Name,
                       ExpireDate = DateTime.Now + this.RefreshInterval
                   };
        }

        private async void PrepareConfigurations()
        {
            try
            {
                var activeValues = await this.ConfigurationBusiness.FilterAsync(this.ApplicationName);

                if (activeValues == null)
                    return;

                var configurationModels = activeValues.ToList();
                var expireDate = DateTime.Now + this.RefreshInterval;
                var cacheModels = configurationModels.Select(av => new CacheModel
                                                                   {
                                                                       Model = av,
                                                                       Key = av.Name,
                                                                       ExpireDate = expireDate
                                                                   })
                                                     .ToList();

                this.RedisManager.Set(cacheModels);
            }
            catch (Exception exception)
            {
                //TODO: Log
            }
        }

        /// <summary>
        /// Konfigürasyon tablosunun okunması, önbellek yapısında saklanması ve önbelleğin belli bir süre içerisinde yenilenmesini yönetir.
        /// </summary>
        /// <param name="applicationName">Yüklenecek konfigürasyonların bağlı olduğu proje</param>
        /// <param name="connectionStringConfig">Storage bağlantı bilgileri</param>
        /// <param name="refreshInterval">Önbellek yenilenme süresi. (Ms olarak)</param>
        public ConfigurationReader(string applicationName, ConnectionStringConfigModel connectionStringConfig, int refreshInterval)
        {
            if (connectionStringConfig == null)
                throw new ArgumentNullException(nameof(connectionStringConfig));
            if (connectionStringConfig == null)
                throw new ArgumentNullException(nameof(connectionStringConfig));
            if (string.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(applicationName));
            if (refreshInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(refreshInterval));

            this.ApplicationName = applicationName;
            this.RefreshInterval = TimeSpan.FromMilliseconds(refreshInterval);
            var autoFacBuilder = new AutoFacBuilder(connectionStringConfig, applicationName);
            this.ConfigurationBusiness = autoFacBuilder.Container.Resolve<IConfigurationBusiness>();
            this.RedisManager = autoFacBuilder.Container.Resolve<IRedisHashManager>();
            this.PrepareConfigurations();
        }

    }
}