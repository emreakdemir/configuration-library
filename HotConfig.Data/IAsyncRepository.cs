using System.Collections.Generic;
using System.Threading.Tasks;
using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Data
{
    public interface IAsyncRepository<TEntity> where TEntity : IEntity
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> GetAsync(int id);
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task RemoveAsync(TEntity entity);
    }
}