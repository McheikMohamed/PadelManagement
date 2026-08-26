using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Padel.Infrastructure;

namespace Padel.Tests.TestFixtures;

public class DatabaseFixture
{
    public PadelDbContext CreerContext()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json")
            .Build();

        var connectionString = configuration.GetConnectionString("PadelDbTest");

        var options = new DbContextOptionsBuilder<PadelDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new PadelDbContext(options);
    }
}