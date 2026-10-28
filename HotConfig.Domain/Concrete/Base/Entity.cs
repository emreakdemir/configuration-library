using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Domain.Concrete.Base
{
    public abstract class BaseEntity : IEntity
    {
        public virtual int Id { get; set; }
    }
}