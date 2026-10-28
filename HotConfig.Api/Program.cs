using HotConfig.Business;
using HotConfig.Business.Base;
using HotConfig.Api.ActionFilters;
using HotConfig.Data.Dapper;
using HotConfig.Data.Dapper.Abstract;
using HotConfig.Redis.Abstract;
using HotConfig.Redis.Concrete;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var configurationConnectionString = builder.Configuration.GetConnectionString("ConfigurationConnectionString");
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnectionString");
builder.Services.AddSingleton<IRedisHashManager>(_ => new RedisHashManager("HotConfig.Api", redisConnectionString));
builder.Services.AddScoped<OperationLogActionFilter>();
builder.Services.AddTransient<IConfigurationRepository>(_ => new ConfigurationRepository(configurationConnectionString));
builder.Services.AddScoped<IConfigurationBusiness, ConfigurationBusiness>();

builder.Services.AddCors(options =>
                         {
                             options.AddPolicy(myAllowSpecificOrigins,
                                     policy =>
                                     {
                                         policy.WithOrigins("http://localhost:8080")
                                               .AllowAnyHeader()
                                               .AllowAnyMethod()
                                               .AllowCredentials();
                                     });
                         });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors(myAllowSpecificOrigins);
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
