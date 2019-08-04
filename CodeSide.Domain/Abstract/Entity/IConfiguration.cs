using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Domain.Abstract.Entity
{
    public interface IConfiguration : IEntity
    {
        string ApplicationName { get; set; }
        string Name { get; set; }
        string Type { get; set; }
        string Value { get; set; }
        bool IsActive { get; set; }
    }
}