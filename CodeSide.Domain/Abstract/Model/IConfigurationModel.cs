using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Domain.Abstract.Model
{
    public interface IConfigurationModel : IModel
    {
        string ApplicationName { get; set; }
        string Name { get; set; }
        string Type { get; set; }
        string Value { get; set; }
        bool IsActive { get; set; }
    }
}