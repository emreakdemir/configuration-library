namespace CodeSide.Domain.Abstract.Base
{
    public interface IConfigurationReader
    {
        TType GetValue<TType>(string key);
    }
}