using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Domain.Concrete.Base
{
    public class BaseModel : IModel
    {
        public virtual int Id { get; set; }
    }
}