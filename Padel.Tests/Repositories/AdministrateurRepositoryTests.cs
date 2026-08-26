using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure.Repositories;
using Padel.Tests.TestFixtures;
using Xunit;

namespace Padel.Tests.Repositories;

public class AdministrateurRepositoryTests
{
    private readonly DatabaseFixture _fixture = new();

    [Fact]
    public async Task CreerAsync_AdminGlobal_InsereReellementEnBase()
    {
        using var context = _fixture.CreerContext();
        var repository = new AdministrateurRepository(context);

        var matricule = $"AG{Guid.NewGuid().ToString("N")[..7]}";

        await repository.CreerAsync(matricule, "Global", null);

        try
        {
            var admin = await repository.ObtenirParMatriculeAsync(matricule);

            Assert.NotNull(admin);
            Assert.Equal("Global", admin!.Type.ToString());
            Assert.Null(admin.SiteId);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sch_Padel.SP_SupprimerAdministrateur_TestOnly @Matricule = {matricule}");
        }
    }

    [Fact]
    public async Task CreerAsync_TypeSiteSansSiteId_LanceSqlException()
    {
        using var context = _fixture.CreerContext();
        var repository = new AdministrateurRepository(context);

        var matricule = $"AS{Guid.NewGuid().ToString("N")[..7]}";

        // Vérifie la vraie contrainte CK_Administrateurs_Type_SiteId (Issue #10)
        await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
            () => repository.CreerAsync(matricule, "Site", null));
    }

    [Fact]
    public async Task CreerAsync_SansPrefixeA_LanceSqlException()
    {
        using var context = _fixture.CreerContext();
        var repository = new AdministrateurRepository(context);

        // Vérifie la vraie contrainte CK_Administrateurs_MatriculePrefixeA (CF-RS-037)
        var matriculeInvalide = $"G{Guid.NewGuid().ToString("N")[..8]}";

        await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
            () => repository.CreerAsync(matriculeInvalide, "Global", null));
    }

    [Fact]
    public async Task ObtenirParMatriculeAsync_MatriculeInexistant_RetourneNull()
    {
        using var context = _fixture.CreerContext();
        var repository = new AdministrateurRepository(context);

        var resultat = await repository.ObtenirParMatriculeAsync("AZINEXISTANT");

        Assert.Null(resultat);
    }
}