using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Domain.Concrete.Base
{
    public class BaseModel : IModel
    {
        public virtual int Id { get; set; }
    }
}