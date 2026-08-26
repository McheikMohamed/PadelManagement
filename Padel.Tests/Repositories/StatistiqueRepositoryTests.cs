using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class StatistiqueRepositoryTests
{
    private readonly DatabaseFixture _fixture = new();

    [Fact]
    public async Task SelectImpayesAsync_MembreAvecSoldeDu_ApparaitDansLaListeEtLeTotal()
    {
        using var context = _fixture.CreerContext();
        var membreRepository = new MembreRepository(context);
        var statistiqueRepository = new StatistiqueRepository(context);

        var matricule = $"G{Guid.NewGuid().ToString("N")[..8]}";
        await membreRepository.CreerAsync(matricule, "Global", null);

        // Pas de méthode Repository pour mettre à jour SoldeDu directement (aucun
        // Service n'en a eu besoin jusqu'ici) : on passe par une petite procédure
        // de test dédiée plutôt qu'un accès direct interdit par l'option B.
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_DefinirSoldeDu_TestOnly @Matricule = {matricule}, @SoldeDu = 30.00");

        try
        {
            var (impayes, total) = await statistiqueRepository.SelectImpayesAsync(null);

            Assert.Contains(impayes, i => i.Matricule == matricule && i.SoldeDu == 30.00m);
            Assert.True(total >= 30.00m);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {matricule}");
        }
    }

    [Fact]
    public async Task SelectImpayesAsync_AucunImpaye_RetourneListeVideEtTotalZero()
    {
        using var context = _fixture.CreerContext();
        var statistiqueRepository = new StatistiqueRepository(context);

        // Site fictif garanti sans membre en défaut, pour un total exactement à 0.
        var (impayes, total) = await statistiqueRepository.SelectImpayesAsync(999999);

        Assert.Empty(impayes);
        Assert.Equal(0m, total);
    }

    [Fact]
    public async Task SelectPenalitesActivesAsync_MembreAvecPenalite_ApparaitDansLaListe()
    {
        using var context = _fixture.CreerContext();
        var membreRepository = new MembreRepository(context);
        var statistiqueRepository = new StatistiqueRepository(context);

        var matricule = $"L{Guid.NewGuid().ToString("N")[..8]}";
        await membreRepository.CreerAsync(matricule, "Libre", null);

        var datePenalite = DateTime.Today.AddDays(5);
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sch_Padel.SP_DefinirPenalite_TestOnly @Matricule = {matricule}, @DatePenalite = {datePenalite}");

        try
        {
            var penalites = await statistiqueRepository.SelectPenalitesActivesAsync(null);

            Assert.Contains(penalites, p => p.Matricule == matricule);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {matricule}");
        }
    }

    [Fact]
    public async Task SelectStatistiquesMatchesAsync_SitesSansMatches_RetourneZeros()
    {
        using var context = _fixture.CreerContext();
        var statistiqueRepository = new StatistiqueRepository(context);

        var resultat = await statistiqueRepository.SelectStatistiquesMatchesAsync(
            999999, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

        Assert.Equal(0, resultat.TotalMatches);
    }

    [Fact]
    public async Task SelectChiffreAffairesAsync_SiteSansPaiement_RetourneListeVide()
    {
        using var context = _fixture.CreerContext();
        var siteRepository = new SiteRepository(context);
        var statistiqueRepository = new StatistiqueRepository(context);

        var siteId = await siteRepository.CreerAsync($"Site Test {Guid.NewGuid()}");

        try
        {
            var resultat = await statistiqueRepository.SelectChiffreAffairesAsync(
                siteId, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(1)));

            // Aucun paiement lié à ce site tout neuf : la procédure ne retourne
            // aucune ligne (JOIN vide), pas une ligne à 0€ — comportement à connaître.
            Assert.Empty(resultat);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerSite_TestOnly @SiteId = {siteId}");
        }
    }
}