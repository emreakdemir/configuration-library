using HotConfig.Api;
using HotConfig.Business;
using HotConfig.Business.Base;
using HotConfig.Data.Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Services resolve their settings lazily from IConfiguration so that hosts which add configuration late (tests) work.
builder.Services.AddSingleton(sp => ApiKeyVerifier.FromConfiguration(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = ConfigurationOptions.Parse(sp.GetRequiredService<IConfiguration>().GetConnectionString("Redis"));
    options.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(options);
});
builder.Services.AddSingleton<ConfigurationCache>();
builder.Services.AddTransient(sp => new ConfigurationRepository(sp.GetRequiredService<IConfiguration>().GetConnectionString("MySql")));
builder.Services.AddScoped<IConfigurationBusiness>(sp => new ConfigurationBusiness(sp.GetRequiredService<ConfigurationRepository>()));
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

// Fail closed: resolving the verifier throws when no API key is configured.
app.Services.GetRequiredService<ApiKeyVerifier>();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.MapAdminApi();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();

/// <summary>Entry point marker so tests can host the app with WebApplicationFactory.</summary>
public partial class Program
{
}
