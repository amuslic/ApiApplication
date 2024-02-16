using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ApiApplicationIntegrationTests.Utils
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var projectDir = AppDomain.CurrentDomain.BaseDirectory;
                var configPath = Path.Combine(projectDir, "appsettings.Test.json");

                config.AddJsonFile(configPath, optional: false);
            });

            builder.UseEnvironment("Test"); 
        }
    }
}
