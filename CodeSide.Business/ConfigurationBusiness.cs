using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CodeSide.Business.Base;
using CodeSide.Data.Dapper.Abstract;
using CodeSide.Domain.Concrete.Entity;
using CodeSide.Domain.Concrete.Model;

namespace CodeSide.Business
{
    public class ConfigurationBusiness : IConfigurationBusiness
    {
        private IConfigurationRepository Repository { get; }
        private IMapper Mapper { get; }

        public async Task<IEnumerable<ConfigurationModel>> GetAllAsync()
        {
            var result = Enumerable.Empty<ConfigurationModel>();

            try
            {
                var configurations = await this.Repository.GetAllAsync();
                if (configurations != null && configurations.Any())
                {
                    result = this.Mapper.Map<IEnumerable<ConfigurationModel>>(configurations);
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        public async Task<IEnumerable<ConfigurationModel>> FilterAsync(string applicationName, bool isActive = true)
        {
            var result = Enumerable.Empty<ConfigurationModel>();

            try
            {
                var configurations = await this.Repository.FilterAsync(applicationName, isActive);

                if (configurations != null && configurations.Any())
                {
                    result = this.Mapper.Map<IEnumerable<ConfigurationModel>>(configurations);
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        public async Task<ConfigurationModel> GetAsync(int id)
        {
            ConfigurationModel result = null;

            try
            {
                var configuration = await this.Repository.GetAsync(id);
                if (configuration != null)
                {
                    result = this.Mapper.Map<ConfigurationModel>(configuration);
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        public async Task<ConfigurationModel> GetAsync(string applicationName, string name, bool isActive = true)
        {
            ConfigurationModel result = null;

            try
            {
                var configuration = await this.Repository.GetAsync(applicationName, name);
                if (configuration != null)
                {
                    result = this.Mapper.Map<ConfigurationModel>(configuration);
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        public async Task<ConfigurationModel> GetAsync(string key)
        {
            ConfigurationModel result = null;

            try
            {
                var configuration = await this.Repository.GetAsync(key);
                if (configuration != null)
                {
                    result = this.Mapper.Map<ConfigurationModel>(configuration);
                }
            }
            catch (Exception exception)
            {
                //TODO: Log
            }

            return result;
        }

        public async Task AddAsync(ConfigurationModel model)
        {
            try
            {
                var entity = this.Mapper.Map<Configuration>(model);
                await this.Repository.AddAsync(entity);
            }
            catch (Exception exception)
            {
                //TODO: LOG
            }
        }

        public async Task UpdateAsync(ConfigurationModel model)
        {
            try
            {
                var entity = this.Mapper.Map<Configuration>(model);
                await this.Repository.UpdateAsync(entity);
            }
            catch (Exception exception)
            {
                //TODO: LOG
            }
        }

        public async Task RemoveAsync(ConfigurationModel model)
        {
            try
            {
                var entity = this.Mapper.Map<Configuration>(model);
                await this.Repository.RemoveAsync(entity);
            }
            catch (Exception exception)
            {
                //TODO: LOG
            }
        }

        public async Task RemoveAsync(int id)
        {
            try
            {
                await this.Repository.RemoveAsync(id);
            }
            catch (Exception exception)
            {
                //TODO: LOG
            }
        }

        public ConfigurationBusiness(IConfigurationRepository configurationRepository)
        {
            this.Repository = configurationRepository;
            this.Mapper = AutoMapperManager.CreateMapper();
        }
    }
}