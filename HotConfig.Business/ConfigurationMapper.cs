using HotConfig.Domain.Concrete.Entity;
using HotConfig.Domain.Concrete.Model;

namespace HotConfig.Business
{
    internal static class ConfigurationMapper
    {
        internal static ConfigurationModel ToModel(this Configuration entity)
        {
            if (entity == null)
                return null;

            return new ConfigurationModel
                   {
                       Id = entity.Id,
                       ApplicationName = entity.ApplicationName,
                       Name = entity.Name,
                       Type = entity.Type,
                       Value = entity.Value,
                       IsActive = entity.IsActive
                   };
        }

        internal static Configuration ToEntity(this ConfigurationModel model)
        {
            if (model == null)
                return null;

            return new Configuration
                   {
                       Id = model.Id,
                       ApplicationName = model.ApplicationName,
                       Name = model.Name,
                       Type = model.Type,
                       Value = model.Value,
                       IsActive = model.IsActive
                   };
        }
    }
}
