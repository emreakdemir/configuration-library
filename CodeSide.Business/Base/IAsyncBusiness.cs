using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSide.Data;
using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Business.Base
{
    public interface IAsyncBusiness<TEntity, TModel, TRepository>
            where TModel : IModel
            where TEntity : IEntity
            where TRepository : IAsyncRepository<TEntity>
    {
        Task<IEnumerable<TModel>> GetAllAsync();
        Task<TModel> GetAsync(int id);
        Task AddAsync(TModel model);
        Task UpdateAsync(TModel model);
        Task RemoveAsync(TModel model);
    }
}