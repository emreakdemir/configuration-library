using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSide.Domain.Concrete.Entity;

namespace CodeSide.Data.Dapper.Abstract
{
    public interface IConfigurationRepository : IAsyncRepository<Configuration>
    {
        new Task<IEnumerable<Configuration>> GetAllAsync();
        Task<IEnumerable<Configuration>> FilterAsync(string applicationName, bool isActive = true);
        new Task<Configuration> GetAsync(int id);
        Task<Configuration> GetAsync(string applicationName, string name, bool isActive = true);
        Task<Configuration> GetAsync(string name);
        new Task AddAsync(Configuration entity);
        new Task UpdateAsync(Configuration entity);
        new Task RemoveAsync(Configuration entity);
        Task RemoveAsync(int id);
    }
}