using AutoMapper;
using CodeSide.Domain.Concrete.Entity;
using CodeSide.Domain.Concrete.Model;

namespace CodeSide.Business
{
    internal static class AutoMapperManager
    {
        internal static IMapper CreateMapper()
        {
            var mapperConfiguration = new MapperConfiguration(config =>
                                                              {
                                                                  config.CreateMap<Configuration, ConfigurationModel>()
                                                                        .ReverseMap();
                                                              });

            return mapperConfiguration.CreateMapper();
        }

    }
}