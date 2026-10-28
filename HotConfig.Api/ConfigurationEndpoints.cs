using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotConfig.Business.Base;
using HotConfig.Domain.Concrete.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using MySqlConnector;

namespace HotConfig.Api
{
    public static class ConfigurationEndpoints
    {
        public static RouteGroupBuilder MapAdminApi(this IEndpointRouteBuilder routes)
        {
            var api = routes.MapGroup("/api");

            var configurations = api.MapGroup("/configurations").WithTags("Configurations");
            configurations.MapGet("/", List).WithName("ListConfigurations");
            configurations.MapGet("/{id:int}", Get).WithName("GetConfiguration");
            configurations.MapPost("/", Create).WithName("CreateConfiguration");
            configurations.MapPut("/{id:int}", Update).WithName("UpdateConfiguration");
            configurations.MapDelete("/{id:int}", Delete).WithName("DeleteConfiguration");
            configurations.MapPost("/{id:int}/toggle", Toggle).WithName("ToggleConfiguration");

            api.MapPost("/applications/{applicationName}/sync", Sync).WithTags("Applications").WithName("SyncApplication");
            return api;
        }

        private static async Task<Ok<List<ConfigurationModel>>> List(IConfigurationBusiness business, string applicationName, string name, bool? isActive)
        {
            var records = await business.SearchAsync(applicationName, name, isActive);
            return TypedResults.Ok(records.ToList());
        }

        private static async Task<Results<Ok<ConfigurationModel>, ProblemHttpResult>> Get(IConfigurationBusiness business, int id)
        {
            var record = await business.GetAsync(id);
            return record == null ? NotFound(id) : TypedResults.Ok(record);
        }

        private static async Task<Results<Created<ConfigurationModel>, ValidationProblem, ProblemHttpResult>> Create(
                IConfigurationBusiness business, ConfigurationCache cache, ConfigurationRequest request)
        {
            if (Validate(request, out var type) is { } invalid)
                return invalid;

            var record = new ConfigurationModel
                         {
                             ApplicationName = request.ApplicationName.Trim(),
                             Name = request.Name.Trim(),
                             Type = type,
                             Value = request.Value,
                             IsActive = request.IsActive ?? true
                         };

            try
            {
                record.Id = await business.CreateAsync(record);
            }
            catch (MySqlException exception) when (exception.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                return Conflict(record);
            }

            await cache.ApplyAsync(record);
            return TypedResults.Created($"/api/configurations/{record.Id}", record);
        }

        private static async Task<Results<Ok<ConfigurationModel>, ValidationProblem, ProblemHttpResult>> Update(
                IConfigurationBusiness business, ConfigurationCache cache, int id, ConfigurationRequest request)
        {
            if (Validate(request, out var type) is { } invalid)
                return invalid;

            var previous = await business.GetAsync(id);
            if (previous == null)
                return NotFound(id);

            var record = new ConfigurationModel
                         {
                             Id = id,
                             ApplicationName = request.ApplicationName.Trim(),
                             Name = request.Name.Trim(),
                             Type = type,
                             Value = request.Value,
                             IsActive = request.IsActive ?? previous.IsActive
                         };

            try
            {
                await business.UpdateAsync(record);
            }
            catch (MySqlException exception) when (exception.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                return Conflict(record);
            }

            await ReplaceInCache(cache, previous, record);
            return TypedResults.Ok(record);
        }

        private static async Task<Results<NoContent, ProblemHttpResult>> Delete(IConfigurationBusiness business, ConfigurationCache cache, int id)
        {
            var record = await business.GetAsync(id);
            if (record == null)
                return NotFound(id);

            await business.RemoveAsync(id);
            await cache.RemoveAsync(record);
            return TypedResults.NoContent();
        }

        private static async Task<Results<Ok<ConfigurationModel>, ProblemHttpResult>> Toggle(IConfigurationBusiness business, ConfigurationCache cache, int id)
        {
            var record = await business.GetAsync(id);
            if (record == null)
                return NotFound(id);

            record.IsActive = !record.IsActive;
            await business.UpdateAsync(record);
            await cache.ApplyAsync(record);
            return TypedResults.Ok(record);
        }

        private static async Task<Ok<SyncResult>> Sync(IConfigurationBusiness business, ConfigurationCache cache, string applicationName)
        {
            var active = (await business.FilterAsync(applicationName)).ToList();
            var (written, removed) = await cache.ReplaceAsync(applicationName, active);
            return TypedResults.Ok(new SyncResult(applicationName, written, removed));
        }

        private static async Task ReplaceInCache(ConfigurationCache cache, ConfigurationModel previous, ConfigurationModel record)
        {
            // A rename or move leaves the old field behind unless it is removed explicitly.
            if (!string.Equals(previous.ApplicationName, record.ApplicationName, StringComparison.Ordinal)
                || !string.Equals(previous.Name, record.Name, StringComparison.OrdinalIgnoreCase))
                await cache.RemoveAsync(previous);

            await cache.ApplyAsync(record);
        }

        private static ValidationProblem Validate(ConfigurationRequest request, out string type)
        {
            type = null;
            if (request == null)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [""] = new[] { "A JSON request body is required." } });

            var errors = request.Validate(out type);
            return errors.Count == 0 ? null : TypedResults.ValidationProblem(errors);
        }

        private static ProblemHttpResult NotFound(int id)
            => TypedResults.Problem(title: "Not found", detail: $"Configuration {id} does not exist.", statusCode: StatusCodes.Status404NotFound);

        private static ProblemHttpResult Conflict(ConfigurationModel record)
            => TypedResults.Problem(title: "Conflict",
                                    detail: $"Application '{record.ApplicationName}' already has a record named '{record.Name}'.",
                                    statusCode: StatusCodes.Status409Conflict);

        public sealed record SyncResult(string ApplicationName, int Written, int Removed);
    }
}
