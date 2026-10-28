namespace HotConfig.Domain.Abstract.Base
{
    public interface IConfigurationReader
    {
        TType GetValue<TType>(string key);
    }
}