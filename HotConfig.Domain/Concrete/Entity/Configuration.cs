using HotConfig.Domain.Abstract.Entity;
using HotConfig.Domain.Concrete.Base;

namespace HotConfig.Domain.Concrete.Entity
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