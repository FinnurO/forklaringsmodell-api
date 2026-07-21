using Forklaringsmodell.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Forklaringsmodell.Tests.Integration;

/// <summary>
/// WebApplicationFactory som bytter ut den registrerte SQLite-filbaserte DbContext-en med
/// en isolert in-memory SQLite-database per test-factory-instans, slik at integrasjonstester
/// ikke kolliderer med hverandre eller med utviklerens forklaringsmodell.db.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ForklaringsmodellDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            _connection.Open();

            services.AddDbContext<ForklaringsmodellDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
