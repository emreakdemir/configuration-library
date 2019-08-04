using System.Collections.Generic;
using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Business.Base
{
    public interface IBusiness
    {
        IEnumerable<IModel> GetAll();
        IModel Get(int id);
        void Add(IModel model);
        void Update(IModel model);
        void Remove(IModel model);
    }
}