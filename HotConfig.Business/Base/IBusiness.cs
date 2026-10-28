using System.Collections.Generic;
using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Business.Base
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