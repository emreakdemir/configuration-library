using CodeSide.Domain.Abstract.Entity;
using CodeSide.Domain.Concrete.Base;

namespace CodeSide.Domain.Concrete.Entity
{
    public class Configuration : BaseEntity, IConfiguration
    {
        public string ApplicationName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
    }
}