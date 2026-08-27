using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class PenalitesEtSoldesTests
{
    private readonly DatabaseFixture _fixture = new();

    [Fact]
    public async Task SP_AppliquerPenalitesEtSoldes_MatchPasseAvecUnSeulJoueurPaye_AppliqueSoldeEtPenalite()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);
        var membreRepository = new MembreRepository(context);
        var matchRepository = new MatchRepository(context);
        var paiementRepository = new PaiementRepository(context);

        var siteId = await siteRepository.CreerAsync($"Site Test {Guid.NewGuid()}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_CreerHoraireSite_TestOnly @SiteId = {siteId}, @Annee = {DateTime.Now.Year - 1}, @HeureOuverture = '08:00', @HeureFermeture = '22:00'");
        var terrainId = await terrainRepository.CreerAsync(siteId, 1);

        var organisateur = $"G{Guid.NewGuid().ToString("N")[..8]}";
        await membreRepository.CreerAsync(organisateur, "Global", null);

        // Match déjà passé (l'an dernier), avec seulement l'organisateur inscrit et payé.
        var dateMatch = new DateTime(DateTime.Now.Year - 1, 6, 15, 10, 0, 0);
        var matchId = await matchRepository.CreerReservationAsync(terrainId, dateMatch, organisateur, estPrive: true);

        var inscriptionId = (await context.Database
            .SqlQuery<int>($"EXEC sch_Padel.SP_SelectInscriptionId_TestOnly @MatchId = {matchId}, @MembreMatricule = {organisateur}")
            .ToListAsync())
            .Single();
        await paiementRepository.TraiterPaiementAsync(inscriptionId, 15.00m);

        try
        {
            // Act : simule le passage du job planifié
            await context.Database.ExecuteSqlInterpolatedAsync($"EXEC sch_Padel.SP_AppliquerPenalitesEtSoldes");

            // Assert : 3 places non pourvues × 15€ = 45€ de solde dû
            var membre = await membreRepository.ObtenirParMatriculeAsync(organisateur);
            Assert.Equal(45.00m, membre!.SoldeDu);
            Assert.NotNull(membre.DateProchaineReservationAutorisee);
            Assert.True(membre.DateProchaineReservationAutorisee!.Value >= DateOnly.FromDateTime(DateTime.Today.AddDays(6)));
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

    [Fact]
    public async Task SP_AppliquerPenalitesEtSoldes_MemeMatchAppeleDeuxFois_NAppliquePasDeuxFois()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);
        var membreRepository = new MembreRepository(context);
        var matchRepository = new MatchRepository(context);

        var siteId = await siteRepository.CreerAsync($"Site Test {Guid.NewGuid()}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_CreerHoraireSite_TestOnly @SiteId = {siteId}, @Annee = {DateTime.Now.Year - 1}, @HeureOuverture = '08:00', @HeureFermeture = '22:00'");
        var terrainId = await terrainRepository.CreerAsync(siteId, 1);

        var organisateur = $"G{Guid.NewGuid().ToString("N")[..8]}";
        await membreRepository.CreerAsync(organisateur, "Global", null);

        var dateMatch = new DateTime(DateTime.Now.Year - 1, 7, 1, 10, 0, 0);
        var matchId = await matchRepository.CreerReservationAsync(terrainId, dateMatch, organisateur, estPrive: true);

        try
        {
            // Vérifie le marqueur PenaliteTraitee : un second passage du job ne doit
            // pas doubler le solde déjà appliqué au premier passage.
            await context.Database.ExecuteSqlInterpolatedAsync($"EXEC sch_Padel.SP_AppliquerPenalitesEtSoldes");
            var membreApresPremierPassage = await membreRepository.ObtenirParMatriculeAsync(organisateur);
            var soldeApresPremierPassage = membreApresPremierPassage!.SoldeDu;

            await context.Database.ExecuteSqlInterpolatedAsync($"EXEC sch_Padel.SP_AppliquerPenalitesEtSoldes");
            var membreApresSecondPassage = await membreRepository.ObtenirParMatriculeAsync(organisateur);

            Assert.Equal(soldeApresPremierPassage, membreApresSecondPassage!.SoldeDu);
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