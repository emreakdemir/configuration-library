using HotConfig.Domain.Abstract.Model;
using HotConfig.Domain.Concrete.Base;

namespace HotConfig.Domain.Concrete.Model
{
    public class ConfigurationModel : BaseModel, IConfigurationModel
    {
        public string ApplicationName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
    }
}