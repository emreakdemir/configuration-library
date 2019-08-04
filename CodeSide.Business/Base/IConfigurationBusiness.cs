using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSide.Data.Dapper;
using CodeSide.Domain.Concrete.Entity;
using CodeSide.Domain.Concrete.Model;

namespace CodeSide.Business.Base
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