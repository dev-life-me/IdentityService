using Application.Interfaces;
using Infrastructure.Persistence.AppDbContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace IntergrationTest.Configs
{
    public class CustomeWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        public CustomeWebApplicationFactory(string connectionstring)
        {
            
            _connectionString = connectionstring;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTest");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<IdentityServiceDbContext>));
                
                services.AddDbContext<IdentityServiceDbContext>(options =>
                {
                    options.UseNpgsql(_connectionString);
                });
            });
        }
    }
}
