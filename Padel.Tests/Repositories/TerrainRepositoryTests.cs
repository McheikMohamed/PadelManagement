using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class TerrainRepositoryTests
{
    private readonly DatabaseFixture _fixture = new();

    private async Task<int> CreerSiteDeTestAsync(SiteRepository siteRepository)
    {
        return await siteRepository.CreerAsync($"Site Test {Guid.NewGuid()}");
    }

    [Fact]
    public async Task CreerAsync_InsereReellementEnBaseEtRetourneUnId()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);

        var siteId = await CreerSiteDeTestAsync(siteRepository);

        try
        {
            var terrainId = await terrainRepository.CreerAsync(siteId, 1);

            Assert.True(terrainId > 0);

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {terrainId}");
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId}");
        }
    }

    [Fact]
    public async Task CreerAsync_MemeNumeroMemeSite_LanceSqlException()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);

        var siteId = await CreerSiteDeTestAsync(siteRepository);
        var premierTerrainId = await terrainRepository.CreerAsync(siteId, 1);

        try
        {
            // Vérifie la vraie contrainte UK_Terrains_SiteId_Numero (Issue #7)
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
                () => terrainRepository.CreerAsync(siteId, 1));
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {premierTerrainId}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId}");
        }
    }

    [Fact]
    public async Task ListerParSiteAsync_RetourneUniquementLesTerrainsDuSite()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);

        var siteId1 = await CreerSiteDeTestAsync(siteRepository);
        var siteId2 = await CreerSiteDeTestAsync(siteRepository);

        var terrainSite1 = await terrainRepository.CreerAsync(siteId1, 1);
        var terrainSite2 = await terrainRepository.CreerAsync(siteId2, 1);

        try
        {
            var terrainsDuSite1 = await terrainRepository.ListerParSiteAsync(siteId1);

            Assert.Single(terrainsDuSite1);
            Assert.Equal(terrainSite1, terrainsDuSite1[0].TerrainId);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {terrainSite1}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {terrainSite2}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId1}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId2}");
        }
    }

    [Fact]
    public async Task ObtenirParIdAsync_IdInexistant_RetourneNull()
    {
        using var context = _fixture.CreerContext();
        var terrainRepository = new TerrainRepository(context);

        var resultat = await terrainRepository.ObtenirParIdAsync(999999);

        Assert.Null(resultat);
    }
}