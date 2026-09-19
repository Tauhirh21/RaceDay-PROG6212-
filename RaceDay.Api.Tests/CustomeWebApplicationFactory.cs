using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceDay.Api.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        // Captured ONCE per factory instance so all HTTP calls in this test class hit the same database
        private readonly string _databaseName = $"RaceDayTestDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var settings = new Dictionary<string, string>
                {
                    {"Jwt:Key", "TestSuperSecretKeyForRaceDayApiThatIsAtLeast32CharsLong!"},
                    {"Jwt:Issuer", "TestIssuer"},
                    {"Jwt:Audience", "TestAudience"},
                    {"Jwt:ExpiryMinutes", "120"}
                };
                config.AddInMemoryCollection(settings!);
            });

            builder.ConfigureServices(services =>
            {
                // Remove all SQL Server DbContext registrations
                var descriptorsToRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(RaceDayDbContext) ||
                    (d.ServiceType.FullName != null && d.ServiceType.FullName.Contains("IDbContextOptions"))
                ).ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Use the SHARED database name (not a new Guid each time)
                services.AddDbContext<RaceDayDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });
            });
        }
    }
}