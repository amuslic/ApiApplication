using ApiApplication.Api.Utils;
using ApiApplication.Application;
using ApiApplication.Application.Abstractions;
using ApiApplication.Application.Commands;
using ApiApplication.Application.Configuration;
using ApiApplication.Database;
using ApiApplication.Database.Repositories;
using ApiApplication.Database.Repositories.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ApiApplication
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
            services.AddScoped<IExternalMovieApiProxy, ExternalMovieApiProxy>();
            services.Decorate<IExternalMovieApiProxy, CachedMovieApiProxyDecorator>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<ISystemTime, SystemTime>();

            services.AddTransient<IShowtimesRepository, ShowtimesRepository>();
            services.AddTransient<ITicketsRepository, TicketsRepository>();
            services.AddTransient<IAuditoriumsRepository, AuditoriumsRepository>();

            services.AddDbContext<CinemaContext>(options =>
            {
                options.UseInMemoryDatabase("CinemaDb")
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ApiExceptionFilter));
            });

            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(typeof(Startup).Assembly);
            });
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            services.Configure<RedisConfiguration>(Configuration.GetSection(RedisConfiguration.ConfigurationKey));
            services.Configure<ExternalMovieProviderConfiguration>(Configuration.GetSection(ExternalMovieProviderConfiguration.ConfigurationKey));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
 
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });        

            app.UseSwagger();

            var seedData = Configuration.GetValue<bool>("SeedSampleData");
            if (seedData)
            {
                SampleData.Initialize(app);
            }
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        }
    }
}
