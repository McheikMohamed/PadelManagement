using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class MembreRepositoryTests
{
    private readonly DatabaseFixture _fixture = new();

    [Fact]
    public async Task CreerAsync_MembreGlobal_InsereReellementEnBase()
    {
        using var context = _fixture.CreerContext();
        var repository = new MembreRepository(context);

        var matricule = $"G{Guid.NewGuid().ToString("N")[..8]}";

        await repository.CreerAsync(matricule, "Global", null);

        try
        {
            var membre = await repository.ObtenirParMatriculeAsync(matricule);

            Assert.NotNull(membre);
            Assert.Equal("Global", membre!.Type.ToString());
            Assert.Null(membre.SiteId);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {matricule}");
        }
    }

    [Fact]
    public async Task CreerAsync_TypeSiteSansSiteId_LanceSqlException()
    {
        using var context = _fixture.CreerContext();
        var repository = new MembreRepository(context);

        var matricule = $"S{Guid.NewGuid().ToString("N")[..8]}";

        // Vérifie la vraie contrainte CK_Membres_Type_SiteId (Issue #9)
        await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
            () => repository.CreerAsync(matricule, "Site", null));
    }

    [Fact]
    public async Task CreerAsync_MatriculeAvecPrefixeA_LanceSqlException()
    {
        using var context = _fixture.CreerContext();
        var repository = new MembreRepository(context);

        // Vérifie la vraie contrainte CK_Membres_MatriculeSansPrefixeA (CF-RS-037)
        var matriculeInvalide = $"A{Guid.NewGuid().ToString("N")[..8]}";

        await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
            () => repository.CreerAsync(matriculeInvalide, "Global", null));
    }

    [Fact]
    public async Task ObtenirParMatriculeAsync_MatriculeInexistant_RetourneNull()
    {
        using var context = _fixture.CreerContext();
        var repository = new MembreRepository(context);

        var resultat = await repository.ObtenirParMatriculeAsync("ZINEXISTANT");

        Assert.Null(resultat);
    }

    [Fact]
    public async Task ListerAsync_SiteIdNull_RetourneTousLesMembres()
    {
        using var context = _fixture.CreerContext();
        var repository = new MembreRepository(context);

        var matricule1 = $"G{Guid.NewGuid().ToString("N")[..8]}";
        var matricule2 = $"L{Guid.NewGuid().ToString("N")[..8]}";

        await repository.CreerAsync(matricule1, "Global", null);
        await repository.CreerAsync(matricule2, "Libre", null);

        try
        {
            var membres = await repository.ListerAsync(null);

            Assert.Contains(membres, m => m.Matricule == matricule1);
            Assert.Contains(membres, m => m.Matricule == matricule2);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {matricule1}");
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerMembre_TestOnly @Matricule = {matricule2}");
        }
    }
}