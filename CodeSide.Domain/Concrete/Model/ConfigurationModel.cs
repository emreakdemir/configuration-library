using CodeSide.Domain.Abstract.Model;
using CodeSide.Domain.Concrete.Base;

namespace CodeSide.Domain.Concrete.Model
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