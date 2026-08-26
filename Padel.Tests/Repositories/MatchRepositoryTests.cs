using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class MatchRepositoryTests
{
    private readonly DatabaseFixture _fixture = new();

    private record ContexteDeTest(int SiteId, int TerrainId, string MembreMatricule);

    private async Task<ContexteDeTest> ConstruireContexteAsync(
        Microsoft.EntityFrameworkCore.DbContext context,
        SiteRepository siteRepository,
        TerrainRepository terrainRepository,
        MembreRepository membreRepository)
    {
        var siteId = await siteRepository.CreerAsync($"Site Test {Guid.NewGuid()}");

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_CreerHoraireSite_TestOnly @SiteId = {siteId}, @Annee = {DateTime.Now.Year}, @HeureOuverture = '08:00', @HeureFermeture = '22:00'");

        var terrainId = await terrainRepository.CreerAsync(siteId, 1);

        var matricule = $"G{Guid.NewGuid().ToString("N")[..8]}";
        await membreRepository.CreerAsync(matricule, "Global", null);

        return new ContexteDeTest(siteId, terrainId, matricule);
    }

    private async Task NettoyerContexteAsync(
        Microsoft.EntityFrameworkCore.DbContext context,
        ContexteDeTest ctx,
        int? matchId = null)
    {
        if (matchId.HasValue)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMatch_TestOnly @MatchId = {matchId.Value}");
        }

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {ctx.MembreMatricule}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {ctx.TerrainId}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerHoraireSite_TestOnly @SiteId = {ctx.SiteId}");
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {ctx.SiteId}");
    }

    [Fact]
    public async Task CreerReservationAsync_CasValide_CreeReellementUnMatch()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);
        var membreRepository = new MembreRepository(context);
        var matchRepository = new MatchRepository(context);

        var ctx = await ConstruireContexteAsync(context, siteRepository, terrainRepository, membreRepository);
        int? matchId = null;

        try
        {
            var dateReservation = DateTime.Today.AddDays(1).AddHours(10);

            matchId = await matchRepository.CreerReservationAsync(
                ctx.TerrainId, dateReservation, ctx.MembreMatricule, estPrive: true);

            Assert.True(matchId > 0);

            var match = await matchRepository.ObtenirParIdAsync(matchId.Value);
            Assert.NotNull(match);
            Assert.Equal(ctx.MembreMatricule, match!.OrganisateurMatricule);
        }
        finally
        {
            await NettoyerContexteAsync(context, ctx, matchId);
        }
    }

    [Fact]
    public async Task CreerReservationAsync_CreneauDejaOccupe_LanceRaiserror()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);
        var membreRepository = new MembreRepository(context);
        var matchRepository = new MatchRepository(context);

        var ctx = await ConstruireContexteAsync(context, siteRepository, terrainRepository, membreRepository);
        var dateReservation = DateTime.Today.AddDays(1).AddHours(10);
        var matchId = await matchRepository.CreerReservationAsync(
            ctx.TerrainId, dateReservation, ctx.MembreMatricule, estPrive: true);

        try
        {
            // Vérifie CF-RV-003 : un même créneau/terrain ne peut être réservé deux fois
            await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
                () => matchRepository.CreerReservationAsync(
                    ctx.TerrainId, dateReservation, ctx.MembreMatricule, estPrive: true));
        }
        finally
        {
            await NettoyerContexteAsync(context, ctx, matchId);
        }
    }

    [Fact]
    public async Task InscrireJoueurAsync_QuatriemeJoueur_ConfirmeLeMatchViaLeTrigger()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var terrainRepository = new TerrainRepository(context);
        var membreRepository = new MembreRepository(context);
        var matchRepository = new MatchRepository(context);
        var paiementRepository = new PaiementRepository(context);

        var ctx = await ConstruireContexteAsync(context, siteRepository, terrainRepository, membreRepository);
        var dateReservation = DateTime.Today.AddDays(1).AddHours(11);
        var matchId = await matchRepository.CreerReservationAsync(
            ctx.TerrainId, dateReservation, ctx.MembreMatricule, estPrive: true);

        var membresSupplementaires = new List<string>();

        try
        {
            // L'organisateur est déjà inscrit (1/4) via CreerReservationAsync.
            // On ajoute 3 joueurs de plus, chacun payé, pour atteindre 4/4 et
            // vérifier que TR_Paiements_CheckMatchComplet (Issue #15) confirme
            // réellement le match — test de bout en bout Repository + trigger SQL.
            for (int i = 0; i < 3; i++)
            {
                var matriculeJoueur = $"L{Guid.NewGuid().ToString("N")[..8]}";
                await membreRepository.CreerAsync(matriculeJoueur, "Libre", null);
                membresSupplementaires.Add(matriculeJoueur);

                var inscriptionId = await matchRepository.InscrireJoueurAsync(
                    matchId, matriculeJoueur, ctx.MembreMatricule);

                await paiementRepository.TraiterPaiementAsync(inscriptionId, 15.00m);
            }

            // Il faut aussi payer l'inscription de l'organisateur pour atteindre 4 payés
            // (rappel : CreerReservationAsync inscrit l'organisateur mais ne le paie pas).
            // Récupération via procédure dédiée, jamais un accès EF Core direct à la table
            // (interdit par l'option B — cf. SP_SelectInscriptionId_TestOnly).
            var inscriptionIdOrganisateur = (await context.Database
                .SqlQuery<int>($"EXEC sch_Padel.SP_SelectInscriptionId_TestOnly @MatchId = {matchId}, @MembreMatricule = {ctx.MembreMatricule}")
                .ToListAsync())
                .Single();

            await paiementRepository.TraiterPaiementAsync(inscriptionIdOrganisateur, 15.00m);

            var match = await matchRepository.ObtenirParIdAsync(matchId);
            Assert.Equal(Padel.Domain.Enums.StatutMatch.Complet, match!.Statut);
        }
        finally
        {
            // Ordre important : supprimer le match (et ses inscriptions liées) AVANT
            // les membres, sinon FK_InscriptionsMatch_Membres bloque la suppression
            // d'un membre encore référencé par une inscription existante.
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMatch_TestOnly @MatchId = {matchId}");

            foreach (var matricule in membresSupplementaires)
            {
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {matricule}");
            }

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {ctx.MembreMatricule}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerTerrain_TestOnly @TerrainId = {ctx.TerrainId}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerHoraireSite_TestOnly @SiteId = {ctx.SiteId}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {ctx.SiteId}");
        }
    }
}