using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class PaiementRepositoryTests
{
    private readonly DatabaseFixture _fixture = new();

    private async Task<(int SiteId, int TerrainId, string Matricule, int MatchId, int InscriptionId)>
        ConstruireContexteAvecInscriptionAsync(PadelDbContext context)
    {
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);
        var membreRepository = new MembreRepository(context);
        var matchRepository = new MatchRepository(context);

        var siteId = await siteRepository.CreerAsync($"Site Test {Guid.NewGuid()}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_CreerHoraireSite_TestOnly @SiteId = {siteId}, @Annee = {DateTime.Now.Year}, @HeureOuverture = '08:00', @HeureFermeture = '22:00'");
        var terrainId = await terrainRepository.CreerAsync(siteId, 1);

        var matricule = $"G{Guid.NewGuid().ToString("N")[..8]}";
        await membreRepository.CreerAsync(matricule, "Global", null);

        var matchId = await matchRepository.CreerReservationAsync(
            terrainId, DateTime.Today.AddDays(1).AddHours(9), matricule, estPrive: true);

        var inscriptionId = (await context.Database
            .SqlQuery<int>($"EXEC sch_Padel.SP_SelectInscriptionId_TestOnly @MatchId = {matchId}, @MembreMatricule = {matricule}")
            .ToListAsync())
            .Single();

        return (siteId, terrainId, matricule, matchId, inscriptionId);
    }

    private async Task NettoyerAsync(
        PadelDbContext context,
        int siteId, int terrainId, string matricule, int matchId)
    {
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerMatch_TestOnly @MatchId = {matchId}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {matricule}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {terrainId}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerHoraireSite_TestOnly @SiteId = {siteId}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId}");
    }

    [Fact]
    public async Task TraiterPaiementAsync_CasValide_InsereReellementUnPaiement()
    {
        using var context = _fixture.CreerContext();
        var repository = new PaiementRepository(context);

        var (siteId, terrainId, matricule, matchId, inscriptionId) =
            await ConstruireContexteAvecInscriptionAsync(context);

        try
        {
            var paiementId = await repository.TraiterPaiementAsync(inscriptionId, 15.00m);

            Assert.True(paiementId > 0);
        }
        finally
        {
            await NettoyerAsync(context, siteId, terrainId, matricule, matchId);
        }
    }

    [Fact]
    public async Task TraiterPaiementAsync_InscriptionDejaPayee_LanceRaiserror()
    {
        using var context = _fixture.CreerContext();
        var repository = new PaiementRepository(context);

        var (siteId, terrainId, matricule, matchId, inscriptionId) =
            await ConstruireContexteAvecInscriptionAsync(context);

        await repository.TraiterPaiementAsync(inscriptionId, 15.00m);

        try
        {
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
                () => repository.TraiterPaiementAsync(inscriptionId, 15.00m));
        }
        finally
        {
            await NettoyerAsync(context, siteId, terrainId, matricule, matchId);
        }
    }

    [Fact]
    public async Task RembourserAsync_CasValide_MarqueLePaiementCommeRembourse()
    {
        using var context = _fixture.CreerContext();
        var repository = new PaiementRepository(context);

        var (siteId, terrainId, matricule, matchId, inscriptionId) =
            await ConstruireContexteAvecInscriptionAsync(context);

        var paiementId = await repository.TraiterPaiementAsync(inscriptionId, 15.00m);

        try
        {
            await repository.RembourserAsync(paiementId);
        }
        finally
        {
            await NettoyerAsync(context, siteId, terrainId, matricule, matchId);
        }
    }

    [Fact]
    public async Task RembourserAsync_DejaRembourse_LanceRaiserror()
    {
        using var context = _fixture.CreerContext();
        var repository = new PaiementRepository(context);

        var (siteId, terrainId, matricule, matchId, inscriptionId) =
            await ConstruireContexteAvecInscriptionAsync(context);

        var paiementId = await repository.TraiterPaiementAsync(inscriptionId, 15.00m);
        await repository.RembourserAsync(paiementId);

        try
        {
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
                () => repository.RembourserAsync(paiementId));
        }
        finally
        {
            await NettoyerAsync(context, siteId, terrainId, matricule, matchId);
        }
    }
}