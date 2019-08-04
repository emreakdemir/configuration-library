using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSide.Business.Base;
using CodeSide.ConfigurationApi.ActionFilters;
using CodeSide.Domain.Concrete.Model;
using Microsoft.AspNetCore.Mvc;

namespace CodeSide.ConfigurationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(OperationLogActionFilter))]
    public class ConfigurationController : ControllerBase
    {
        private IConfigurationBusiness ConfigurationBusiness { get; }

        public ConfigurationController(IConfigurationBusiness configurationBusiness)
        {
            this.ConfigurationBusiness = configurationBusiness;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfigurationModel>>> Get()
        {
            IEnumerable<ConfigurationModel> result;
            try
            {
                result = await this.ConfigurationBusiness.GetAllAsync();
            }
            catch (Exception exception)
            {
                //TODO: Log
                return this.BadRequest(exception);
            }

            return this.Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConfigurationModel>> Get(int id)
        {
            ConfigurationModel result = null;
            try
            {
                result = await this.ConfigurationBusiness.GetAsync(id);
            }
            catch (Exception exception)
            {
                //TODO: Log
                return this.BadRequest(exception);
            }

            return this.Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ConfigurationModel model)
        {
            try
            {
                await this.ConfigurationBusiness.AddAsync(model);
            }
            catch (Exception exception)
            {
                //TODO: Log
                return this.BadRequest(exception);
            }

            return this.Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ConfigurationModel model)
        {
            try
            {
                await this.ConfigurationBusiness.UpdateAsync(model);
            }
            catch (Exception exception)
            {
                //TODO: Log
                return this.BadRequest(exception);
            }

            return this.Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await this.ConfigurationBusiness.RemoveAsync(id);
            }
            catch (Exception exception)
            {
                //TODO: Log
                return this.BadRequest(exception);
            }

            return this.Ok();
        }
    }
}