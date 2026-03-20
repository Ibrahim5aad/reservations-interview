using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace api.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=InMemory;Mode=Memory;Cache=Shared");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DB registrations
            var descriptors = services
                .Where(d => d.ServiceType == typeof(SqliteConnection) || d.ServiceType == typeof(IDbConnection))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            // Add in-memory connection
            services.AddSingleton(_connection);
            services.AddSingleton<IDbConnection>(_connection);
        });
    }

    protected override void Dispose(bool disposing)
    {
        _connection.Close();
        base.Dispose(disposing);
    }
}
