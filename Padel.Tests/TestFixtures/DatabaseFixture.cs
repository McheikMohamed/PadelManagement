using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Padel.Infrastructure;
using Xunit.Sdk;

namespace Padel.Tests.TestFixtures;

public class DatabaseFixture
{
    public PadelDbContext CreerContext()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PadelDbTest");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new SkipException(
                "Aucune chaîne de connexion 'PadelDbTest' n'est configurée. " +
                "Les tests d'intégration base de données sont ignorés.");
        }

        var options = new DbContextOptionsBuilder<PadelDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new PadelDbContext(options);
    }
}