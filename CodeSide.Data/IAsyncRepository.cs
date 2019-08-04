using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Data
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