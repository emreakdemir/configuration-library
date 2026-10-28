using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HotConfig.Api;
using HotConfig.Domain.Concrete.Model;
using HotConfig.Extensions;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Xunit;

namespace HotConfig.Tests
{
    public class AdminApiTests : IClassFixture<ApiFixture>
    {
        private readonly ApiFixture _api;
        private readonly HttpClient _client;

        public AdminApiTests(ApiFixture api)
        {
            this._api = api;
            this._client = api.CreateAuthorizedClient();
        }

        private static object Body(string app, string name, string type = "String", string value = "v", bool? isActive = null)
            => new { applicationName = app, name, type, value, isActive };

        private async Task<ConfigurationModel> Create(string app, string name, string type = "String", string value = "v", bool? isActive = null)
        {
            var response = await this._client.PostAsJsonAsync("/api/configurations", Body(app, name, type, value, isActive));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await Read(response);
        }

        private static async Task<ConfigurationModel> Read(HttpResponseMessage response)
            => (await response.Content.ReadFromJsonAsync<ConfigurationModel>(new JsonSerializerOptions(JsonSerializerDefaults.Web)))!;

        private async Task<HashEntry[]> Hash(string app) => await this._api.Redis.HashGetAllAsync(app);

        private static async Task<JsonElement> Json(HttpResponseMessage response) => await response.Content.ReadFromJsonAsync<JsonElement>();

        [Theory]
        [InlineData(null)]
        [InlineData("wrong-key")]
        public async Task Api_rejects_a_missing_or_wrong_key(string key)
        {
            using var client = this._api.CreateClient();
            if (key != null)
                client.DefaultRequestHeaders.Add("X-Api-Key", key);

            foreach (var response in new[]
                                     {
                                         await client.GetAsync("/api/configurations"),
                                         await client.PostAsync("/api/configurations", null),
                                         await client.PostAsync("/api/applications/x/sync", null)
                                     })
            {
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.Equal(401, (await Json(response)).GetProperty("status").GetInt32());
            }
        }

        [Fact]
        public async Task Health_needs_no_key()
        {
            using var client = this._api.CreateClient();
            var response = await client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void Api_refuses_to_start_without_a_key()
        {
            var empty = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { ["HotConfig:ApiKey"] = " " }).Build();
            var exception = Assert.Throws<InvalidOperationException>(() => ApiKeyVerifier.FromConfiguration(empty));
            Assert.Contains("HotConfig:ApiKey", exception.Message);
        }

        [Fact]
        public async Task Create_writes_the_record_to_redis_and_the_provider_reads_it()
        {
            var app = ApiFixture.NewApp();
            var created = await this.Create(app, "Max:Items", "int", "42");

            Assert.True(created.Id > 0);
            Assert.Equal("Int32", created.Type);

            var hash = await this.Hash(app);
            var entry = Assert.Single(hash);
            Assert.Equal("max:items", entry.Name.ToString());
            var cached = ((string) entry.Value).FromJson<ConfigurationCacheModel>().Model;
            Assert.Equal(created.Id, cached.Id);
            Assert.Equal("42", cached.Value);

            var source = new HotConfigConfigurationSource(new HotConfigOptions
                                                          {
                                                              ApplicationName = app,
                                                              Redis = this._api.RedisConnectionString,
                                                              RefreshInterval = TimeSpan.FromMinutes(1)
                                                          });
            var configuration = new ConfigurationBuilder().Add(source).Build();
            Assert.Equal("42", configuration["Max:Items"]);
            ((IDisposable) ((IConfigurationRoot) configuration).Providers.Single()).Dispose();
        }

        [Fact]
        public async Task Inactive_create_stays_out_of_redis()
        {
            var app = ApiFixture.NewApp();
            await this.Create(app, "Off", isActive: false);
            Assert.Empty(await this.Hash(app));
        }

        [Fact]
        public async Task Toggle_removes_and_restores_the_redis_field()
        {
            var app = ApiFixture.NewApp();
            var created = await this.Create(app, "Flag", "bool", "true");

            var off = await this._client.PostAsync($"/api/configurations/{created.Id}/toggle", null);
            Assert.False((await Read(off)).IsActive);
            Assert.Empty(await this.Hash(app));

            var on = await this._client.PostAsync($"/api/configurations/{created.Id}/toggle", null);
            Assert.True((await Read(on)).IsActive);
            Assert.Single(await this.Hash(app));
        }

        [Fact]
        public async Task Update_changes_mysql_and_moves_the_redis_field_on_rename()
        {
            var app = ApiFixture.NewApp();
            var created = await this.Create(app, "Old", value: "a");

            var response = await this._client.PutAsJsonAsync($"/api/configurations/{created.Id}", Body(app, "New", "string", "b"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var fetched = await Read(await this._client.GetAsync($"/api/configurations/{created.Id}"));
            Assert.Equal("New", fetched.Name);
            Assert.Equal("b", fetched.Value);
            Assert.True(fetched.IsActive);

            var entry = Assert.Single(await this.Hash(app));
            Assert.Equal("new", entry.Name.ToString());
            Assert.Equal("b", ((string) entry.Value).FromJson<ConfigurationCacheModel>().Model.Value);
        }

        [Fact]
        public async Task Update_moving_to_another_application_cleans_the_old_hash()
        {
            var first = ApiFixture.NewApp();
            var second = ApiFixture.NewApp();
            var created = await this.Create(first, "Key");

            await this._client.PutAsJsonAsync($"/api/configurations/{created.Id}", Body(second, "Key"));

            Assert.Empty(await this.Hash(first));
            Assert.Single(await this.Hash(second));
        }

        [Fact]
        public async Task Delete_removes_the_record_everywhere()
        {
            var app = ApiFixture.NewApp();
            var created = await this.Create(app, "Gone");

            var response = await this._client.DeleteAsync($"/api/configurations/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(await this.Hash(app));
            Assert.Equal(HttpStatusCode.NotFound, (await this._client.GetAsync($"/api/configurations/{created.Id}")).StatusCode);
        }

        [Fact]
        public async Task List_filters_by_application_name_and_active_flag()
        {
            var app = ApiFixture.NewApp();
            await this.Create(app, "Alpha.Timeout");
            await this.Create(app, "Beta.Timeout", isActive: false);
            await this.Create(app, "Gamma_Retry");

            async Task<string[]> Names(string query)
                => (await this._client.GetFromJsonAsync<List<ConfigurationModel>>($"/api/configurations?applicationName={app}{query}"))!
                   .Select(r => r.Name).ToArray();

            Assert.Equal(new[] { "Alpha.Timeout", "Beta.Timeout", "Gamma_Retry" }, await Names(""));
            Assert.Equal(new[] { "Alpha.Timeout", "Beta.Timeout" }, await Names("&name=timeout"));
            Assert.Equal(new[] { "Alpha.Timeout", "Gamma_Retry" }, await Names("&isActive=true"));
            Assert.Equal(new[] { "Beta.Timeout" }, await Names("&isActive=false"));
            Assert.Empty(await Names("&name=Gamma.Retry")); // '_' is a literal, not a wildcard
        }

        [Fact]
        public async Task Invalid_requests_get_a_400_problem_details_response()
        {
            var response = await this._client.PostAsJsonAsync("/api/configurations", new { applicationName = "", name = "n", type = "Guid", value = "" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errors = (await Json(response)).GetProperty("errors");
            Assert.True(errors.TryGetProperty("ApplicationName", out _));
            Assert.True(errors.TryGetProperty("Type", out _));
            Assert.True(errors.TryGetProperty("Value", out _));
            Assert.False(errors.TryGetProperty("Name", out _));
        }

        [Fact]
        public async Task A_value_that_does_not_fit_the_type_is_rejected()
        {
            var response = await this._client.PostAsJsonAsync("/api/configurations", Body(ApiFixture.NewApp(), "n", "int", "abc"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True((await Json(response)).GetProperty("errors").TryGetProperty("Value", out _));
        }

        [Fact]
        public async Task Unknown_ids_return_404_problem_details()
        {
            var responses = new[]
                            {
                                await this._client.GetAsync("/api/configurations/999999"),
                                await this._client.PutAsJsonAsync("/api/configurations/999999", Body("a", "n")),
                                await this._client.DeleteAsync("/api/configurations/999999"),
                                await this._client.PostAsync("/api/configurations/999999/toggle", null)
                            };

            foreach (var response in responses)
            {
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                Assert.Equal(404, (await Json(response)).GetProperty("status").GetInt32());
            }
        }

        [Fact]
        public async Task Duplicate_name_in_one_application_returns_409()
        {
            var app = ApiFixture.NewApp();
            await this.Create(app, "Dup");

            var response = await this._client.PostAsJsonAsync("/api/configurations", Body(app, "dup"));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Sync_rewrites_the_hash_from_mysql_and_drops_stale_fields()
        {
            var app = ApiFixture.NewApp();
            var keep = await this.Create(app, "Keep", value: "1");
            var lost = await this.Create(app, "Lost", value: "2");
            await this.Create(app, "Inactive", isActive: false);

            var db = this._api.Redis;
            await db.HashDeleteAsync(app, "lost");
            await db.HashSetAsync(app, "keep", "{\"stale\":true}");
            await db.HashSetAsync(app, "ghost", "{}");

            var response = await this._client.PostAsync($"/api/applications/{app}/sync", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await Json(response);
            Assert.Equal(2, body.GetProperty("written").GetInt32());
            Assert.Equal(1, body.GetProperty("removed").GetInt32());

            var hash = (await this.Hash(app)).ToDictionary(e => e.Name.ToString(), e => (string) e.Value);
            Assert.Equal(new[] { "keep", "lost" }, hash.Keys.OrderBy(k => k).ToArray());
            Assert.Equal(keep.Id, hash["keep"].FromJson<ConfigurationCacheModel>().Model.Id);
            Assert.Equal(lost.Id, hash["lost"].FromJson<ConfigurationCacheModel>().Model.Id);
        }

        [Fact]
        public async Task Root_serves_the_admin_page_without_a_key()
        {
            using var client = this._api.CreateClient();
            var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("HotConfig admin", await response.Content.ReadAsStringAsync());
        }
    }
}
