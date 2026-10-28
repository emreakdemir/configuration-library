using System.Collections.Generic;
using System.Threading.Tasks;
using HotConfig.Domain.Concrete.Entity;

namespace HotConfig.Data.Dapper.Abstract
{
    public interface IConfigurationRepository : IAsyncRepository<Configuration>
    {
        new Task<IEnumerable<Configuration>> GetAllAsync();
        Task<IEnumerable<Configuration>> FilterAsync(string applicationName, bool isActive = true);
        /// <summary>Filters by exact application, name substring and active flag; a null filter is ignored.</summary>
        Task<IEnumerable<Configuration>> SearchAsync(string applicationName, string name, bool? isActive);
        new Task<Configuration> GetAsync(int id);
        Task<Configuration> GetAsync(string applicationName, string name, bool isActive = true);
        Task<Configuration> GetAsync(string name);
        new Task AddAsync(Configuration entity);
        /// <summary>Inserts the record and returns its new id.</summary>
        Task<int> InsertAsync(Configuration entity);
        new Task UpdateAsync(Configuration entity);
        new Task RemoveAsync(Configuration entity);
        Task RemoveAsync(int id);
    }
}