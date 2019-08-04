using System.Collections.Generic;
using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Data
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