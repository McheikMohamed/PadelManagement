using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class AnnulationEtDesinscriptionTests
{
    private readonly DatabaseFixture _fixture = new();

    [Fact]
    public async Task AnnulerMatchAsync_RembourseLesJoueursPayesEtPasseLeStatutAAnnule()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);
        var membreRepository = new MembreRepository(context);
        var matchRepository = new MatchRepository(context);
        var paiementRepository = new PaiementRepository(context);

        var siteId = await siteRepository.CreerAsync($"Site Test {Guid.NewGuid()}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_CreerHoraireSite_TestOnly @SiteId = {siteId}, @Annee = {DateTime.Now.Year}, @HeureOuverture = '08:00', @HeureFermeture = '22:00'");
        var terrainId = await terrainRepository.CreerAsync(siteId, 1);

        var organisateur = $"G{Guid.NewGuid().ToString("N")[..8]}";
        await membreRepository.CreerAsync(organisateur, "Global", null);

        var matchId = await matchRepository.CreerReservationAsync(
            terrainId, DateTime.Today.AddDays(10).AddHours(9), organisateur, estPrive: true);

        var inscriptionId = (await context.Database
            .SqlQuery<int>($"EXEC sch_Padel.SP_SelectInscriptionId_TestOnly @MatchId = {matchId}, @MembreMatricule = {organisateur}")
            .ToListAsync())
            .Single();
        await paiementRepository.TraiterPaiementAsync(inscriptionId, 15.00m);

        try
        {
            await matchRepository.AnnulerMatchAsync(matchId, organisateur);

            var match = await matchRepository.ObtenirParIdAsync(matchId);
            Assert.Equal(Padel.Domain.Enums.StatutMatch.Annule, match!.Statut);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMatch_TestOnly @MatchId = {matchId}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {organisateur}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {terrainId}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerHoraireSite_TestOnly @SiteId = {siteId}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId}");
        }
    }
}