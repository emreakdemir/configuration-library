using CodeSide.Business;
using CodeSide.Business.Base;
using CodeSide.ConfigurationApi.ActionFilters;
using CodeSide.Data.Dapper;
using CodeSide.Data.Dapper.Abstract;
using CodeSide.Redis.Abstract;
using CodeSide.Redis.Concrete;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeSide.ConfigurationApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        readonly string _myAllowSpecificOrigins = "_myAllowSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var configurationConnectionString = this.Configuration.GetConnectionString("ConfigurationConnectionString");
            var redisConnectionString = this.Configuration.GetConnectionString("RedisConnectionString");
            services.AddSingleton<IRedisHashManager>(hashManager => new RedisHashManager("CodeSide.ConfigurationApi", redisConnectionString));
            services.AddScoped<OperationLogActionFilter>();
            services.AddTransient<IConfigurationRepository>(repository => new ConfigurationRepository(configurationConnectionString));
            services.AddScoped(typeof(IConfigurationBusiness), typeof(ConfigurationBusiness));

            services.AddCors(options =>
                             {
                                 options.AddPolicy(this._myAllowSpecificOrigins,
                                         builder =>
                                         {
                                             builder.WithOrigins("http://localhost:8080")
                                                    .AllowAnyHeader()
                                                    .AllowAnyMethod()
                                                    .AllowCredentials();
                                         });
                             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(this._myAllowSpecificOrigins);
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}