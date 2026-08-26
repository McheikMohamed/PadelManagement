using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class SiteRepositoryTests
{
    private readonly DatabaseFixture _fixture = new();

    [Fact]
    public async Task CreerAsync_InsereReellementEnBaseEtRetourneUnId()
    {
        using var context = _fixture.CreerContext();
        var repository = new SiteRepository(context);

        var nomUnique = $"Site Test {Guid.NewGuid()}";

        var siteId = await repository.CreerAsync(nomUnique);

        Assert.True(siteId > 0);

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId}");
    }

    [Fact]
    public async Task CreerAsync_AvecNomDejaExistant_LanceSqlException()
    {
        using var context = _fixture.CreerContext();
        var repository = new SiteRepository(context);

        var nomUnique = $"Site Test {Guid.NewGuid()}";
        var premierId = await repository.CreerAsync(nomUnique);

        try
        {
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
                () => repository.CreerAsync(nomUnique));
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {premierId}");
        }
    }

    [Fact]
    public async Task ListerAsync_RetourneLesSitesReellementEnBase()
    {
        using var context = _fixture.CreerContext();
        var repository = new SiteRepository(context);

        var nomUnique = $"Site Test {Guid.NewGuid()}";
        var siteId = await repository.CreerAsync(nomUnique);

        try
        {
            var sites = await repository.ListerAsync();

            Assert.Contains(sites, s => s.SiteId == siteId && s.Nom == nomUnique);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId}");
        }
    }

    [Fact]
    public async Task ObtenirParIdAsync_IdInexistant_RetourneNull()
    {
        using var context = _fixture.CreerContext();
        var repository = new SiteRepository(context);

        var resultat = await repository.ObtenirParIdAsync(999999);

        Assert.Null(resultat);
    }
}