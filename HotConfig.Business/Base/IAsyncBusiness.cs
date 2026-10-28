using System.Collections.Generic;
using System.Threading.Tasks;
using HotConfig.Data;
using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Business.Base
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