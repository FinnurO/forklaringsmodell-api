using Forklaringsmodell.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Forklaringsmodell.Tests;

/// <summary>
/// Lager en isolert SQLite in-memory-database per test (åpen forbindelse holdes i live
/// gjennom hele testens levetid, slik at skjemaet ikke forsvinner mellom kall). Brukes av
/// service-nivå-tester som ikke går via WebApplicationFactory. Implementerer IDisposable slik
/// at "using var db = TestDbContextFactory.Create();" rydder opp baade DbContext og
/// SqliteConnection naar testen er ferdig.
/// </summary>
public static class TestDbContextFactory
{
    public static TestDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ForklaringsmodellDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ForklaringsmodellDbContext(options);
        context.Database.EnsureCreated();

        return new TestDbContext(context, connection);
    }
}

public sealed class TestDbContext : IDisposable
{
    public ForklaringsmodellDbContext Context { get; }
    private readonly SqliteConnection _connection;

    public TestDbContext(ForklaringsmodellDbContext context, SqliteConnection connection)
    {
        Context = context;
        _connection = connection;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
