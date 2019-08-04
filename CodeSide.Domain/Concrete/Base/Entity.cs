using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Domain.Concrete.Base
{
    public abstract class BaseEntity : IEntity
    {
        public virtual int Id { get; set; }
    }
}