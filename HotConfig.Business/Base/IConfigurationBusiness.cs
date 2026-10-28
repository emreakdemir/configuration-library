using System.Collections.Generic;
using System.Threading.Tasks;
using HotConfig.Data.Dapper;
using HotConfig.Domain.Concrete.Entity;
using HotConfig.Domain.Concrete.Model;

namespace HotConfig.Business.Base
{
    public interface IConfigurationBusiness : IAsyncBusiness<Configuration, ConfigurationModel, ConfigurationRepository>
    {
        new Task<IEnumerable<ConfigurationModel>> GetAllAsync();
        Task<IEnumerable<ConfigurationModel>> FilterAsync(string applicationName, bool isActive = true);
        new Task<ConfigurationModel> GetAsync(int id);
        Task<ConfigurationModel> GetAsync(string applicationName, string name, bool isActive = true);
        Task<ConfigurationModel> GetAsync(string key);
        new Task AddAsync(ConfigurationModel model);
        new Task UpdateAsync(ConfigurationModel model);
        new Task RemoveAsync(ConfigurationModel model);
        Task RemoveAsync(int id);
    }
}