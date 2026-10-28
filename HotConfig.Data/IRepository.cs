using System.Collections.Generic;
using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Data
{
    public interface IRepository<TEntity> where TEntity : IEntity
    {
        IEnumerable<TEntity> GetAll();
        TEntity Get(int id);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);

    }
}