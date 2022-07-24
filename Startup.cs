using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace HealthCheck
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Configuring CORS
            services.AddCors();
            
            // Add Sql Server Health Check 
            services.AddHealthChecks()
            .AddSqlServer(
                connectionString: Mapping.SqlConnectionString.GetConnectionString(), 
                name: "Sql Server instance",
                tags: new[] { "database" }
            ).AddCheck<CustomHealthChecks>("Custom Health Checks", tags: new[] { "custom" });
            
            // Add Health Check UI
            services.AddHealthChecksUI().AddInMemoryStorage();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HealthCheck", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthCheck v1"));
            }
            
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseRouting();            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI();

                // Route to all Health Checks
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = p => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                // Route to filter custom Health Check
                endpoints.MapHealthChecks("/health/custom", new HealthCheckOptions
                {
                    Predicate = reg => reg.Tags.Contains("custom"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapControllers();                
            });
            
            // Route to acess health check UI
            app.UseHealthChecksUI(options => { options.UIPath = "/dashboard"; });
        }
    }
}
