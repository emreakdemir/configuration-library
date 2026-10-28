using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotConfig.Business.Base;
using HotConfig.Data.Dapper.Abstract;
using HotConfig.Domain.Concrete.Model;

namespace HotConfig.Business
{
    /// <remarks>Repository failures propagate to the caller; callers decide whether to tolerate them.</remarks>
    public class ConfigurationBusiness : IConfigurationBusiness
    {
        private IConfigurationRepository Repository { get; }

        public async Task<IEnumerable<ConfigurationModel>> GetAllAsync()
        {
            var configurations = await this.Repository.GetAllAsync();
            return configurations?.Select(c => c.ToModel()).ToList() ?? new List<ConfigurationModel>();
        }

        public async Task<IEnumerable<ConfigurationModel>> FilterAsync(string applicationName, bool isActive = true)
        {
            var configurations = await this.Repository.FilterAsync(applicationName, isActive);
            return configurations?.Select(c => c.ToModel()).ToList() ?? new List<ConfigurationModel>();
        }

        public async Task<IEnumerable<ConfigurationModel>> SearchAsync(string applicationName, string name, bool? isActive)
        {
            var configurations = await this.Repository.SearchAsync(applicationName, name, isActive);
            return configurations?.Select(c => c.ToModel()).ToList() ?? new List<ConfigurationModel>();
        }

        public async Task<ConfigurationModel> GetAsync(int id) => (await this.Repository.GetAsync(id)).ToModel();

        public async Task<ConfigurationModel> GetAsync(string applicationName, string name, bool isActive = true)
            => (await this.Repository.GetAsync(applicationName, name, isActive)).ToModel();

        public async Task<ConfigurationModel> GetAsync(string key) => (await this.Repository.GetAsync(key)).ToModel();

        public Task AddAsync(ConfigurationModel model) => this.Repository.AddAsync(model.ToEntity());

        public Task<int> CreateAsync(ConfigurationModel model) => this.Repository.InsertAsync(model.ToEntity());

        public Task UpdateAsync(ConfigurationModel model) => this.Repository.UpdateAsync(model.ToEntity());

        public Task RemoveAsync(ConfigurationModel model) => this.Repository.RemoveAsync(model.ToEntity());

        public Task RemoveAsync(int id) => this.Repository.RemoveAsync(id);

        public ConfigurationBusiness(IConfigurationRepository configurationRepository)
        {
            this.Repository = configurationRepository;
        }
    }
}
